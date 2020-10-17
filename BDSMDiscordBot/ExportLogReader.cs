using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using BDSMDiscordBot.Configuration;
using BDSMDiscordBot.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BDSMDiscordBot
{
    public class ExportLogReader
    {
        private readonly JsonSerializerOptions _jsonSerializerOptions;
        private readonly AppConfiguration _options;
        private readonly ILogger<ExportLogReader> _logger;
        private readonly DirectoryInfo _rootDirectory;
        private readonly Dictionary<string, Lazy<List<FileInfo>>> _files;

        public ExportLogReader(JsonSerializerOptions jsonSerializerOptions,
            IOptions<AppConfiguration> options, ILogger<ExportLogReader> logger)
        {
            _jsonSerializerOptions = jsonSerializerOptions
                ?? throw new ArgumentNullException(nameof(jsonSerializerOptions), "'jsonSerializerOptions' is null");
            _options = options.Value
                ?? throw new ArgumentNullException(nameof(options), "'options' is null");
            _logger = logger;

            if (string.IsNullOrEmpty(_options.RootDirectory))
            {
                throw new ArgumentException("appConfiguration.RootDirectory is null.");
            }

            _rootDirectory = new DirectoryInfo(_options.RootDirectory);
            if (!_rootDirectory.Exists)
            {
                throw new ArgumentException($"{_rootDirectory.FullName} does not exist.");
            }

            var allDirectories = _rootDirectory.GetDirectories();
            _files = allDirectories.ToDictionary(
                directory => directory.Name,
                directory => new Lazy<List<FileInfo>>(() => directory.GetFiles().ToList()));
        }

        public FileInfo GetFile(string directory, string filename)
        {
            if (string.IsNullOrEmpty(directory))
            {
                throw new ArgumentException($"{nameof(directory)} cannot be null or empty");
            }
            else if (string.IsNullOrEmpty(filename))
            {
                throw new ArgumentException($"{nameof(filename)} cannot be null or empty");
            }

            if (_files.TryGetValue(directory, out var lazyFiles))
            {
                return lazyFiles.Value.FirstOrDefault(file => file.Name.Equals(filename));
            }

            var directoryInfo = _rootDirectory.EnumerateDirectories()
                    .FirstOrDefault(x => x.Name.Equals(directory));

            if (directoryInfo == default)
            {
                _logger.LogWarning("{Directory} does not exist in root content.");
                return null;
            }

            _logger.LogInformation("{Directory} did not exist before. Adding.");

            var lazy = new Lazy<List<FileInfo>>(() => directoryInfo.GetFiles().ToList());
            _files.Add(directoryInfo.Name, lazy);

            return lazy.Value.FirstOrDefault(file => file.Name.Equals(filename));
        }

        public async IAsyncEnumerable<ExportedLog> DeserializeFilesAsync(IList<string> filenames)
        {
            var includeAllFiles = filenames == null || !filenames.Any();
            var set = new HashSet<string>(filenames ?? Enumerable.Empty<string>());

            _logger.LogInformation($"Fetching files from: {_rootDirectory.FullName}");

            foreach (var file in _rootDirectory.GetFiles().Where(x => includeAllFiles || set.Contains(x.Name)))
            {

                var exportedLog = await DeserializeFileAsync(file).ConfigureAwait(false);
                yield return exportedLog;
            }

            _logger.LogInformation("Completed.");
        }

        public async Task<ExportedLog> DeserializeFileAsync(FileInfo file)
        {
            using (var streamReader = file.OpenRead())
            {
                _logger.LogInformation($"\tReading: {file.FullName}");

                var exportedLog = await JsonSerializer.DeserializeAsync<ExportedLog>(streamReader, _jsonSerializerOptions).ConfigureAwait(false);

                _logger.LogInformation($"\tFinished: {file.FullName}");
                return exportedLog;
            }
        }
    }
}
