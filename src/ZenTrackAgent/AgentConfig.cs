using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;

namespace ZenTrackAgent
{
    [DataContract]
    internal sealed class AgentConfigFile
    {
        [DataMember(Name = "databasePath")]
        public string DatabasePath { get; set; }

        [DataMember(Name = "pollIntervalSeconds")]
        public int PollIntervalSeconds { get; set; }

        [DataMember(Name = "processNames")]
        public List<string> ProcessNames { get; set; }

        [DataMember(Name = "logToConsole")]
        public bool LogToConsole { get; set; }
    }

    internal sealed class AgentConfig
    {
        public string DatabasePath { get; private set; }

        public TimeSpan PollInterval { get; private set; }

        public IList<string> ProcessNames { get; private set; }

        public bool LogToConsole { get; private set; }

        public static AgentConfig Load(string path)
        {
            if (!File.Exists(path))
            {
                throw new ConfigurationException("Config file not found: " + path);
            }

            AgentConfigFile fileConfig;
            DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(AgentConfigFile));
            using (FileStream stream = File.OpenRead(path))
            {
                fileConfig = (AgentConfigFile)serializer.ReadObject(stream);
            }

            if (fileConfig == null)
            {
                throw new ConfigurationException("Config file could not be read.");
            }

            List<string> cleanedProcessNames = (fileConfig.ProcessNames ?? new List<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Select(NormalizeProcessName)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (cleanedProcessNames.Count == 0)
            {
                throw new ConfigurationException("At least one process name must be configured.");
            }

            int pollIntervalSeconds = fileConfig.PollIntervalSeconds <= 0 ? 60 : fileConfig.PollIntervalSeconds;

            string databasePath = string.IsNullOrWhiteSpace(fileConfig.DatabasePath)
                ? @"%LOCALAPPDATA%\ZenTrack\data\zentrack.db"
                : fileConfig.DatabasePath.Trim();

            return new AgentConfig
            {
                DatabasePath = databasePath,
                PollInterval = TimeSpan.FromSeconds(pollIntervalSeconds),
                ProcessNames = cleanedProcessNames,
                LogToConsole = fileConfig.LogToConsole
            };
        }

        public string GetDataDirectory(string baseDirectory)
        {
            string databasePath = GetDatabasePath(baseDirectory);
            string directory = Path.GetDirectoryName(databasePath);
            return string.IsNullOrWhiteSpace(directory) ? baseDirectory : directory;
        }

        public string GetGroupDatabasePath(string baseDirectory)
        {
            return Path.Combine(GetDataDirectory(baseDirectory), "GroupDB.db");
        }

        public string GetDatabasePath(string baseDirectory)
        {
            string expandedPath = Environment.ExpandEnvironmentVariables(DatabasePath);

            if (Path.IsPathRooted(expandedPath))
            {
                return expandedPath;
            }

            return Path.GetFullPath(Path.Combine(baseDirectory, expandedPath));
        }

        public static string NormalizeProcessName(string processName)
        {
            string trimmed = processName.Trim();
            if (trimmed.EndsWith(".exe", StringComparison.OrdinalIgnoreCase))
            {
                trimmed = trimmed.Substring(0, trimmed.Length - 4);
            }

            return trimmed;
        }
    }

    internal sealed class ConfigurationException : Exception
    {
        public ConfigurationException(string message) : base(message)
        {
        }
    }
}
