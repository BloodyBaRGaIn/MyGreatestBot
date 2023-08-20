using DicordNET.ApiClasses;
using DicordNET.Bot;
using DicordNET.Commands;
using DSharpPlus.Entities;

namespace DicordNET.Player
{
    internal partial class PlayerManager
    {
        internal static void Dequeue()
        {
            string content;

            using (var stream = File.Open(IgnoredPath, FileMode.OpenOrCreate, FileAccess.ReadWrite))
            {
                using var reader = new StreamReader(stream);
                content = reader.ReadToEnd();
            }

            string[] split = content.Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);

        get_track:

            if (!tracks_queue.TryDequeue(out var track))
            {
                currentTrack = null;
                return;
            }

            foreach (var str in split)
            {
                (ApiIntents type, string id) short_info;
                try
                {
                    short_info = ITrackInfo.ParseShortInfo(str);
                }
                catch
                {
                    continue;
                }
                if (track.TrackType == short_info.type && track.Id == short_info.id)
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Skipping ignored track"
                    });
                    goto get_track;
                }
            }

            currentTrack = track;
        }
    }
}
