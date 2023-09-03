using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Extensions;
using System;
using System.Linq;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        private static DiscordEmbedBuilder GetPlayingMessage<T>(ITrackInfo track, string state)
            where T : CommandExecutionException
        {
            T exception = (typeof(T).GetConstructors()
                .First(c =>
                {
                    System.Reflection.ParameterInfo[] parameters = c.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
                })
                .Invoke(new[] { track.GetMessage(state) }) as T) ?? throw new NullReferenceException();
            DiscordEmbedBuilder message = exception.GetDiscordEmbed(true);
            message.Thumbnail = track.GetThumbnail();
            return message;
        }
    }
}
