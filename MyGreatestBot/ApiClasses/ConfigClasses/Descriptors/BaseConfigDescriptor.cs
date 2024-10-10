using MyGreatestBot.Extensions;

namespace MyGreatestBot.ApiClasses.ConfigClasses.Descriptors
{
    internal sealed class RootDescriptor
    {
        internal string Directory { get; }
        internal string Extension { get; } = string.Empty;

        internal RootDescriptor(string key)
        {
            Directory = PropertiesProvider.GetProperty(key);
        }

        internal RootDescriptor(string key, string extension) : this(key)
        {
            Extension = extension;
        }
    }

    internal abstract class BaseConfigDescriptor
    {
        internal abstract RootDescriptor Root { get; }
        internal string Name { get; }
        internal string FullPath { get; }
        internal BaseConfigDescriptor(string key)
        {
            Name = string.Join('.',
                StringExtensions.EnsureStrings(
                    PropertiesProvider.GetProperty(key), Root.Extension));
            FullPath = System.IO.Path.Combine(Root.Directory, Name);
        }
    }
}
