using DicordNET.ApiClasses;
using DicordNET.Bot;
using DicordNET.Commands;
using DSharpPlus.Entities;
using System.Collections.Generic;
using System.Linq;

namespace DicordNET.Player
{
    internal static partial class PlayerManager
    {
        internal static void Skip(int add_count = 0, CommandActionSource source = CommandActionSource.None)
        {
            lock (tracks_queue)
            {
                if (tracks_queue.Count < add_count)
                {
                    if ((source & CommandActionSource.Mute) == 0)
                    {
                        BotWrapper.SendMessage(new DiscordEmbedBuilder()
                        {
                            Color = DiscordColor.Blue,
                            Title = "Cannot skip"
                        });
                        return;
                    }
                }
                List<ITrackInfo> collection = new();
                while (tracks_queue.Any())
                {
                    collection.Add(tracks_queue.Dequeue());
                }
                collection = collection.Skip(add_count).ToList();
                foreach (var track in collection)
                {
                    tracks_queue.Enqueue(track);
                }
            }

            if ((source & CommandActionSource.Mute) == 0)
            {
                if (IsPlaying)
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Blue,
                        Title = $"Skipped{(add_count == 0 ? "" : $" {add_count + 1} tracks")}"
                    });
                }
                else
                {
                    BotWrapper.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Red,
                        Title = "Nothing to skip"
                    });
                }
            }
            IsPlaying = false;
        }
    }
}
