using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

[AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
public class DeveloperOnly : CheckBaseAttribute
{
    public DeveloperOnly() { }

    readonly List<float> whitelist = new List<float>
    {
    388454632835514380 //Coca
    };

    public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help)
    {
        return Task.FromResult(!whitelist.Contains(ctx.User.Id));
    }
}