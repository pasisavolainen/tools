using System;
using System.Collections.Generic;
using System.Linq;
using dtail.Config;

namespace dtail
{
    public class ContainerInfo : IContainerSaveableInfo
    {
        public string ShortName { get; set; } = string.Empty;
        public bool IsVisible { get; set; } = true;
        public IEnumerable<string> Aliases { get; set; }
        public IEnumerable<string> VisibleInChannels { get; set; }

        public ContainerInfo()
        {
            ShortName = Guid.NewGuid().ToString();
            Aliases = VisibleInChannels = Enumerable.Empty<string>();
        }
    }
}
