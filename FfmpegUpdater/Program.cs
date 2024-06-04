using System.IO.Compression;

namespace FfmpegUpdater
{
    internal static class Program
    {
        private const string ffmpeg_directory_name = "ffmpeg_binaries";
        private const string ffmpeg_download_link_file_name = "ffmpeg_download_link.txt";
        private const string ffmpeg_archive_name = "ffmpeg-master-latest-win64-gpl.zip";
        private const string ffmpeg_executable_name = "ffmpeg.exe";
        private const string ffmpeg_backup_name = "ffmpeg_old.exe";

        private static int Main()
        {
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
                return 1;
            }

            while (current_directory_info != null && current_directory_info.GetFiles("*.sln").Length == 0)
            {
                current_directory_info = current_directory_info.Parent;
            }

            if (current_directory_info == null)
            {
                Console.Error.WriteLine("Cannot find solution directory");
                return 1;
            }

            // find ffmpeg directory
            DirectoryInfo? ffmpeg_directory = current_directory_info
                .GetDirectories(ffmpeg_directory_name)
                .FirstOrDefault();

            if (ffmpeg_directory == null)
            {
                Console.Error.WriteLine($"Cannot find {ffmpeg_directory_name} directory");
                return 1;
            }

            // find download link file
            FileInfo? download_link_info = ffmpeg_directory
                .GetFiles(ffmpeg_download_link_file_name)
                .FirstOrDefault();

            if (download_link_info == null)
            {
                Console.Error.WriteLine($"Cannot find {ffmpeg_download_link_file_name}");
                return 1;
            }

            // get download link
            string download_link;

            try
            {
                download_link = File.ReadAllText(download_link_info.FullName).TrimEnd([.. Environment.NewLine]);

                if (string.IsNullOrWhiteSpace(download_link))
                {
                    throw new Exception("File is empty");
                }
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Cannot read link file{Environment.NewLine}" +
                    $"{GetExceptionMessage(ex)}");
                return 1;
            }

            // download ffmpeg zip file
            string target_zip_path = Path.Combine(ffmpeg_directory.FullName, ffmpeg_archive_name);

            try
            {
                using HttpClient httpClient = new();
                using Task<Stream> streamTask = httpClient.GetStreamAsync(download_link);
                streamTask.Wait();
                using FileStream fileStream = new(target_zip_path,
                    FileMode.OpenOrCreate);
                streamTask.Result.CopyTo(fileStream);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Cannot download file{Environment.NewLine}" +
                    $"{GetExceptionMessage(ex)}");
                return 1;
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
                return 1;
            }

            ZipArchiveEntry? entry = archive.Entries.FirstOrDefault(entry => entry.FullName.EndsWith(ffmpeg_executable_name));

            if (entry == null)
            {
                Console.Error.WriteLine($"Archive not containing {ffmpeg_executable_name}");
                return 1;
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

            try
            {
                entry.ExtractToFile(destination_file_path, true);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(
                    $"Cannot extract file{Environment.NewLine}" +
                    $"{GetExceptionMessage(ex)}");
                return 1;
            }

            archive.Dispose();

            try
            {
                File.Delete(target_zip_path);
            }
            catch { }

            Console.WriteLine("FFMPEG updated successfully!");

            return 0;
        }

        private static string GetExceptionMessage(Exception ex)
        {
            return $"{ex.GetType().Name} : {(string.IsNullOrWhiteSpace(ex.Message) ? "No message provided" : ex.Message)}";
        }
    }
}
