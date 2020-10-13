using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using BDSMDiscordBot.Models;
using Discord;
using Discord.WebSocket;
using DiscordImporterBot.Models;
using Microsoft.Extensions.Logging;

namespace DiscordImporterBot.Work
{
    public class UploadLogWork : IWork
    {
        private readonly string _filesDirectory;
        private readonly bool _skipBotMessages;
        private readonly ISocketMessageChannel _executedInChannel;
        private readonly ExportedLog _exportedLog;
        private readonly SocketTextChannel _destinationChannel;
        private readonly ILogger<UploadLogWork> _logger;

        public UploadLogWork(ISocketMessageChannel executedInChannel, ExportedLog exportedLog,
            SocketTextChannel destinationChannel, string requestId, string filesDirectory, bool skipBotMessages,
            ILogger<UploadLogWork> logger)
        {
            _executedInChannel = executedInChannel ?? throw new ArgumentNullException(nameof(executedInChannel));
            _exportedLog = exportedLog ?? throw new ArgumentNullException(nameof(exportedLog));
            _destinationChannel = destinationChannel ?? throw new ArgumentNullException(nameof(destinationChannel));
            RequestId = requestId ?? throw new ArgumentNullException(nameof(requestId));

            _logger = logger;
            _filesDirectory = filesDirectory;
            _skipBotMessages = skipBotMessages;
        }

        public string RequestId { get; }

        public async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
            {
                _logger.LogInformation("{RequestId} Cancellation is requested. Stopping.", RequestId);
                return;
            }

            _logger.LogInformation("STARTING [{RequestId}]...", RequestId);
            await _executedInChannel.SendMessageAsync($"STARTING [{RequestId}]...").ConfigureAwait(false);

            for (int i = 0; i < _exportedLog.Messages.Count; i++)
            {
                var message = _exportedLog.Messages[i];
                if (i % 25 == 0)
                {
                    _logger.LogInformation("\t[{RequestId}] Message #{Index}", RequestId, i);
                }
                else
                {
                    _logger.LogDebug("\t[{RequestId}] Message #{Index}", RequestId, i);
                }

                try
                {
                    await UploadMessageAsync(message).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "{RequestId}: Unable to upload message. {Message}", RequestId, message);
                }
            }

            _logger.LogInformation("FINISHED [{RequestId}]...", RequestId);
            await _executedInChannel.SendMessageAsync($"FINISHED [{RequestId}]...").ConfigureAwait(false);
        }

        private async Task UploadMessageAsync(ChannelMessage message)
        {
            var hasAttachments = message.Attachments?.Any() ?? false;
            if (message.Author.IsBot && _skipBotMessages)
            {
                _logger.LogInformation("\t[{RequestId}] {MessageId}: {Message} is from bot. Skipping...",
                    RequestId, message.Id, message);
                return;
            }
            else if (string.IsNullOrEmpty(message.Content) && !hasAttachments)
            {
                _logger.LogInformation(
                    "\t[{RequestId}] {MessageId}: {Message} is an empty string and there are no embeds. Skipping...",
                    RequestId, message.Id, message);
                return;
            }

            bool isFirstEntry = true;
            foreach (var attachment in message.Attachments)
            {
                try
                {
                    await UploadAsync(message, attachment, isFirstEntry).ConfigureAwait(false);
                    isFirstEntry = false;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "\t[{RequestId}] {MessageId}: Error uploading attachment. {Attachment}",
                        RequestId, message.Id, attachment);
                }
            }

            foreach (var embed in message.Embeds)
            {
                try
                {
                    await UploadAsync(message, embed, isFirstEntry).ConfigureAwait(false);
                    isFirstEntry = false;
                }
                catch (Exception e)
                {
                    _logger.LogError(e, "\t[{RequestId}] {MessageId}: Error uploading embed. {Embed}",
                        RequestId, message.Id, embed);
                }
            }

            if (isFirstEntry)
            {
                _logger.LogDebug("\t[{RequestId}] {MessageId}: Did not contain any attachments or embeds.");
                await UploadAsync(message, isFirstEntry).ConfigureAwait(false);
            }
        }

        private Task UploadAsync(ChannelMessage message, bool isFirstEntry)
        {
            var embed = isFirstEntry
                ? WithOriginalInformation(message).Build()
                : null;
            return _destinationChannel.SendMessageAsync(message.Content, embed: embed);
        }

        private async Task UploadAsync(ChannelMessage message, ChannelAttachment attachment, bool isFirstEntry)
        {
            var fullPath = Path.Combine(_filesDirectory, attachment.Url);

            var content = isFirstEntry ? message.Content : string.Empty;
            var embed = isFirstEntry
                ? WithOriginalInformation(message).Build()
                : null;

            using (var reader = File.OpenRead(fullPath))
            {
                await _destinationChannel.SendFileAsync(reader, attachment.FileName, content, embed: embed)
                    .ConfigureAwait(false);
            }
        }

        private async Task UploadAsync(ChannelMessage message, ChannelEmbed embed, bool isFirstEntry)
        {
            var content = isFirstEntry ? message.Content : string.Empty;
            var embedBuilder = isFirstEntry
                ? WithOriginalInformation(message)
                : new EmbedBuilder();

            // This is a thumbnail link to a local image that we downloaded and should upload.
            if (embed.Thumbnail != null
                && !Uri.TryCreate(embed.Thumbnail?.Url ?? string.Empty, UriKind.Absolute, out var _))
            {
                _logger.LogDebug("\t\t{MessageId} uses relative thumbnail path. Uploading: {ThumbnailUri}",
                    message.Id, embed.Thumbnail.Url);

                //var builder = new EmbedBuilder { ImageUrl =  };
                var embedUrl = new Uri(embed.Url);
                var name = embedUrl.Segments.Last();
                var fileInfo = new FileInfo(Path.Combine(_filesDirectory, embed.Thumbnail.Url));

                embedBuilder.ImageUrl = $"attachment://{name}";
                var relativeEmbed = embedBuilder.Build();
                using (var reader = fileInfo.OpenRead())
                {
                    await _destinationChannel.SendFileAsync(reader, name, content, embed: relativeEmbed)
                        .ConfigureAwait(false);
                }

                return;
            }

            // This is probably a message with a ton of actual links in it.
            if (embed.Thumbnail != null)
            {
                _logger.LogInformation("\t\t{MessageId} has embed message link. Skipping? {Skipping} URL: {ThumbnailUri}",
                    message.Id, !isFirstEntry, embed.Thumbnail.Url);

                if (isFirstEntry)
                {
                    await _destinationChannel.SendMessageAsync(content, false, embed: embedBuilder.Build())
                        .ConfigureAwait(false);
                }

                return;
            }

            if (!string.IsNullOrEmpty(embed.Url))
            {
                embedBuilder.WithImageUrl(embed.Url);
            }
            else if (embed.Image != null)
            {
                embedBuilder.WithImageUrl(embed.Image.Url);
            }

            await _destinationChannel.SendMessageAsync(content, false, embed: embedBuilder.Build())
                .ConfigureAwait(false);
        }

        private EmbedBuilder WithOriginalInformation(ChannelMessage message)
        {
            return new EmbedBuilder()
                .WithAuthor(builder => builder.WithName($"{message.Author.FullName} ({message.Author.Id})"))
                .WithFooter(builder => builder.WithText($"{message.Timestamp:s}\tId: {message.Id}"));
        }
    }
}
