using System;
using System.Text.Json;
using System.Threading.Tasks;
using BDSMDiscordBot.Configuration;
using BDSMDiscordBot.Models;
using BDSMDiscordBot.Work;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Serilog;

namespace BDSMDiscordBot
{
    public class Program
    {
        // SCOPES https://discord.com/api/oauth2/authorize?client_id=759605874968100935&permissions=68608&scope=bot

        public static async Task Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            var options = host.Services.GetRequiredService<IOptions<SecretsConfiguration>>();
            var clientSecret = options.Value;
            var client = host.Services.GetRequiredService<DiscordSocketClient>();
            var logger = host.Services.GetRequiredService<ILogger<Program>>();

            client.Log += (LogMessage message) => Log(message, logger);

            var modules = host.Services.GetRequiredService<CommandHandler>();

            await modules.InstallCommandsAsync().ConfigureAwait(false);
            await client.LoginAsync(TokenType.Bot, clientSecret.DiscordToken);
            await client.StartAsync();

            // Waits for Ctrl+C or SIGTERM to shut down.
            await host.RunAsync();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            var serilog = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .CreateLogger();

            var builder = new ConfigurationBuilder()
                .AddUserSecrets("f03252cd-8d22-47f0-8460-6d6570a5d8d5")
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .AddCommandLine(args);

            if (bool.TryParse(Environment.GetEnvironmentVariable("IS_DEBUG"), out var result) && result)
            {
                builder.AddJsonFile("appsettings.Development.json");
            }

            var config = builder.Build();

            return Host.CreateDefaultBuilder(args)
                .ConfigureHostConfiguration(configurationBuilder =>
                {
                    configurationBuilder.AddConfiguration(config);
                })
                .ConfigureLogging(loggerBuilder =>
                {
                    loggerBuilder.ClearProviders().AddSerilog(serilog);
                })
                .ConfigureServices((context, services) =>
                {
                    ConfigureServices(services);
                    ConfigureAllOptions(services, config);
                })
                .UseConsoleLifetime();
        }

        private static IServiceCollection ConfigureAllOptions(IServiceCollection serviceCollection,
            IConfiguration configuration)
        {
            return serviceCollection
                .AddOptions()
                .Configure<AppConfiguration>(configuration)
                .Configure<SecretsConfiguration>(configuration);
        }

        private static IServiceCollection ConfigureServices(IServiceCollection serviceCollection)
        {
            var serializerOptions = new JsonSerializerOptions();
            serializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            serializerOptions.WriteIndented = true;
            serializerOptions.Converters.Add(new ChannelTypeConverter());
            serializerOptions.Converters.Add(new MessageTypeConverter());

            return serviceCollection
                .AddSingleton<IBackgroundTaskQueue, BackgroundTaskQueue>()
                .AddSingleton<DiscordSocketClient>()
                .AddSingleton<PermissionResolver>()
                .AddSingleton<CommandService>(service =>
                {
                    var serviceOptions = new CommandServiceConfig();
                    serviceOptions.CaseSensitiveCommands = false;
                    serviceOptions.LogLevel = LogSeverity.Verbose;
                    return new CommandService(serviceOptions);
                })
                .AddSingleton<CommandHandler>()
                .AddSingleton<ContentResolver>()
                .AddSingleton(serializerOptions)

                .AddHostedService<MessageUploaderService>()
                .AddTransient<ExportLogReader>();
        }

        private static Task Log(LogMessage message, ILogger<Program> logger)
        {
            switch (message.Severity)
            {
                case LogSeverity.Error:
                    logger.LogError(message.Message, message);
                    break;
                case LogSeverity.Critical:
                    logger.LogCritical(message.Message, message);
                    break;
                case LogSeverity.Warning:
                    logger.LogWarning(message.Message, message);
                    break;
                case LogSeverity.Info:
                    logger.LogInformation(message.Message, message);
                    break;
                case LogSeverity.Verbose:
                    logger.LogTrace(message.Message, message);
                    break;
                case LogSeverity.Debug:
                    logger.LogDebug(message.Message, message);
                    break;
                default:
                    throw new NotSupportedException("Severity is not supported: " + message.Severity);
            }

            return Task.CompletedTask;
        }
    }
}
