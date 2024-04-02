using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses.Music;
using MyGreatestBot.Commands.Exceptions;
using MyGreatestBot.Extensions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;

namespace MyGreatestBot.Player
{
    internal sealed partial class Player
    {
        private static DiscordEmbedBuilder GetPlayingMessage<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] T>(ITrackInfo track, string state)
            where T : CommandExecutionException
        {
            T exception;

            try
            {
                exception = (typeof(T).GetConstructors()
                .First(c =>
                {
                    ParameterInfo[] parameters = c.GetParameters();
                    return parameters.Length == 1 && parameters[0].ParameterType == typeof(string);
                })
                .Invoke(new[] { track.GetMessage(state) }) as T)
                ?? throw new ArgumentException("Cannot create message");
            }
            catch
            {
                throw;
            }

            _ = exception.WithSuccess();
            DiscordEmbedBuilder message = exception.GetDiscordEmbed();
            message.Thumbnail = track.GetThumbnail();
            return message;
        }
    }
}
