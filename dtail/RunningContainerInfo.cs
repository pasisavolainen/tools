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
        public RunningContainerInfo(string id, ContainerInfo info, Progress<string> progress,
                                    Task logTask, CancellationTokenSource logCancellation)
            => (Id, Info, Progress, LogTask, LogCancellation)
             = (id, info, progress, logTask, logCancellation);
        public string Id { get; internal set; }
        public ContainerInfo Info { get; set; }
        public Progress<string> Progress { get; internal set; }
        public CancellationTokenSource LogCancellation { get; internal set; }
        public string ShortName { get => Info.ShortName; set => Info.ShortName = value; }
        public bool IsVisible { get => Info.IsVisible ; set => Info.IsVisible = value; }
        public IEnumerable<string> Aliases { get => Info.Aliases; set => Info.Aliases = value; }

        public Task LogTask { get; internal set; }

        public LinkedList<LogLine> LogLines { get; } = new ();
        IEnumerable<string> IContainerSaveableInfo.Aliases { get => Aliases; set => Aliases = value.ToList(); }
        IEnumerable<string> IContainerSaveableInfo.VisibleInChannels { get => Enumerable.Empty<string>(); set => _ = value; }

        internal LogLine AddLogLine(DateTime dt, string logLine)
        {
            var line = new LogLine(Id, dt, logLine);
            LogLines.AddLast(line);
            return line;
        }
    }
}
