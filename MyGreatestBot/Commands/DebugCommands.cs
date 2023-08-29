using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MyGreatestBot.Bot;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.Versioning;
using System.Threading.Tasks;

namespace MyGreatestBot.Commands
{
    [Category(CommandStrings.DebugCategoryName)]
    [SupportedOSPlatform("windows")]
    internal class DebugCommands : BaseCommandModule
    {
        [Command("test")]
        [Description("Get \"Hello World\" response message")]
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
        [Description("Get origin bot name")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task NameCommand(CommandContext ctx)
        {
            DiscordEmbedBuilder _embed = new DiscordEmbedBuilder().WithTitle("Name");

            if (BotWrapper.Client == null)
            {
                _embed = _embed
                    .WithColor(DiscordColor.Red)
                    .WithDescription("Cannot get my username");
            }
            else
            {
                _embed = _embed
                    .WithColor(DiscordColor.White);

                DiscordUser current_user = BotWrapper.Client.CurrentUser;

                try
                {
                    DiscordMember member = ctx.Guild.GetMemberAsync(current_user.Id).GetAwaiter().GetResult();
                    _embed = _embed
                        .WithDescription($"My display name is {member.DisplayName}");
                }
                catch
                {
                    _embed = _embed
                        .WithDescription($"My user name is {current_user.Username}");
                }
            }

            _ = await ctx.Channel.SendMessageAsync(_embed);
        }

        [Command(CommandStrings.HelpCommandName)]
        [Aliases("h")]
        [Description("Get help")]
        [SuppressMessage("Performance", "CA1822")]
        public async Task HelpCommand(
            CommandContext ctx,
            [AllowNull, RemainingText, Description("Command name")] string command = null)
        {
            if (BotWrapper.Commands == null)
            {
                throw new ArgumentNullException(nameof(BotWrapper.Commands), "Commands not initialized");
            }

            CustomHelpFormatter custom = string.IsNullOrWhiteSpace(command)
                ? new CustomHelpFormatter(ctx).WithAllCommands()
                : BotWrapper.Commands.RegisteredCommands.TryGetValue(command.ToLowerInvariant(), out Command? cmd)
                    ? new CustomHelpFormatter(ctx).WithCommand(cmd)
                    : throw new ArgumentException("Invalid command");

            _ = await ctx.Channel.SendMessageAsync(custom.Build().Embed);
        }
    }
}
