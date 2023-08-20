using DicordNET.Bot;
using DicordNET.Commands;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal partial class PlayerManager
    {
        internal static void Ignore(CommandActionSource source = CommandActionSource.None)
        {
            if (!IsPlaying || currentTrack == null)
            {
                if ((source & CommandActionSource.Mute) == 0)
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Nothing to ignore"
                    });
                }

                return;
            }

            lock (currentTrack)
            {
                using var stream = File.Open(IgnoredPath, FileMode.Append, FileAccess.Write);
                using var writer = new StreamWriter(stream);
                writer.WriteLine(currentTrack.GetShortInfo());
                writer.Close();
            }

            Skip(0, CommandActionSource.Mute);

            if ((source & CommandActionSource.Mute) == 0)
            {
                BotWrapper.SendMessage(new DiscordEmbedBuilder()
                {
                    Color = DiscordColor.Yellow,
                    Title = "Track ignored"
                });
            }
        }
    }
}
