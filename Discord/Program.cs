using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Extensions;
using Newtonsoft.Json;
using static System.Console;
using static Shared.Shared;

namespace Discord
{
    class Program
    {
        static async Task Main()
        {
            await BeginMySQL();
            MainAsync().GetAwaiter().GetResult();
        }
        
        public static List<ulong> activeInteraction = new();
        public static DiscordClient Client { get; private set; }
        public static CommandsNextExtension Commands { get; private set; }
        
        static async Task MainAsync()
        {

            string json = string.Empty;

            await using (FileStream fs = File.OpenRead("config.json"))
            using (StreamReader sr = new(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync().ConfigureAwait(false);
            
            ConfigJson ConfigJson = JsonConvert.DeserializeObject<ConfigJson>(json);
            
            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = ConfigJson.Token,
                TokenType = TokenType.Bot,
                AutoReconnect = true,
                MinimumLogLevel = Microsoft.Extensions.Logging.LogLevel.Debug
            });
            
            Client.Ready += OnClientReady;

            CommandsNextConfiguration commandsConfig = new()
            {
                StringPrefixes = ConfigJson.Prefix,
                EnableDms = false,
                EnableMentionPrefix = true,
                IgnoreExtraArguments = true,
            };

            Client.UseInteractivity(new InteractivityConfiguration
            {
                Timeout = new TimeSpan(0, 0, 20)
            });
            
            Commands = Client.UseCommandsNext(commandsConfig);

            Commands.CommandErrored += CmdErroredHandler;
            Commands.RegisterCommands<CommandAdd>();

            await Client.ConnectAsync();

            await Task.Delay(-1);
        }
        
        private static Task OnClientReady(DiscordClient sender, ReadyEventArgs e)
        {
            return Task.CompletedTask;
        }
        
        private static async Task CmdErroredHandler(CommandsNextExtension _, CommandErrorEventArgs e)
        {
            try
            {
                IReadOnlyList<CheckBaseAttribute> failedChecks = ((ChecksFailedException)e.Exception).FailedChecks;
                foreach (CheckBaseAttribute failedCheck in failedChecks)
                {
                    switch (failedCheck)
                    {
                        case DeveloperOnly:
                            await e.Context.RespondAsync("You are not a whitelisted developer!");
                            break;
                    }
                }
            }
            catch (Exception)
            {
                switch (e.Exception.Message)
                {
                    case "Could not find a suitable overload for the command.":
                        await e.Context.RespondAsync($"The arguments for this command are invalid.\nPlease do '{ConfigJson.Prefix}help {e.Command.Name}' to see all needed arguments.");
                        break;
                    default:
                        WriteLine(e.Exception.Message);
                        WriteLine(e.Exception.StackTrace);
                        WriteLine(e.Exception.Source);
                        break;
                }
            }
        }
    }
}