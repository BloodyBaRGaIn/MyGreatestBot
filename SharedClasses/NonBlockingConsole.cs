using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;

namespace SharedClasses
{
    public class NonBlockingConsole() : IDisposable
    {
        private readonly Queue<string> inputStrings = new();
        private Task? task;
        private Task<string?>? readTask;
        private readonly CancellationTokenSource cts = new();
        private bool disposed;

        public void Start()
        {
            task ??= Task.Run(InputHandler, cts.Token);
        }

        public bool DequeueString([MaybeNullWhen(false)] out string result)
        {
            return inputStrings.TryDequeue(out result);
        }

        private void InputHandler()
        {
            while (true)
            {
                if (cts.IsCancellationRequested)
                {
                    break;
                }

                try
                {
                    _ = Task.Delay(1);
                }
                catch
                {
                    return;
                }

                string? inputStr;
                try
                {
                    inputStr = ReadLineAsync(cts.Token).Result;
                }
                catch
                {
                    return;
                }

                if (!string.IsNullOrWhiteSpace(inputStr))
                {
                    inputStrings.Enqueue(inputStr);
                }
            }
        }

        private async Task<string?> ReadLineAsync(CancellationToken cancellationToken = default)
        {
            readTask ??= Task.Run(Console.ReadLine);

            _ = await Task.WhenAny(readTask, Task.Delay(-1, cancellationToken));

            cancellationToken.ThrowIfCancellationRequested();

            string? result = await readTask;
            readTask = null;

            return result;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        private void Dispose(bool disposing)
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            if (disposing)
            {
                ;
            }

            cts.Cancel();
            try
            {
                _ = readTask?.Wait(100);
            }
            catch { }

            try
            {
                _ = task?.Wait(100);
            }
            catch { }
            finally
            {
                cts.Dispose();
            }

            try
            {
                task?.Dispose();
            }
            catch { }
        }
    }
}
