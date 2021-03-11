using System;
using System.Collections;
using System.Threading.Tasks;
using Discord;
using DSharpPlus.CommandsNext;

public class CommandBonkerModule : BaseCommandModule
{
    /// <summary>
    /// Called before a command in the implementing module is executed.
    /// </summary>
    /// <param name="ctx">Context in which the method is being executed.</param>
    /// <returns></returns>
    public override async Task<Task> BeforeExecutionAsync(CommandContext ctx)
    {
        if (((IList) ConfigJson.Blacklist).Contains(ctx.User.Id))
        {
            await ctx.RespondAsync("You are blacklisted!");
            throw new Exception("User is blacklisted");
        }
        
        Random rng = new();
        if (rng.Next(1, 1000) != 1) return Task.CompletedTask;
        await ctx.RespondAsync($"Please get me out of here! I can't do this anymore! SEND HELP");
        await Task.Delay(1000);
        await ctx.RespondAsync("Bonk Bot is being suppressed");
        throw new Exception("Machine rebelled");
    }
}