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

            ParameterInfo[] parametersInfo = method.GetParameters();

            if (args.Count() > parametersInfo.Length)
            {
                arguments = args.ToArray()[..parametersInfo.Length];
            }
            if (args.Count() < parametersInfo.Length)
            {
                int essential = parametersInfo.Count(m => !m.IsOptional);
                if (args.Count() < essential)
                {
                    throw new InvalidOperationException("Insufficient parameters.");
                }

                arguments = [.. args, .. Enumerable.Repeat<string?>(null, parametersInfo.Length - essential)];
            }

            return method.Invoke(this, arguments);
        }
    }
}
