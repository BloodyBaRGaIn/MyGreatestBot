using System;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace FfmpegUpdater
{
    public static class HttpClientExtensions
    {
        public static async Task<long> GetDownloadSize(this HttpClient client,
                                                       string requestUrl,
                                                       CancellationToken cancellationToken = default)
        {
            using HttpResponseMessage response = await client.GetAsync(requestUrl,
                                                                       HttpCompletionOption.ResponseHeadersRead,
                                                                       cancellationToken);

            long? contentLength = response.Content.Headers.ContentLength;

            return contentLength ?? 0;
        }

        public static async Task DownloadDataAsync(this HttpClient client,
                                                   string requestUrl,
                                                   Stream destination,
                                                   IProgress<long> progress,
                                                   CancellationToken cancellationToken = default)
        {
            using HttpResponseMessage response =
                await client.GetAsync(requestUrl,
                                      HttpCompletionOption.ResponseHeadersRead,
                                      cancellationToken);

            long? contentLength = response.Content.Headers.ContentLength;
            using Stream download = await response.Content.ReadAsStreamAsync(cancellationToken);

            if (progress is null || !contentLength.HasValue)
            {
                await download.CopyToAsync(destination, cancellationToken);
                return;
            }

            byte[] buffer = new byte[client.MaxResponseContentBufferSize];
            if (buffer == null || buffer.Length == 0)
            {
                throw new InvalidOperationException(
                    $"Cannot allocate memory for {buffer} with size of {client.MaxResponseContentBufferSize} byte(s).");
            }

            await download.CopyToAsync(destination, buffer.AsMemory(), progress, cancellationToken);
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
