using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace dtail
{
    public class ContainerInfo
    {
        public string ShortName { get; set; }
        public bool IsVisible { get; set; }
        public List<string> Aliases { get; set; }
        public Progress<string> Progress { get; internal set; }
        public CancellationTokenSource LogCancellation { get; internal set; }
        public string Id { get; internal set; }
        public Task LogTask { get; internal set; }
    }
}
