using MyGreatestBot.Bot;
using MyGreatestBot.Utils;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

namespace MyGreatestBot
{
    /// <summary>
    /// Main class
    /// </summary>
    [SupportedOSPlatform("windows")]
    [UnsupportedOSPlatform("linux")]
    [UnsupportedOSPlatform("macos")]
    [UnsupportedOSPlatform("android")]
    [UnsupportedOSPlatform("ios")]
    internal class Program
    {
        /// <summary>
        /// Main entry point
        /// </summary>
        private static void Main()
        {
            try
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            }
            catch { }

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ApiManager.InitApis();

            BotWrapper.Run();

            ApiManager.DeinitApis();
        }
    }
}
