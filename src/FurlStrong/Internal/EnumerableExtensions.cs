using System;
using System.Collections.Generic;

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
    }
}