using System.Collections.Generic;
using System.Text.Json;

namespace dtail.Extensions
{
    public static class UtilityExtensions
    {
        public static string Dump(this object item)
            => JsonSerializer.Serialize(item);

        public static ListDataSource<string> AsSource(this IEnumerable<string> items)
            => new(items, alias => false) {
                FormatItem = (index, item) => item,
            };

    }
}
