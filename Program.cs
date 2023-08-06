using DicordNET.ApiClasses;
using System.Runtime.Versioning;
using System.Text;

namespace DicordNET
{
    internal class Program
    {
        [SupportedOSPlatform("windows")]
        private static void Main()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            YoutubeApiWrapper.Init();
            YandexApiWrapper.Init();
            VkApiWrapper.Init();
            StaticBotInstanceContainer.Run();
        }
    }
}
