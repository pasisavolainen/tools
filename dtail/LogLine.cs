using System;

namespace dtail
{
    public record LogLine(string ContainerId, DateTime Time, string Line)
    {
    }
}
