using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace MyGreatestBot.Commands.Utils
{
    public abstract class ConsoleCommandModule
    {
        public virtual object? InvokeMethod(string commandName, string?[]? arguments = null)
        {
            IEnumerable<MethodInfo> methods = GetType().GetMethods()
                .Where(m => string.Equals(
                    m.GetCustomAttribute<ConsoleCommandAttribute>(false)?.Name ?? string.Empty,
                    commandName, StringComparison.InvariantCultureIgnoreCase));

            if (!methods.Any())
            {
                throw new InvalidOperationException($"Cannot find command \"{commandName}\".");
            }

            if (methods.Count() > 1)
            {
                throw new InvalidOperationException($"Multiple commands with name \"{commandName}\" found.");
            }

            MethodInfo method = methods.FirstOrDefault() ??
                throw new InvalidOperationException($"Cannot extract method for command \"{commandName}\".");

            IEnumerable<string> args = StringExtensions.EnsureStrings(arguments);

            int max = method.GetParameters().Length;

            if (args.Count() > max)
            {
                arguments = args.ToArray()[..max];
            }

            return method.Invoke(this, arguments);
        }
    }
}
