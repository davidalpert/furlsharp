using System;
using System.Collections.Generic;
using System.Linq;

namespace FurlStrong.Internal
{
    public static class EnumerableExtensions
    {
        public static void ForEach<T>(this IEnumerable<T> items, Action<T> act)
        {
            if (act == null) return;

            items.ForEach((item,index) => act(item));
        }

        public static void ForEach<T>(this IEnumerable<T> items, Action<T,int> act)
        {
            if (act == null) return;

            var i = 0;
            foreach (var item in items)
            {
                act(item, i);
            }
        }

        public static void RemoveFirst<T>(this List<T> items)
        {
            if (items == null || items.Count == 0) return;
            items.RemoveAt(0);
        }

        public static void RemoveLast<T>(this List<T> items)
        {
            if (items == null) return;
            var count = items.Count;
            if (count == 0) return;
            items.RemoveAt(count - 1);
        }
    }
}