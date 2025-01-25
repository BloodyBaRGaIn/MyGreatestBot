using SharedClasses;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace FfmpegUpdater
{
    [SupportedOSPlatform("windows")]
    internal static class Program
    {
        private const string FfmpegExtension = "exe";
        private const string FfmpegDirKey = "FfmpegDir";
        private const string FfmpegFileNameKey = "FfmpegFileName";
        private const string FfmpegRepoFileNameKey = "FfmpegRepoFileName";

        private static int Exit(int exitCode)
        {
            Console.WriteLine("Press any key to exit...");
            _ = Console.ReadKey(true);
            Environment.Exit(exitCode);
            return exitCode;
        }

        private static int Main()
        {
            // get properties
            if (!BuildPropsProvider.GetProperties(out Dictionary<string, string> PropertiesDictionary))
            {
                Console.Error.WriteLine($"Cannot get properties{Environment.NewLine}" +
                    $"{GetExceptionMessage(BuildPropsProvider.LastError)}");
                return Exit(1);
            }

            if (!PropertiesDictionary.TryGetValue(FfmpegDirKey, out string? ffmpeg_directory_name)
                || string.IsNullOrWhiteSpace(ffmpeg_directory_name))
            {
                Console.Error.WriteLine($"Cannot get {FfmpegDirKey} value");
                return Exit(1);
            }

            if (!PropertiesDictionary.TryGetValue(FfmpegFileNameKey, out string? ffmpeg_file_name)
                || string.IsNullOrWhiteSpace(ffmpeg_file_name))
            {
                Console.Error.WriteLine($"Cannot get {FfmpegFileNameKey} value");
                return Exit(1);
            }

            if (!PropertiesDictionary.TryGetValue(FfmpegRepoFileNameKey, out string? ffmpeg_repo_file_name)
                || string.IsNullOrWhiteSpace(ffmpeg_repo_file_name))
            {
                Console.Error.WriteLine($"Cannot get {FfmpegRepoFileNameKey} value");
                return Exit(1);
            }

            string ffmpeg_executable_name = $"{ffmpeg_file_name}.{FfmpegExtension}";
            string ffmpeg_backup_name = $"{ffmpeg_file_name}_old.{FfmpegExtension}";

            // get solution path
            DirectoryInfo? current_directory_info;

            try
            {
                current_directory_info = new(Directory.GetCurrentDirectory());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Cannot get current directory info{Environment.NewLine}" +
                    $"{GetExceptionMessage(ex)}");
                return Exit(1);
            }

            while (current_directory_info != null && current_directory_info.GetFiles("*.sln").Length == 0)
            {
                current_directory_info = current_directory_info.Parent;
            }

            if (current_directory_info == null)
            {
                Console.Error.WriteLine("Cannot find solution directory");
                return Exit(1);
            }

            // find ffmpeg directory
            DirectoryInfo? ffmpeg_directory = current_directory_info
                .GetDirectories(ffmpeg_directory_name)
                .FirstOrDefault();

            if (ffmpeg_directory == null)
            {
                Console.Error.WriteLine($"Cannot find {ffmpeg_directory_name} directory");
                return Exit(1);
            }

            // find download link file
            FileInfo? repo_link_info = ffmpeg_directory
                .GetFiles(ffmpeg_repo_file_name)
                .FirstOrDefault();

            if (repo_link_info == null)
            {
                Console.Error.WriteLine($"Cannot find {ffmpeg_repo_file_name}");
                return Exit(1);
            }

            // get repo link file content
            string repo_link_file_content;

            try
            {
                repo_link_file_content = File.ReadAllText(repo_link_info.FullName).TrimEnd([.. Environment.NewLine]);

                if (string.IsNullOrWhiteSpace(repo_link_file_content))
                {
                    throw new Exception("File is empty");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Cannot read link file{Environment.NewLine}" +
                    $"{GetExceptionMessage(ex)}");
                return Exit(1);
            }

            string[] repo_file_split = repo_link_file_content.Split([.. Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
            if (repo_file_split.Length != 2)
            {
                Console.Error.WriteLine("Repo file is invalid");
                return Exit(1);
            }

            string repo_link = repo_file_split[0];

            // get repo latest tag
            string repo_tag;

            try
            {
                Process? git = Process.Start(new ProcessStartInfo("cmd.exe")
                {
                    Arguments = $"/C git ls-remote --refs --heads --tags {repo_link}",
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false
                });
                if (git == null)
                {
                    throw new ArgumentNullException(nameof(git));
                }
                if (!git.WaitForExit(10000))
                {
                    git.Kill();
                    throw new InvalidOperationException(nameof(git));
                }
                string output = git.StandardOutput.ReadToEnd();
                string error = git.StandardError.ReadToEnd();
                if (!string.IsNullOrWhiteSpace(error))
                {
                    Console.Error.WriteLine($"{nameof(git)} has error{Environment.NewLine}{error}");
                }
                if (string.IsNullOrWhiteSpace(output))
                {
                    throw new InvalidOperationException(nameof(output));
                }
                repo_tag = output
                    .TrimEnd([.. Environment.NewLine])
                    .Split([.. Environment.NewLine], StringSplitOptions.RemoveEmptyEntries)[^1];
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Cannot get tag{Environment.NewLine}" +
                    $"{GetExceptionMessage(ex)}");
                return Exit(1);
            }

            string[] split_tag = repo_tag.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

            if (split_tag.Length != 2)
            {
                Console.Error.WriteLine("Tag is invalid");
                return Exit(1);
            }

            string hash_commit = split_tag[0];
            string tag_name = split_tag[1];
            tag_name = tag_name[(tag_name.LastIndexOf('/') + 1)..];

            Console.WriteLine($"Latest commit hash: {hash_commit}");
            Console.WriteLine($"Latest tag: {tag_name}");

            string target_zip_file_name = repo_file_split[1];

            string zip_link =
                $"{repo_link[..repo_link.LastIndexOf('.')]}/releases/download/{tag_name}/{target_zip_file_name}";

            // download ffmpeg zip file
            string target_zip_path = Path.Combine(ffmpeg_directory.FullName, target_zip_file_name);

            Console.WriteLine($"Downloading file {zip_link} to {target_zip_path}");

            {
                static void PrintProgress(long value, int pad)
                {
                    Console.Write($"{value.ToString().PadLeft(pad)}");
                }

                static void ClearProgress(int pad)
                {
                    for (int i = 0; i < pad; i++)
                    {
                        Console.Write('\b');
                    }
                }

                using Semaphore progressSemaphore = new(1, 1);
                long old_progress = 0;
                Progress<long> downloadProgress = new();
                Console.Write("Progress: ");
                PrintProgress(old_progress, 3);
                downloadProgress.ProgressChanged += (sender, progress) =>
                {
                    if (progress == old_progress)
                    {
                        return;
                    }
                    _ = progressSemaphore.WaitOne();
                    ClearProgress(3);
                    PrintProgress(progress, 3);
                    old_progress = progress;
                    _ = progressSemaphore.Release();
                };

                try
                {
                    using HttpClient httpClient = new();
                    httpClient.Timeout = TimeSpan.FromMinutes(5);
                    using FileStream fileStream = new(target_zip_path, FileMode.Create, FileAccess.Write, FileShare.None);
                    using CancellationTokenSource cts = new();
                    using Task task = httpClient.DownloadDataAsync(zip_link, fileStream, downloadProgress, cts.Token);
                    cts.CancelAfter(TimeSpan.FromMinutes(5));
                    try
                    {
                        task.Wait();
                    }
                    catch { }
                    if (!task.IsCompletedSuccessfully)
                    {
                        throw new TimeoutException("Cannot download");
                    }
                    Console.WriteLine();
                }
                catch (Exception ex)
                {
                    Console.WriteLine();
                    Console.Error.WriteLine(
                        $"Cannot download file{Environment.NewLine}" +
                        $"{GetExceptionMessage(ex)}");
                    return Exit(1);
                }
            }

            ZipArchive archive;

            try
            {
                archive = ZipFile.OpenRead(target_zip_path);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Cannot open archive{Environment.NewLine}" +
                    $"{GetExceptionMessage(ex)}");
                return Exit(1);
            }

            ZipArchiveEntry? entry = archive.Entries.FirstOrDefault(entry => entry.FullName.EndsWith(ffmpeg_executable_name));

            if (entry == null)
            {
                Console.Error.WriteLine($"Archive not containing {ffmpeg_executable_name}");
                return Exit(1);
            }

            string destination_file_path = Path.Combine(ffmpeg_directory.FullName, ffmpeg_executable_name);
            string ffmpeg_old_path = Path.Combine(ffmpeg_directory.FullName, ffmpeg_backup_name);

            try
            {
                File.Delete(ffmpeg_old_path);
            }
            catch { }

            try
            {
                File.Move(
                    destination_file_path,
                    ffmpeg_old_path);
            }
            catch { }

            Console.WriteLine($"Extracting file {ffmpeg_executable_name}");

            try
            {
                entry.ExtractToFile(destination_file_path, true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Cannot extract file{Environment.NewLine}" +
                    $"{GetExceptionMessage(ex)}");
                return Exit(1);
            }

            archive.Dispose();

            try
            {
                File.Delete(target_zip_path);
            }
            catch { }

            Console.WriteLine("FFMPEG updated successfully!");

            return Exit(0);
        }

        private static string GetExceptionMessage(Exception? ex)
        {
            return string.Join(" : ",
                ex?.GetType()?.Name,
                string.IsNullOrWhiteSpace(ex?.Message)
                    ? "No message provided"
                    : ex.Message);
        }
    }
}
