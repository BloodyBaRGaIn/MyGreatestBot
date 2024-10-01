using System;

namespace MyGreatestBot.Commands.Utils
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ExampleAttribute(string example) : Attribute
    {
        public string Example { get; } = example;
    }
}
