using System.Threading;

namespace MyGreatestBot.Extensions
{
    public static class ThreadExtensions
    {
        public static void SetHighestAvailableTheadPriority(
            this Thread thread,
            ThreadPriority highest = ThreadPriority.Highest,
            ThreadPriority lowest = ThreadPriority.Lowest)
        {
            ThreadPriority current = highest;
            while (true)
            {
                try
                {
                    thread.Priority = current;
                    break;
                }
                catch
                {
                    if (current == lowest)
                    {
                        break;
                    }
                    current--;
                }
            }
        }
    }
}
