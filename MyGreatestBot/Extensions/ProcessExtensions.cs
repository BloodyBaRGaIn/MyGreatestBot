using System;
using System.Diagnostics;

namespace MyGreatestBot.Extensions
{
    public static class ProcessExtensions
    {
        private static readonly ProcessPriorityClass[] PriorityArray =
        {
            ProcessPriorityClass.Idle,
            ProcessPriorityClass.BelowNormal,
            ProcessPriorityClass.Normal,
            ProcessPriorityClass.AboveNormal,
            ProcessPriorityClass.High,
            ProcessPriorityClass.RealTime
        };

        public static void SetHighestAvailableProcessPriority(
            this Process process,
            ProcessPriorityClass highest = ProcessPriorityClass.RealTime,
            ProcessPriorityClass lowest = ProcessPriorityClass.Idle)
        {
            int priorityIndex = Array.IndexOf(PriorityArray, highest);
            int lowestIndex = Array.IndexOf(PriorityArray, lowest);
            if (priorityIndex == -1 || lowestIndex == -1)
            {
                return;
            }

            while (true)
            {
                try
                {
                    process.PriorityClass = PriorityArray[priorityIndex];
                    break;
                }
                catch
                {
                    if (priorityIndex == lowestIndex)
                    {
                        break;
                    }
                }
                priorityIndex--;
            }
        }
    }
}
