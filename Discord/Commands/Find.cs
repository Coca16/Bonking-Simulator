using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using static Shared.Shared;
using static Discord.Program;
// ReSharper disable UnusedMember.Global

namespace Discord.Commands
{
    public class CommandFind : CommandBonkerModule
    {
        [Command("add")]
        [Aliases("ad")]
        [Priority(2)]
        public async Task AddMember(CommandContext ctx, DiscordMember person) { DiscordClientAdder(person.DisplayName, ctx); }
        
        [Command("add")]
        [Priority(1)]
        public async Task AddUser(CommandContext ctx, DiscordUser person) { DiscordClientAdder(person.Username, ctx); }
        
        private static async void DiscordClientAdder(string name, CommandContext ctx)
        {
            if (name.Length <=  20 && name.Length >= 1)
            {
                InteractivityExtension interactivity = ctx.Client.GetInteractivity();
                if (!activeInteraction.Contains(ctx.User.Id))
                {
                    DiscordEmoji up = DiscordEmoji.FromName(Program.Client , ":thumbsup:");
                    DiscordEmoji down = DiscordEmoji.FromName(Program.Client ,":thumbsdown:");

                    DiscordMessage message = await ctx.RespondAsync($"Do you wish to add {name}?");
                    await message.CreateReactionAsync(up);             
                    await message.CreateReactionAsync(down);
                    activeInteraction.Add(ctx.User.Id);
                    InteractivityResult<MessageReactionAddEventArgs> reaction = await interactivity
                        .WaitForReactionAsync(x => x.Message.Id == message.Id && x.User.Id == ctx.User.Id && (x.Emoji.Equals(up) || x.Emoji.Equals(down)));
                    activeInteraction.Remove(ctx.User.Id);

                    if (reaction.Result.Emoji.Name == up.Name)
                    {
                        await Add(name).ConfigureAwait(false);
                        await ctx.RespondAsync($"Added {name}!").ConfigureAwait(false);
                    }
                    else if (reaction.Result.Emoji.Name == down.Name)
                    {
                        await ctx.RespondAsync($"Cancelling add. If you would like it to be another name you can use {ConfigJson.Prefix[0]}add `name`.").ConfigureAwait(false);
                    }
                }
                else
                {
                    await ctx.RespondAsync($"You already are in another interaction or just were in one! Wait a maximum of 20 seconds before trying again.");
                }
            }
            else await ctx.RespondAsync($"This user's nickname or username is too long (20 char max).");
        }

        [Command("add")]
        [Priority(0)]
        public async Task AddName(CommandContext ctx, [RemainingText] string name)
        {
            if (name.Length <= 20 && name.Length >= 1)
            {
                Add(name);
                await ctx.RespondAsync($"Added {name}!");
            }
            else await ctx.RespondAsync($"This user's nickname or username is too long (20 char max).");
        }
    }
}