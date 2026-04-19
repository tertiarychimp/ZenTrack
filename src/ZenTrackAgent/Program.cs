using System;
using System.IO;

namespace ZenTrackAgent
{
    internal static class Program
    {
        private static int Main(string[] args)
        {
            try
            {
                string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
                string configPath = Path.Combine(baseDirectory, "ZenTrackAgent.config.json");
                CommandLineOptions options = CommandLineOptions.Parse(args);

                AgentConfig config = AgentConfig.Load(configPath);
                Directory.CreateDirectory(config.GetDataDirectory(baseDirectory));

                using (TrackerDatabase database = new TrackerDatabase(config.GetDatabasePath(baseDirectory)))
                {
                    if (options.ServeUi)
                    {
                        database.Initialize();
                        using (GroupDatabase groupDatabase = new GroupDatabase(config.GetGroupDatabasePath(baseDirectory)))
                        {
                            groupDatabase.Initialize();
                            using (WebUiServer server = new WebUiServer(database, groupDatabase, options.Port))
                            {
                                server.Run(options.MaxRuntime);
                            }
                        }
                    }
                    else
                    {
                        using (ZenUsageTracker tracker = new ZenUsageTracker(config, database))
                        {
                            tracker.Run(options.MaxRuntime);
                        }
                    }
                }

                return 0;
            }
            catch (ConfigurationException ex)
            {
                Console.Error.WriteLine("Configuration error: " + ex.Message);
                return 2;
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine("Fatal error: " + ex);
                return 1;
            }
        }
    }
}
