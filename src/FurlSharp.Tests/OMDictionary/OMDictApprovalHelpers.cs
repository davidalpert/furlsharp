using System;
using System.Collections.Generic;
using System.Linq;

namespace FurlSharp.Tests.OMDictionary
{
    /// <summary>
    /// Assist in formatting kvp and tuples for review and approval.
    /// </summary>
    public static class OMDictApprovalHelpers
    {
        public static string FormatForApproval<TKey, TValue>(this IEnumerable<Tuple<TKey, TValue>> items)
        {
            return string.Format("[{0}]", string.Join(", ", items.Select(p => p.ToString())));
        }

        public static string FormatForApproval<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return string.Format("[{0}]", string.Join(", ", items.Select(p => string.Format("({0}, {1})", p.Key, p.Value))));
        }
    }
}