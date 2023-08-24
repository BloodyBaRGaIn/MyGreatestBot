﻿using System;

namespace DicordNET.Extensions
{
    /// <summary>
    /// <see cref="Exception"/> extensions
    /// </summary>
    internal static class ExceptionExtensions
    {
        internal static string GetExtendedMessage(this Exception exception)
        {
            if (string.IsNullOrWhiteSpace(exception.Message))
            {
                return exception.GetType().Name;
            }
            string result = $"{exception.GetType().Name} : {exception.Message}";
            if (exception.InnerException != null)
            {
                result += $"{Environment.NewLine}{GetExtendedMessage(exception.InnerException)}";
            }
            return result;
        }
    }
}
