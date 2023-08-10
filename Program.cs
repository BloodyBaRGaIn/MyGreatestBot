using DicordNET.ApiClasses;
using DicordNET.Bot;
using DicordNET.Utils;
using System.Diagnostics;
using System.Runtime.Versioning;
using System.Text;

namespace DicordNET
{
    internal class Program
    {
        [SupportedOSPlatform("windows")]
        private static void Main()
        {
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            ApiConfig.InitApis();

            BotWrapper.Run();
        }
    }
}
