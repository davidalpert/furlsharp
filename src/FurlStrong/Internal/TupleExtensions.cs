using System;
using System.Collections.Generic;

namespace FurlStrong.Internal
{
    public static class TupleExtensions
    {
        public static KeyValuePair<T1, T2> ToKeyValuePair<T1, T2>(this Tuple<T1, T2> tuple)
        {
            return new KeyValuePair<T1, T2>(tuple.Item1, tuple.Item2);
        }
    }
}