using System.IO;
using System.Threading.Tasks;
using BDSMDiscordBot.Configuration;
using BDSMDiscordBot.Work;
using Discord.Commands;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BDSMDiscordBot
{
    public class UploadLogModule : ModuleBase<SocketCommandContext>
    {
        private readonly ExportLogReader _exportLogReader;
        private readonly IBackgroundTaskQueue _workQueue;
        private readonly PermissionResolver _permissionResolver;
        private readonly ILogger<UploadLogModule> _logger;
        private readonly ILogger<UploadLogWork> _workItemLogger;
        private readonly string _contentRoot;

        public UploadLogModule(ExportLogReader exportLogReader, IBackgroundTaskQueue workQueue,
            PermissionResolver permissionResolver, IOptions<AppConfiguration> appConfiguration,
            ILogger<UploadLogModule> logger, ILogger<UploadLogWork> workItemLogger)
        {
            _exportLogReader = exportLogReader;
            _workQueue = workQueue;
            _permissionResolver = permissionResolver;
            _logger = logger;
            _workItemLogger = workItemLogger;
            _contentRoot = appConfiguration.Value.RootDirectory;
        }

        [Command("upload")]
        [Summary("Adds log contents for a given file.")]
        public async Task AddLogAsync(
            [Summary("Category")] string category,
            [Summary("File to read")] string filename,
            [Summary("Channel to post to")] ulong channelId,
            [Summary("Skip bot messages")] bool skipBotMessages = false)
        {
            if (!_permissionResolver.HasPermission(Context.Guild, Context.User))
            {
                _logger.LogWarning("{User} does not have permission to upload.", Context.User.Username);
                return;
            }

            _logger.LogInformation("Logging to {Channel}. File: {Filename}", channelId, filename);

            var fileInfo = _exportLogReader.GetFile(category, filename);
            if (fileInfo == null)
            {
                _logger.LogWarning("Unable to locate {Filename} in {Directory}.", filename, category);
                return;
            }

            var exportedLog = await _exportLogReader.DeserializeFileAsync(fileInfo);
            var channel = Context.Guild.GetTextChannel(channelId);
            if (channel == null)
            {
                _logger.LogWarning("Unable to get {Channel} to upload message to.", channelId);
                return;
            }
            else if (channel.Guild == null)
            {
                _logger.LogWarning("channel.Guild is null. Context: {Context}", Context);
                return;
            }

            _logger.LogInformation("Queuing work. File: {Filename}. Number of Messages: {NumberOfMessages}",
                filename, exportedLog.Messages.Count);

            var folder = Path.Combine(_contentRoot, category);
            var workItem = new UploadLogWork(Context.Channel, exportedLog, channel, filename, folder, skipBotMessages,
                _workItemLogger);

            _workQueue.QueueBackgroundWorkItem(workItem);
            _logger.LogDebug("\t- Queued work. File: {Filename}.", filename);
        }
    }
}
