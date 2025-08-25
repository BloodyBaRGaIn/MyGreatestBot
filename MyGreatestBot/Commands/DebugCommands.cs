using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using MyGreatestBot.Commands.Utils;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using VkNet.Model;

namespace MyGreatestBot.Commands
{
    [Category(CommandStrings.DebugCategoryName)]
    internal class DebugCommands : BaseCommandModule
    {
        [Command("test")]
        [Description("Get \"Hello World\" response message")]
        [Category(CommandStrings.DebugCategoryName)]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task TestCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            handler.Message.Send(new DiscordEmbedBuilder()
            {
                Color = DiscordColor.White,
                Title = "Test",
                Description = "Hello World from .NET"
            });

            await Task.Delay(1);
        }

        [Command("name")]
        [Description("Get the display name of the bot")]
        [Category(CommandStrings.DebugCategoryName)]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task NameCommand(CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

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

            handler.Message.Send(_embed);

            await Task.Delay(1);
        }

        [Command("echo")]
        [Description("Echo the message")]
        [Category(CommandStrings.DebugCategoryName)]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task EchoCommand(
            CommandContext ctx,
            [RemainingText,
            Description("Text")] string text)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.TextChannel = ctx.Channel;

            handler.Message.Send(text);

            await Task.Delay(1);
        }

        [Command("birthday")]
        [Description("Checks for the bot's birthday")]
        [Category(CommandStrings.DebugCategoryName)]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task BirthdayTask(
            CommandContext ctx)
        {
            ConnectionHandler? handler = ConnectionHandler.GetConnectionHandler(ctx.Guild);
            if (handler == null)
            {
                return;
            }

            handler.BypassBirthdayCheck();
            handler.TextChannel = ctx.Channel;

            (int age, bool bday) = DiscordWrapper.Instance.CalculateAge();

            DiscordEmbedBuilder builder = new()
            {
                Color = DiscordColor.White,
                Title = "Anniversary",
            };

            if (age > 0)
            {
                if (bday)
                {
                    builder.Description = 
                        $":partying_face:" +
                        $"It's my {age} year anniversary today!!!" +
                        $":partying_face:";
                }
                else
                {
                    builder.Description =
                        $":confused:" +
                        $"It's not my birthday today... I was created {age} years ago." +
                        $":confused:";
                }
            }
            else
            {
                if (bday)
                {
                    builder.Description =
                        $":partying_face:" +
                        $"I was created just today!!! Congratulations!" +
                        $":partying_face:";
                }
                else
                {
                    builder.Description =
                        $":pleading_face:" +
                        $"I was created less than year ago... Too early to celebrate." +
                        $":pleading_face:";
                }
            }

            handler.Message.Send(builder);

            await Task.Delay(1);
        }

        [Command(CommandStrings.HelpCommandName), Aliases("h")]
        [Description("Get help")]
        [Category(CommandStrings.DebugCategoryName)]
        [SuppressMessage("Performance", "CA1822")]
        [SuppressMessage("CodeQuality", "IDE0079")]
        public async Task HelpCommand(
            CommandContext ctx,
            [AllowNull, RemainingText,
            Description("Command name")] string command = null)
        {
            ArgumentNullException.ThrowIfNull(DiscordWrapper.Commands,
                                              nameof(DiscordWrapper.Commands));

            List<CustomHelpFormatter> collection = [];

            if (string.IsNullOrWhiteSpace(command)
                || string.Equals(command, "all", StringComparison.CurrentCultureIgnoreCase))
            {
                collection.AddRange(CustomHelpFormatter.WithAllCommands(ctx));
            }
            else if (DiscordWrapper.RegisteredCommands.TryGetValue(command.ToLowerInvariant(),
                                                                   out Command? cmd))
            {
                collection.Add(new CustomHelpFormatter(ctx).WithCommand(cmd));
            }
            else
            {
                throw new ArgumentException("Invalid command");
            }

            foreach (CustomHelpFormatter formatter in collection)
            {
                string message = formatter.Build().Content
                    ?? throw new ArgumentException("Cannot build message");

                _ = ctx.Member is null
                    ? await ctx.Channel.SendMessageAsync(message)
                    : await ctx.Member.SendMessageAsync(message);
            }
        }
    }
}
