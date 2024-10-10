namespace MyGreatestBot.ApiClasses.ConfigClasses.Descriptors
{
    internal sealed class JsonConfigDescriptor : BaseConfigDescriptor
    {
        private static readonly RootDescriptor root = new("ConfigDir", "json");

        internal override RootDescriptor Root => root;

        internal JsonConfigDescriptor(string key) : base(key)
        {

        }
    }
}
