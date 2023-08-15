using DicordNET.ApiClasses;
using DicordNET.Bot;
using System.Diagnostics;
using System.Text;

namespace DicordNET
{
    /// <summary>
    /// Main class
    /// </summary>
    internal class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        private static void Main()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ApiConfig.InitApis();

            BotWrapper.Run();

            ApiConfig.DeinitApis();
        }
    }
}
