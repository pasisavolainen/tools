using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace dtail
{
    public class ContainerLogsView
    {
        public Dictionary<string, ContainerInfo> Containers { get; set; } = new Dictionary<string, ContainerInfo>();

        public event EventHandler<LogLine> LineArrived;

        internal bool HasContainer(string containerId)
            => Containers.ContainsKey(containerId);

        internal void AddContainer(ContainerInfo c)
            => Containers.TryAdd(c.Id, c);

        public LinkedList<LogLine> LogLines { get; } = new LinkedList<LogLine>();

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

        internal void Refresh()
        {
            LineArrived?.Invoke(this, null);
        }
    }
}