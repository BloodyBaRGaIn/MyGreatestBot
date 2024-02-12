using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Resume(CommandActionSource source)
        {
            IsPaused = false;
            if (source.HasFlag(CommandActionSource.Mute))
            {
                return;
            }
            if (currentTrack == null)
            {
                throw new ResumeException("Nothing to resume");
            }
            else if (IsPlaying)
            {
                Handler.Message.Send(new ResumeException("Resumed").WithSuccess());
            }
            else
            {
                throw new PlayerException("Illegal state detected");
            }
        }
    }
}
