using System.Collections.Generic;
using System.Linq;

namespace dtail.Config
{
    public class DTailConfig
    {
        public DTailConfig()
            => ContainerInfos = Enumerable.Empty<ContainerInfo>();
        public IEnumerable<ContainerInfo> ContainerInfos { get; set; }
    }
}