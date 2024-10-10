using SharedClasses;
using System;
using System.Collections.Generic;

namespace MyGreatestBot.ApiClasses.ConfigClasses
{
    internal static class PropertiesProvider
    {
        private static readonly Dictionary<string, string> Properties;

        static PropertiesProvider()
        {
            if (!BuildPropsProvider.GetProperties(out Properties))
            {
                throw BuildPropsProvider.LastError
                    ?? new InvalidOperationException("Cannot get properties");
            }
        }

        internal static string GetProperty(string key)
        {
            return Properties.TryGetValue(key, out string? value) ? value : string.Empty;
        }
    }
}
