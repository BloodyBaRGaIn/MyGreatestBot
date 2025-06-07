using SharedClasses;
using ShellProgressBar;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Text;
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

        private static readonly CancellationTokenSource cts = new();

        private static void Main()
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CancelKeyPress += Console_CancelKeyPress;

            // get properties
            if (!BuildPropsProvider.GetProperties(out Dictionary<string, string> PropertiesDictionary))
            {
                Console.Error.WriteLine($"Cannot get properties{Environment.NewLine}" +
                    $"{GetExceptionMessage(BuildPropsProvider.LastError)}");
                Exit(1);
            }

            GetPropValue(PropertiesDictionary, FfmpegDirKey, out string ffmpeg_directory_name);

            GetPropValue(PropertiesDictionary, FfmpegFileNameKey, out string ffmpeg_file_name);

            GetPropValue(PropertiesDictionary, FfmpegRepoFileNameKey, out string ffmpeg_repo_file_name);

            string ffmpeg_executable_name = $"{ffmpeg_file_name}.{FfmpegExtension}";
            string ffmpeg_backup_name = $"{ffmpeg_file_name}_old.{FfmpegExtension}";

            // get solution path
            DirectoryInfo? current_directory_info = null;

            try
            {
                current_directory_info = new(Directory.GetCurrentDirectory());
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Cannot get current directory info{Environment.NewLine}" +
                    $"{GetExceptionMessage(ex)}");
                Exit(1);
            }

            while (current_directory_info != null && current_directory_info.GetFiles("*.sln").Length == 0)
            {
                current_directory_info = current_directory_info.Parent;
            }

            if (current_directory_info == null)
            {
                Console.Error.WriteLine("Cannot find solution directory");
                Exit(1);
            }

            // find ffmpeg directory
            DirectoryInfo? ffmpeg_directory = current_directory_info
                .GetDirectories(ffmpeg_directory_name)
                .FirstOrDefault();

            if (ffmpeg_directory == null)
            {
                Console.Error.WriteLine($"Cannot find {ffmpeg_directory_name} directory");
                Exit(1);
            }

            // find download link file
            FileInfo? repo_link_info = ffmpeg_directory
                .GetFiles(ffmpeg_repo_file_name)
                .FirstOrDefault();

            if (repo_link_info == null)
            {
                Console.Error.WriteLine($"Cannot find {ffmpeg_repo_file_name}");
                Exit(1);
            }

            // get repo link file content
            string repo_link_file_content = string.Empty;

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
                Exit(1);
            }

            string[] repo_file_split = repo_link_file_content.Split([.. Environment.NewLine], StringSplitOptions.RemoveEmptyEntries);
            if (repo_file_split.Length != 2)
            {
                Console.Error.WriteLine("Repo file is invalid");
                Exit(1);
            }

            string repo_link = repo_file_split[0];

            // get repo latest tag
            string repo_tag = string.Empty;

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

                ArgumentNullException.ThrowIfNull(git);

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
                    throw new InvalidOperationException($"{nameof(git)} has no output");
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
                Exit(1);
            }

            string[] split_tag = repo_tag.Split([' ', '\t'], StringSplitOptions.RemoveEmptyEntries);

            if (split_tag.Length != 2)
            {
                Console.Error.WriteLine("Tag is invalid");
                Exit(1);
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

            if (string.IsNullOrWhiteSpace(target_zip_path))
            {
                Console.Error.WriteLine("Zip file path is empty");
                Exit(1);
            }

            {
                HttpClient downloadHttpClient = new()
                {
                    Timeout = TimeSpan.FromMinutes(5),
                    MaxResponseContentBufferSize = 0x10000
                };

                long download_total_bytes = 0;

                Console.WriteLine(
                    $"Download URI:{Environment.NewLine}" +
                    $"{zip_link}");
                Console.WriteLine(
                    $"The file will be saved as:{Environment.NewLine}" +
                    $"{target_zip_path}");

                Console.WriteLine();

                try
                {
                    using Task<long> getSizeTask = downloadHttpClient.GetDownloadSize(zip_link, cts.Token);
                    getSizeTask.Wait();
                    download_total_bytes = getSizeTask.Result;
                    if (download_total_bytes == 0)
                    {
                        throw new Exception("Invalid or empty responce");
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine(
                        $"Cannot get download size{Environment.NewLine}" +
                        $"{GetExceptionMessage(ex)}");
                    Exit(1);
                }

                using ProgressBar progressBar = new(
                    10000,
                    "Downloading file",
                    new ProgressBarOptions()
                    {
                        ForegroundColor = ConsoleColor.Yellow,
                        ForegroundColorDone = ConsoleColor.DarkGreen,
                        ForegroundColorError = ConsoleColor.Red,
                        BackgroundColor = ConsoleColor.DarkGray,
                        BackgroundCharacter = '\u2593',
                        CollapseWhenFinished = false,
                        ProgressBarOnBottom = true,
                        ShowEstimatedDuration = true,
                        EnableTaskBarProgress = true,
                    });

                DateTime now = DateTime.Now;

                using Semaphore progressSemaphore = new(1, 1);
                long old_progress_bytes = new();
                Progress<long> downloadProgress = new();

                downloadProgress.ProgressChanged += (sender, progress_bytes) =>
                {
                    if (progress_bytes == old_progress_bytes)
                    {
                        return;
                    }

                    _ = progressSemaphore.WaitOne();

                    if (progress_bytes != 0)
                    {
                        TimeSpan timeTaken = DateTime.Now - now;
                        long bytesLeft = download_total_bytes - progress_bytes;
                        progressBar.EstimatedDuration = timeTaken * bytesLeft / progress_bytes;
                    }
                    IProgress<float> float_progress = progressBar.AsProgress<float>();
                    float_progress.Report((float)progress_bytes / download_total_bytes);

                    old_progress_bytes = progress_bytes;

                    _ = progressSemaphore.Release();
                };

                FileStream? fileStream = null;
                Exception? exception = null;

                try
                {
                    fileStream = new(
                        target_zip_path, FileMode.Create, FileAccess.Write, FileShare.ReadWrite);
                    using Task task = downloadHttpClient.DownloadDataAsync(
                        zip_link, fileStream, downloadProgress, cts.Token);

                    try
                    {
                        task.Wait();
                    }
                    catch { }

                    if (task.IsCanceled)
                    {
                        throw new OperationCanceledException("Download cancelled");
                    }
                    if (!task.IsCompletedSuccessfully)
                    {
                        throw new TimeoutException("Download failed");
                    }
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
                finally
                {
                    downloadHttpClient.Dispose();

                    try
                    {
                        if (exception != null)
                        {
                            progressBar.ObservedError = true;
                        }
                        progressBar.Dispose();
                    }
                    catch { }

                    try
                    {
                        fileStream?.Close();
                        fileStream?.Dispose();
                    }
                    catch { }
                }

                Console.WriteLine();

                if (exception != null)
                {
                    Console.Error.WriteLine(GetExceptionMessage(exception));
                    Exit(1);
                }
            }

            ZipArchive? archive = null;

            try
            {
                archive = ZipFile.OpenRead(target_zip_path);

                ArgumentNullException.ThrowIfNull(archive);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Cannot open archive{Environment.NewLine}" +
                    $"{GetExceptionMessage(ex)}");
                Exit(1);
            }

            ZipArchiveEntry? entry = archive.Entries.FirstOrDefault(entry => entry.FullName.EndsWith(ffmpeg_executable_name));

            if (entry == null)
            {
                Console.Error.WriteLine($"Archive not containing {ffmpeg_executable_name}");
                Exit(1);
            }

            string destination_file_path = Path.Combine(ffmpeg_directory.FullName, ffmpeg_executable_name);
            string ffmpeg_old_path = Path.Combine(ffmpeg_directory.FullName, ffmpeg_backup_name);

            if (string.IsNullOrWhiteSpace(destination_file_path))
            {
                Console.Error.WriteLine("Destination file path is empty");
                Exit(1);
            }

            if (string.IsNullOrWhiteSpace(ffmpeg_old_path))
            {
                Console.Error.WriteLine("Old file path is empty");
                Exit(1);
            }

            try
            {
                File.Copy(
                    destination_file_path,
                    ffmpeg_old_path, true);
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
                Exit(1);
            }

            archive.Dispose();

            try
            {
                File.Delete(target_zip_path);
            }
            catch { }

            Console.WriteLine("FFMPEG updated successfully!");

            Exit(0);
        }

        private static void Console_CancelKeyPress(object? sender, ConsoleCancelEventArgs e)
        {
            _ = sender;
            e.Cancel = true;

            try
            {
                cts.Cancel();
            }
            catch { }
        }


        [DoesNotReturn]
        private static void Exit(int exitCode)
        {
            try
            {
                cts.Dispose();
            }
            catch { }

            Console.WriteLine("Press any key to exit...");
            _ = Console.ReadKey(true);
            Environment.Exit(exitCode);

            while (true)
            {
                try
                {
                    Task.Delay(1).Wait();
                }
                catch { }
            }
        }

        private static void GetPropValue(Dictionary<string, string> dictionary, string name, out string value)
        {
            if (!dictionary.TryGetValue(name, out string? tmp_value)
                || string.IsNullOrWhiteSpace(tmp_value))
            {
                Console.Error.WriteLine($"Cannot get {name} value");
                Exit(1);
            }

            value = tmp_value;
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
