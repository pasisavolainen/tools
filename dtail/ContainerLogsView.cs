using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using dtail.Config;

namespace dtail
{
    public class ContainerLogsView
    {
        public const string SYSTEM_CONTAINER_ID = "sys";
        public const string SYSTEM_SHORTNAME_ID = "!sys";
        public Dictionary<string, RunningContainerInfo> Containers { get; set; } = new();

        public event EventHandler<LogLine>? LineArrived;

        internal bool HasContainer(string containerId)
            => Containers.ContainsKey(containerId);

        internal void AddContainer(RunningContainerInfo c)
            => Containers.TryAdd(c.Id, c);

        public LinkedList<LogLine> LogLines { get; } = new();
        public DTailConfig Config { get; private set; } = new();

        internal void AddLogLine(string containerId, DateTime dt, string logLine)
        {
            if(!Containers.TryGetValue(containerId, out var container))
            {
                Trace.WriteLine($"Container {containerId} not found, skipping");
                return;
            }

            var ll = container.AddLogLine(dt, logLine);
            LogLines.AddLast(ll);
            LineArrived?.Invoke(this, ll);
        }

        internal void SeedContainers(DTailConfig config)
        {
            Config = config;

            // add virtual container for system messages
            var systemCI = GetTemplate(SYSTEM_CONTAINER_ID, () => new () {
                ShortName = SYSTEM_SHORTNAME_ID, Aliases = new[] {SYSTEM_CONTAINER_ID}
                });

            AddContainer(new(SYSTEM_CONTAINER_ID, systemCI, new Progress<string>(), Task.CompletedTask, new()));
        }

        internal void Refresh()
        {
            LineArrived?.Invoke(this, default!);
        }

        internal bool RenameContainer(RunningContainerInfo container, string newShortname)
        {
            if (string.IsNullOrWhiteSpace(newShortname))
                return false;

            var exists = Containers.Values.Any(c => c != container && c.Aliases.Contains(newShortname));
            if (exists)
                return false;

            container.ShortName = newShortname;

            return true;
        }

        internal ContainerInfo GetTemplate(string containerRunningName, Func<ContainerInfo> fallback)
            => Config.ContainerInfos?.Where(c => c.Aliases.Contains(containerRunningName)).FirstOrDefault()
                        ?? fallback();

        internal void LogSys(string message)
            => AddLogLine(SYSTEM_CONTAINER_ID, DateTime.UtcNow, message);

        internal RunningContainerInfo GetCurrentContainerOrSystem()
        {
            return Containers[SYSTEM_CONTAINER_ID];
        }
    }
}