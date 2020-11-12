using System.Collections.Generic;
using dtail.Config;

namespace dtail
{
    public class ContainerInfo : IContainerSaveableInfo
    {
        public string ShortName { get; set; }
        public bool IsVisible { get; set; }
        public IEnumerable<string> Aliases { get; set; }
        public IEnumerable<string> VisibleInChannels { get; set; }
    }
}
