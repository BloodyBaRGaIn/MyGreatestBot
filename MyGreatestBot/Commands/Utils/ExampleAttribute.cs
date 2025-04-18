﻿using MyGreatestBot.Extensions;
using System;
using System.Collections.Generic;

namespace MyGreatestBot.Commands.Utils
{
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public sealed class ExampleAttribute(params string[] examples) : Attribute
    {
        public IEnumerable<string> ExamplesCollection { get; } = StringExtensions.EnsureStrings(examples);
    }
}
