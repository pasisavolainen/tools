using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using dtail.Config;

namespace dtail
{
    public class RunningContainerInfo : IContainerSaveableInfo
    {
        public string Id { get; internal set; }
        public ContainerInfo Info { get; set; }
        public string ShortName { get => Info.ShortName; set => Info.ShortName = value; }
        public bool IsVisible { get => Info.IsVisible ; set => Info.IsVisible = value; }
        public IEnumerable<string> Aliases { get => Info.Aliases; set => Info.Aliases = value; }
        public Progress<string> Progress { get; internal set; }
        public CancellationTokenSource LogCancellation { get; internal set; }

        public Task LogTask { get; internal set; }

        public LinkedList<LogLine> LogLines { get; } = new LinkedList<LogLine>();
        IEnumerable<string> IContainerSaveableInfo.Aliases { get => Aliases; set => Aliases = value.ToList(); }
        IEnumerable<string> IContainerSaveableInfo.VisibleInChannels { get => null; set => _ = value; }

        internal LogLine AddLogLine(DateTime dt, string logLine)
        {
            var line = new LogLine(Id, dt, logLine);
            LogLines.AddLast(line);
            return line;
        }
    }
}
