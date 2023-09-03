using DSharpPlus.Entities;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using MyGreatestBot.Extensions;
using System;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void RequestSeek(TimeSpan span, CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
            if (IsPlaying && currentTrack != null)
            {
                if (currentTrack.IsSeekPossible(span))
                {
                    lock (currentTrack)
                    {
                        currentTrack.PerformSeek(span);
                        SeekRequested = true;
                    }

                    if (!mute)
                    {
                        Handler.Message.Send(GetPlayingMessage<SeekException>(currentTrack, "Playing"));
                    }

                    Task.Yield().GetAwaiter().GetResult();
                }
                else
                {
                    if (!mute)
                    {
                        throw new SeekException("Cannot seek");
                    }
                    return;
                }
            }
            else
            {
                if (!mute)
                {
                    throw new SeekException("Nothing to seek");
                }
                return;
            }
        }
    }
}
