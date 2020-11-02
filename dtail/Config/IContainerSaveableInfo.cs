using System.Collections.Generic;

namespace dtail.Config
{
    public interface IContainerSaveableInfo
    {
        string ShortName { get; set; }
        IEnumerable<string> Aliases { get; set; }
        IEnumerable<string> VisibleInChannels { get; set; }
    }
}
