using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace dtail
{
    public class ContainerRunner
    {
        public ContainerRunner(DockerClient dockerClient)
        {
            DockerClient = dockerClient;
        }

        public DockerClient DockerClient { get; }

        Dictionary<string, ContainerInfo> Containers { get; set; } = new Dictionary<string, ContainerInfo>();

        CancellationTokenSource MonitorCancellation { get; } = new CancellationTokenSource();
        Progress<Message> MonitorProgress { get; set; }
        Task MonitorTask { get; set; }

        internal async Task RunAsync()
        {
            MonitorProgress = new Progress<Message>(OnMonitorProgress);

            MonitorTask = DockerClient.System.MonitorEventsAsync(new ContainerEventsParameters(), MonitorProgress, MonitorCancellation.Token);

            while (true)
            {
                await Task.Delay(1000);
                await RefreshContainers();
            }
        }

        private async Task RefreshContainers()
        {
            var conts = await DockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true });

            var clp = new ContainerLogsParameters { Follow = true, ShowStderr = true, ShowStdout = true, Timestamps = true };
            foreach (var container in conts)
            {
                if (Containers.ContainsKey(container.ID))
                    continue;

                var c = new ContainerInfo
                {
                    Id = container.ID,
                    ShortName = container.Names.FirstOrDefault() ?? container.ID,
                    Progress = new Progress<string>(logLine => ProcessLogline(container.ID, logLine)),
                    Aliases = container.Names.Union(new[] { container.ID }).ToList(),
                    LogCancellation = new CancellationTokenSource(),
                };
                // need it here for mapping that starts as soon as container log starts ticking
                Containers[container.ID] = c;
                Console.WriteLine($"Found: {c.ShortName}");

                c.LogTask = DockerClient.Containers.GetContainerLogsAsync(container.ID, clp, c.LogCancellation.Token, c.Progress);
            }
        }

        private void ProcessLogline(string containerId, string logLine)
        {
            Containers.TryGetValue(containerId, out var container);
            var name = container?.ShortName ?? containerId;
            var dt = DateTime.UtcNow;
            // probably fucks up non-latin
            logLine = new string(logLine.Where(c => c > 10).ToArray());
            var rxMatch = Regex.Match(logLine, @".{0,8}?(?<dt>\d{4}-\d{2}-\d{2}T)");
            var idxDt = rxMatch.Groups["dt"].Index;
            if (logLine.Length > 32 && (idxDt >= 0 && idxDt < 8))
            {
                var timeString = logLine.Substring(idxDt, 30);
                dt = DateTime.Parse(timeString);
                logLine = logLine.Substring(idxDt + 31);
            }
            Console.WriteLine($"{dt:HH:mm:ss} <{name}> {logLine}");
        }

        private void OnMonitorProgress(Message obj)
        {
            Console.WriteLine(obj.ToString());
        }
    }
}
