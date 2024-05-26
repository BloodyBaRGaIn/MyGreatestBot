using System.Threading;

namespace MyGreatestBot.Extensions
{
    public static class SemaphoreExtensions
    {
        /// <summary>
        /// <inheritdoc cref="WaitHandle.WaitOne()"/>
        /// </summary>
        /// <returns>
        /// <inheritdoc cref="WaitHandle.WaitOne()"/>
        /// </returns>
        public static bool TryWaitOne(this Semaphore semaphore)
        {
            try
            {
                return semaphore?.WaitOne() ?? false;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// <inheritdoc cref="WaitHandle.WaitOne(int)"/>
        /// </summary>
        /// <param name="millisecondsTimeout">
        /// <inheritdoc cref="WaitHandle.WaitOne(int)" path="/param[@name='millisecondsTimeout']"/>
        /// </param>
        /// <returns>
        /// <inheritdoc cref="WaitHandle.WaitOne(int)"/>
        /// </returns>
        public static bool TryWaitOne(this Semaphore semaphore, int millisecondsTimeout)
        {
            try
            {
                return semaphore?.WaitOne(millisecondsTimeout) ?? false;
            }
            catch
            {
                return false;
            }
        }

        public static int ReleaseAll(this Semaphore semaphore)
        {
            int count = 0;
            if (semaphore == null)
            {
                return 0;
            }
            while (true)
            {
                try
                {
                    count = semaphore.Release();
                }
                catch
                {
                    return count;
                }
            }
        }

        /// <summary>
        /// <inheritdoc cref="Semaphore.Release(int)"/>
        /// </summary>
        /// <param name="releaseCount">
        /// <inheritdoc cref="Semaphore.Release(int)" path="/param[@name='releaseCount']"/>
        /// </param>
        /// <returns></returns>
        public static int TryRelease(this Semaphore semaphore, int releaseCount = 0)
        {
            if (releaseCount <= 0)
            {
                releaseCount = 1;
            }
            try
            {
                return semaphore?.Release(releaseCount) ?? releaseCount;
            }
            catch
            {
                return 0;
            }
        }

        /// <summary>
        /// <inheritdoc cref="System.IDisposable.Dispose"/>
        /// </summary>
        public static void TryDispose(this Semaphore semaphore)
        {
            _ = semaphore.ReleaseAll();
            try
            {
                semaphore?.Dispose();
            }
            catch { }
        }
    }
}
