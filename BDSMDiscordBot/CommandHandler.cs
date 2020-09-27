using System;
using System.Reflection;
using System.Threading.Tasks;
using Discord.Commands;
using Discord.WebSocket;
using DiscordImporterBot.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace DiscordImporterBot
{
    public class CommandHandler : IDisposable
    {
        private readonly DiscordSocketClient _client;
        private readonly CommandService _commands;
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<CommandHandler> _logger;
        private readonly char _prefix;

        public CommandHandler(DiscordSocketClient client, CommandService commands, IServiceProvider serviceProvider,
            IOptions<AppConfiguration> appConfiguration, ILogger<CommandHandler> logger)
        {
            _commands = commands;
            _serviceProvider = serviceProvider;
            _client = client;
            _logger = logger;

            var configuration = appConfiguration.Value
                ?? throw new ArgumentNullException("'appConfiguration' cannot be null.");
            var prefix = configuration.Prefix?.ToCharArray()
                ?? throw new ArgumentNullException("Prefix", "There was no prefix specified.");

            if (prefix.Length != 1)
            {
                throw new ArgumentException($"Prefix is not a single character: {configuration.Prefix}");
            }

            _prefix = prefix[0];
        }

        public void Dispose()
        {
            _client.MessageReceived -= HandleCommandAsync;
        }

        public async Task InstallCommandsAsync()
        {
            // Hook the MessageReceived event into our command handler
            _client.MessageReceived += HandleCommandAsync;

            // Here we discover all of the command modules in the entry 
            // assembly and load them.
            await _commands.AddModulesAsync(assembly: Assembly.GetEntryAssembly(),
                services: _serviceProvider);

            // Remove the upload log module.
            // await _commands.RemoveModuleAsync<UploadLogModule>().ConfigureAwait(false);
        }

        private async Task HandleCommandAsync(SocketMessage socketMessage)
        {
            // Don't process the command if it was a system message
            var message = socketMessage as SocketUserMessage;
            if (message == null)
            {
                return;
            }

            // Create a number to track where the prefix ends and the command begins
            int argPos = 0;

            // Determine if the message is a command based on the prefix and make sure no bots trigger commands
            if (!(message.HasCharPrefix(_prefix, ref argPos)
                || message.HasMentionPrefix(_client.CurrentUser, ref argPos))
                || message.Author.IsBot)
            {
                //_logger.LogDebug("Command did not match prefix, or is bot. {SocketMessage}", socketMessage);
                return;
            }

            // Create a WebSocket-based command context based on the message
            var context = new SocketCommandContext(_client, message);

            // Execute the command with the command context we just
            // created, along with the service provider for precondition checks.

            // Keep in mind that result does not indicate a return value
            // rather an object stating if the command executed successfully.
            var result = await _commands.ExecuteAsync(
                context: context,
                argPos: argPos,
                services: _serviceProvider);

            // Optionally, we may inform the user if the command fails
            // to be executed; however, this may not always be desired,
            // as it may clog up the request queue should a user spam a
            // command.
            // await context.Channel.SendMessageAsync(result.ErrorReason);

            if (!result.IsSuccess)
            {
                _logger.LogWarning("Did not successfully execute message {SocketMessage}. Error: {Result}",
                    socketMessage, result);
            }
        }
    }
}
