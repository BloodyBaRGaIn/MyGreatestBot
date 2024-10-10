namespace MyGreatestBot.ApiClasses.ConfigClasses.Descriptors
{
    internal sealed class FfmpegDescriptor : BaseConfigDescriptor
    {
        private static readonly RootDescriptor root = new("FfmpegDir", "exe");

        internal override RootDescriptor Root => root;

        internal FfmpegDescriptor(string key) : base(key)
        {

        }
    }
}
