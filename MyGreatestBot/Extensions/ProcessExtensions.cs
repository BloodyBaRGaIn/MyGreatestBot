using System;
using System.Diagnostics;

namespace MyGreatestBot.Extensions
{
    /// <summary>
    /// <see cref="Process"/> extensions
    /// </summary>
    public static class ProcessExtensions
    {
        private static readonly ProcessPriorityClass[] PriorityArray =
        [
            ProcessPriorityClass.Idle,
            ProcessPriorityClass.BelowNormal,
            ProcessPriorityClass.Normal,
            ProcessPriorityClass.AboveNormal,
            ProcessPriorityClass.High,
            ProcessPriorityClass.RealTime
        ];

        /// <summary>
        /// Tries to set the priority of a given <paramref name="process"/> instance, 
        /// starting with the <paramref name="highest"/> value 
        /// and ending with the <paramref name="lowest"/> value.
        /// <para>
        /// If an exception occurs while attempting to set the desired priority, 
        /// a reduced priority will be attempted. The priority value to set will be 
        /// decremented until the assignment is successful, 
        /// or until the priority value equals the <paramref name="lowest"/> value.
        /// </para>
        /// </summary>
        /// 
        /// <param name="process">
        /// <see cref="Process"/> instance.<br/>
        /// Should be not <c>null</c>, otherwise the method returns <c>false</c>.
        /// </param>
        /// <param name="highest">
        /// Desired process priority value.
        /// </param>
        /// <param name="lowest">
        /// Minimum acceptable process priority value.<br/>
        /// If the value is greater than the desired priority value,<br/>
        /// this parameter will be assigned with the desired priority value.
        /// </param>
        /// 
        /// <returns>
        /// <c>true</c> if the process priority was successfully set 
        /// to the desired value, otherwise <c>false</c>.
        /// </returns>
        public static bool SetHighestAvailableProcessPriority(
            this Process process,
            ProcessPriorityClass highest = ProcessPriorityClass.RealTime,
            ProcessPriorityClass lowest = ProcessPriorityClass.Idle)
        {
            if (process == null)
            {
                return false;
            }

            int priorityIndex = Array.IndexOf(PriorityArray, highest);
            int lowestIndex = Array.IndexOf(PriorityArray, lowest);

            if (priorityIndex == -1 || lowestIndex == -1)
            {
                return false;
            }

            if (lowestIndex > priorityIndex)
            {
                lowestIndex = priorityIndex;
            }

            bool success = true;

            while (priorityIndex > lowestIndex)
            {
                if (process.SetProcessPriority(PriorityArray[priorityIndex]))
                {
                    return success;
                }
                else
                {
                    success = false;
                }
                priorityIndex--;
            }

            return false;
        }

        /// <summary>
        /// Tries to set the priority of a given <paramref name="process"/> instance
        /// to the given <paramref name="priority"/> value.
        /// </summary>
        /// 
        /// <param name="process">
        /// <inheritdoc cref="SetHighestAvailableProcessPriority" path="/param[@name='process']"/>
        /// </param>
        /// <param name="priority">
        /// <inheritdoc cref="SetHighestAvailableProcessPriority" path="/param[@name='highest']"/>
        /// </param>
        /// 
        /// <returns>
        /// <inheritdoc cref="SetHighestAvailableProcessPriority"/>
        /// </returns>
        public static bool SetProcessPriority(
            this Process process,
            ProcessPriorityClass priority)
        {
            if (process == null)
            {
                return false;
            }

            try
            {
                process.PriorityClass = priority;
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
