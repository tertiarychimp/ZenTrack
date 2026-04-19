using System;

namespace ZenTrackAgent
{
    internal sealed class CommandLineOptions
    {
        public TimeSpan? MaxRuntime { get; private set; }

        public bool ServeUi { get; private set; }

        public int Port { get; private set; }

        public static CommandLineOptions Parse(string[] args)
        {
            CommandLineOptions options = new CommandLineOptions
            {
                Port = 8787
            };

            if (args == null)
            {
                return options;
            }

            foreach (string arg in args)
            {
                if (string.Equals(arg, "--serve-ui", StringComparison.OrdinalIgnoreCase))
                {
                    options.ServeUi = true;
                    continue;
                }

                const string runtimePrefix = "--run-for-seconds=";
                if (arg.StartsWith(runtimePrefix, StringComparison.OrdinalIgnoreCase))
                {
                    options.MaxRuntime = ParsePositiveSeconds(arg.Substring(runtimePrefix.Length));
                    continue;
                }

                const string portPrefix = "--port=";
                if (arg.StartsWith(portPrefix, StringComparison.OrdinalIgnoreCase))
                {
                    options.Port = ParsePort(arg.Substring(portPrefix.Length));
                    continue;
                }
            }

            return options;
        }

        private static TimeSpan ParsePositiveSeconds(string value)
        {
            int seconds;
            if (!int.TryParse(value, out seconds) || seconds <= 0)
            {
                throw new ConfigurationException("Invalid value for --run-for-seconds. Expected a positive integer.");
            }

            return TimeSpan.FromSeconds(seconds);
        }

        private static int ParsePort(string value)
        {
            int port;
            if (!int.TryParse(value, out port) || port < 1 || port > 65535)
            {
                throw new ConfigurationException("Invalid value for --port. Expected an integer from 1 to 65535.");
            }

            return port;
        }
    }
}
