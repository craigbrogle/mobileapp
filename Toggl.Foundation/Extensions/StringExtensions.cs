using System.Collections.Generic;
using System.Linq;

namespace Toggl.Foundation.Extensions
{
    public static class StringExtensions
    {
        public static IList<string> SplitToQueryWords(this string text)
            => text.Split(' ')
                .Where(word => !string.IsNullOrEmpty(word))
                .Distinct()
                .ToList();
    }
}
