using System;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Docker.DotNet;
using Docker.DotNet.Models;
using dtail.Config;
using dtail.Extensions;

namespace dtail
{
    public class ContainerRunner
    {
        public DockerClient DockerClient { get; }
        public ContainerLogsView ContainerLogsView { get; }
        public DTailConfig Config { get; }

        public ContainerRunner(DockerClient dockerClient,
                               ContainerLogsView containerLogsView,
                               DTailConfig config)
        {
            (DockerClient, ContainerLogsView, Config)
            = (dockerClient, containerLogsView, config);

            ContainerLogsView.SeedContainers(config);
        }

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

        internal DTailConfig CollectConfig()
            => new () {
                ContainerInfos = ContainerLogsView.Containers.Values.Select(kv => kv.Info),
            };

        private async Task RefreshContainers()
        {
            var conts = await DockerClient.Containers.ListContainersAsync(new () { All = true });

            var clp = new ContainerLogsParameters { Follow = true, ShowStderr = true, ShowStdout = true, Timestamps = true };
            foreach (var container in conts)
            {
                if(ContainerLogsView.HasContainer(container.ID))
                    continue;

                ContainerLogsView.AddLogLine("sys", DateTime.UtcNow, $"Found new container: {container.Dump()}");

                var ci = ContainerLogsView.GetTemplate(container.Names.FirstOrDefault(),
                            () => new ()
                            {
                                ShortName = container.Names.FirstOrDefault() ?? container.ID,
                                Aliases = container.Names.Union(new[] { container.ID }).ToList(),
                            });

                var c = new RunningContainerInfo
                {
                    Id = container.ID,
                    Info = ci,
                    Progress = new Progress<string>(logLine => ProcessLogline(container.ID, logLine)),
                    LogCancellation = new CancellationTokenSource(),
                };
                // need it here for mapping that starts as soon as container log starts ticking
                ContainerLogsView.AddContainer(c);

                Trace.WriteLine($"Found: {c.ShortName}");

                c.LogTask = DockerClient.Containers.GetContainerLogsAsync(container.ID, clp, c.LogCancellation.Token, c.Progress);
            }
        }

        private void ProcessLogline(string containerId, string logLine)
        {
            var dt = DateTime.UtcNow;
            // probably fucks up non-latin
            logLine = new string(logLine.Where(c => c > 10).ToArray());
            var rxMatch = Regex.Match(logLine, @".{0,8}?(?<dt>\d{4}-\d{2}-\d{2}T)");
            var idxDt = rxMatch.Groups["dt"].Index;
            if (logLine.Length > 32 && (idxDt >= 0 && idxDt < 8))
            {
                var timeString = logLine.Substring(idxDt, 30);
                dt = DateTime.Parse(timeString);
                logLine = logLine[(idxDt + 31)..];
            }
            ContainerLogsView.AddLogLine(containerId, dt, logLine);
            Trace.WriteLine($"{dt:HH:mm:ss} <{containerId}> {logLine}");
        }

        private void OnMonitorProgress(Message obj)
        {
            Trace.WriteLine(obj.ToString());
        }
    }
}
