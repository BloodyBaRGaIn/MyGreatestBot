using DicordNET.ApiClasses;
using DicordNET.Bot;
using DicordNET.Sql;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

namespace DicordNET
{
    /// <summary>
    /// Main class
    /// </summary>
    [SupportedOSPlatform("windows")]
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
