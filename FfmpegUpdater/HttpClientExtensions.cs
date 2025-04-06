using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FfmpegUpdater
{
    public static class HttpClientExtensions
    {
        public static async Task DownloadDataAsync(this HttpClient client,
                                                   string requestUrl,
                                                   Stream destination,
                                                   IProgress<string> progress,
                                                   CancellationToken cancellationToken = default)
        {
            using HttpResponseMessage response = await client.GetAsync(requestUrl,
                                                                       HttpCompletionOption.ResponseHeadersRead,
                                                                       cancellationToken);

            long? contentLength = response.Content.Headers.ContentLength;
            using Stream download = await response.Content.ReadAsStreamAsync(cancellationToken);

            if (progress is null || !contentLength.HasValue)
            {
                await download.CopyToAsync(destination, cancellationToken);
                return;
            }

            Progress<long> progressWrapper = new(totalBytes =>
            {
                long maxBytes = contentLength.Value;
                float precent = totalBytes * 100.0f / maxBytes;
                progress.Report($"{totalBytes} / {maxBytes} ({precent:0.0}%)");
            });

            byte[] buffer = new byte[client.MaxResponseContentBufferSize];
            if (buffer == null || buffer.Length == 0)
            {
                throw new InvalidOperationException($"Cannot allocate memory for {buffer}.");
            }

            await download.CopyToAsync(destination, buffer.AsMemory(), progressWrapper, cancellationToken);
        }

        public static async Task CopyToAsync(this Stream source,
                                             Stream destination,
                                             Memory<byte> buffer,
                                             IProgress<long>? progress = null,
                                             CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(source);
            ArgumentNullException.ThrowIfNull(destination);

            if (!source.CanRead)
            {
                throw new InvalidOperationException($"'{nameof(source)}' is not readable.");
            }
            if (!destination.CanWrite)
            {
                throw new InvalidOperationException($"'{nameof(destination)}' is not writable.");
            }

            long totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = await source.ReadAsync(buffer, cancellationToken).ConfigureAwait(false)) != 0)
            {
                await destination.WriteAsync(buffer[..bytesRead], cancellationToken).ConfigureAwait(false);
                totalBytesRead += bytesRead;
                progress?.Report(totalBytesRead);
                if (cancellationToken.IsCancellationRequested)
                {
                    break;
                }
            }
        }
    }
}
