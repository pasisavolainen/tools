using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace dtail
{
    public class ContainerInfo
    {
        public string Id { get; internal set; }
        public string ShortName { get; set; }
        public bool IsVisible { get; set; }
        public List<string> Aliases { get; set; }
        public Progress<string> Progress { get; internal set; }
        public CancellationTokenSource LogCancellation { get; internal set; }

        public Task LogTask { get; internal set; }

        public LinkedList<LogLine> LogLines { get; } = new LinkedList<LogLine>();

        internal LogLine AddLogLine(DateTime dt, string logLine)
        {
            var line = new LogLine {
                ContainerId = Id,
                Time = dt, Line = logLine };
            LogLines.AddLast(line);
            return line;
        }
    }
    public class LogLine
    {
        public string ContainerId { get; internal set; }
        public DateTime Time { get; set; }
        public string Line { get; set; }
    }
}
