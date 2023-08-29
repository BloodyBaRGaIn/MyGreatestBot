﻿using DSharpPlus.Entities;
using MyGreatestBot.Commands;

namespace MyGreatestBot.Player
{
    internal partial class Player
    {
        internal void Pause(CommandActionSource source = CommandActionSource.None)
        {
            if (!source.HasFlag(CommandActionSource.Mute))
            {
                if (IsPlaying)
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Paused"
                    });
                }
                else
                {
                    Handler.SendMessage(new DiscordEmbedBuilder()
                    {
                        Color = DiscordColor.Yellow,
                        Title = "Nothing to pause"
                    });
                }
            }

            IsPaused = true;
        }
    }
}
