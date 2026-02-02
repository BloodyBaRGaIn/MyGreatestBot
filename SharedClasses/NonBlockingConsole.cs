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
            Thread.CurrentThread.Name = nameof(NonBlockingConsole);

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

                string? inputStr = null;
                try
                {
                    Task<string?> readTask = Console.In.ReadLineAsync(cts.Token).AsTask();
                    readTask.Wait();
                    cts.Token.ThrowIfCancellationRequested();
                    if (readTask.IsCompletedSuccessfully)
                    {
                        inputStr = readTask.Result;
                    }
                }
                catch (OperationCanceledException)
                {
                    break;
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
