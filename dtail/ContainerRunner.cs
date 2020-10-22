using System;
using System.Collections.Generic;
using System.Linq;
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

        internal async Task RunAsync()
        {
            var conts = await DockerClient.Containers.ListContainersAsync(new ContainersListParameters { All = true });

            Console.WriteLine($"Seeing {conts.Count} containers.");

            var clp = new ContainerLogsParameters { Follow = true, ShowStderr = true, ShowStdout = true, Timestamps = true };
            foreach (var container in conts)
            {
                var c = new ContainerInfo
                {
                    Id = container.ID,
                    ShortName = container.Names.FirstOrDefault() ?? container.ID,
                    Progress = new Progress<string>(logLine => SeeLog(container.ID, logLine)),
                    Aliases = container.Names.Union(new[] { container.ID }).ToList(),
                    LogCancellation = new CancellationTokenSource(),
                };
                Containers[container.ID] = c;
                await DockerClient.Containers.GetContainerLogsAsync(container.ID, clp, c.LogCancellation.Token, c.Progress);
            }

            MonitorProgress = new Progress<Message>(OnMonitorProgress);

            await DockerClient.System.MonitorEventsAsync(new ContainerEventsParameters(), MonitorProgress, MonitorCancellation.Token);

            while (true) await Task.Delay(10);
        }

        private void SeeLog(string containerId, string log)
        {
            Containers.TryGetValue(containerId, out var container);
            var name = container?.ShortName ?? containerId;
            Console.WriteLine($"<{name}> {log}");
        }

        private void OnMonitorProgress(Message obj)
        {
            Console.WriteLine(obj.ToString());
        }
    }
}
