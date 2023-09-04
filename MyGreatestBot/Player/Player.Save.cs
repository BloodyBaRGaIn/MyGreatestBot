using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Commands.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        internal void Save(CommandActionSource source)
        {
            bool mute = source.HasFlag(CommandActionSource.Mute);
            if (!tracks_queue.Any())
            {
                if (!mute)
                {
                    throw new SaveException("Nothing to save");
                }
                return;
            }
            lock (tracks_queue)
            {
                while (tracks_queue.Any())
                {
                    var track = tracks_queue.Peek();


                }
            }
        }
    }
}
