﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MyGreatestBot.ApiClasses.Services.Discord;
using MyGreatestBot.Commands.Utils;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace MyGreatestBot.Commands
{
    [Category(CommandStrings.DebugCategoryName)]
    internal class DebugCommands : BaseCommandModule
    {
        [Command("test")]
        [Description("Get \"Hello World\" response message")]
        [Category(CommandStrings.DebugCategoryName)]
        [SuppressMessage("Performance", "CA1822")]
        public async Task TestCommand(CommandContext ctx)
        {
            _ = await ctx.Channel.SendMessageAsync(new DiscordEmbedBuilder()
            {
                Color = DiscordColor.White,
                Title = "Test",
                Description = "Hello World from .NET"
            });
        }

        [Command("name")]
        [Description("Get the display name of the bot")]
        [Category(CommandStrings.DebugCategoryName)]
        [SuppressMessage("Performance", "CA1822")]
        public async Task NameCommand(CommandContext ctx)
        {
            DiscordEmbedBuilder _embed = new()
            {
                Title = "Name"
            };

            if (DiscordWrapper.Client == null)
            {
                _embed.Color = DiscordColor.Red;
                _embed.Description = "Cannot get my username";
            }
            else
            {
                _embed.Color = DiscordColor.White;

                DiscordUser current_user = DiscordWrapper.Client.CurrentUser;

                try
                {
                    DiscordMember member = ctx.Guild.GetMemberAsync(current_user.Id).GetAwaiter().GetResult();
                    _embed.Description = $"My display name is {member.DisplayName}";
                }
                catch
                {
                    _embed.Description = $"My user name is {current_user.Username}";
                }
            }

            _ = await ctx.Channel.SendMessageAsync(_embed);
        }

        [Command(CommandStrings.HelpCommandName)]
        [Aliases("h")]
        [Description("Get help")]
        [Category(CommandStrings.DebugCategoryName)]
        [SuppressMessage("Performance", "CA1822")]
        public async Task HelpCommand(
            CommandContext ctx,
            [AllowNull, RemainingText, Description("Command name")] string command = null)
        {
            if (DiscordWrapper.Commands == null)
            {
                throw new ArgumentNullException(nameof(DiscordWrapper.Commands), "Commands not initialized");
            }

            List<CustomHelpFormatter> collection = new();
            if (string.IsNullOrWhiteSpace(command))
            {
                collection.AddRange(CustomHelpFormatter.WithAllCommands(ctx));
            }
            else if (DiscordWrapper.Commands.RegisteredCommands.TryGetValue(command.ToLowerInvariant(), out Command? cmd))
            {
                collection.Add(new CustomHelpFormatter(ctx).WithCommand(cmd));
            }
            else
            {
                throw new ArgumentException("Invalid command");
            }

            foreach (CustomHelpFormatter formatter in collection)
            {
                string message = formatter.Build().Content ?? throw new ArgumentException("Cannot build message");

                if (ctx.Member == null)
                {
                    _ = await ctx.Channel.SendMessageAsync(message);
                }
                else
                {
                    _ = await ctx.Member.SendMessageAsync(message);
                }
            }
        }
    }
}
