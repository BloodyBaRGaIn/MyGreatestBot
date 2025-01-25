using System;

namespace MyGreatestBot.Commands.Utils
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ConsoleCommandAttribute(string name) : Attribute
    {
        public string Name { get; } = name;
    }
}
