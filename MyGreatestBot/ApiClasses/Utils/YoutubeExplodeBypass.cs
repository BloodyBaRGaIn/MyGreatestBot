using System;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace MyGreatestBot.ApiClasses.Utils
{
    /// <summary>
    /// Source code from YoutubeExplode
    /// </summary>
    internal static class YoutubeExplodeBypass
    {
        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        [SuppressMessage("Globalization", "CA2101")]
        [SuppressMessage("Interoperability", "SYSLIB1054")]
        private static extern int RegOpenKeyEx(
            nuint hKey,
            string subKey,
            int ulOptions,
            int samDesired,
            out nuint hkResult
        );

        [DllImport("advapi32.dll", CharSet = CharSet.Auto)]
        [SuppressMessage("Globalization", "CA2101")]
        private static extern int RegQueryValueEx(
            nuint hKey,
            string lpValueName,
            int lpReserved,
            out uint lpType,
            StringBuilder lpData,
            ref uint lpcbData
        );

        private static string? GetCurrentUserRegistryValue(string key, string entry)
        {
            if (RegOpenKeyEx(new nuint(0x80000001u), key, 0, 0x20019, out nuint keyHandle) != 0)
            {
                return null;
            }

            uint size = 1024u;
            StringBuilder buffer = new((int)size);
            if (RegQueryValueEx(keyHandle, entry, 0, out _, buffer, ref size) != 0)
            {
                return null;
            }

            return buffer.ToString();
        }

        internal static void Bypass()
        {
            if (IsRestricted())
            {
                // otherwise it won't work in restricted countries
                Environment.SetEnvironmentVariable("SLAVA_UKRAINI", "1");
            }
        }

        private static bool IsRestricted()
        {
            string locale = CultureInfo.CurrentCulture.Name;

            if (locale.EndsWith("-ru", StringComparison.OrdinalIgnoreCase) ||
                locale.EndsWith("-by", StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                string? region = GetCurrentUserRegistryValue(@"Control Panel\International\Geo", "Name");

                if (string.Equals(region, "ru", StringComparison.OrdinalIgnoreCase) ||
                    string.Equals(region, "by", StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }
    }
}
