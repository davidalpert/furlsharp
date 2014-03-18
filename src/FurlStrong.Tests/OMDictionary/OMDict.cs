using System;
using System.Collections.Generic;
using System.Linq;

namespace Furlstrong.Tests.OMDictionary
{
    public class OMDict<TKey, TValue>
        where TKey : IComparable<TKey>
    {
        private List<KeyValuePair<TKey, TValue>> _pairs;

        public OMDict(params Tuple<TKey, TValue>[] pairs)
        {
            Load(pairs);
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> Items()
        {
            return _pairs.GroupBy(p => p.Key)
                         .Select(g => new KeyValuePair<TKey, TValue>(g.Key, g.First().Value));
        }

        public IEnumerable<KeyValuePair<TKey, TValue>> AllItems()
        {
            return _pairs;
        }

        public TValue Get(TKey key)
        {
            var firstPair = _pairs.FirstOrDefault(p => p.Key.Equals(key));
            return firstPair.Value;
        }

        public IEnumerable<TValue> GetList(TKey key)
        {
            return _pairs.Where(p => p.Key.Equals(key)).Select(p => p.Value);
        }

        public void Load(params Tuple<TKey, TValue>[] pairs)
        {
            _pairs = pairs.Select(t => new KeyValuePair<TKey, TValue>(t.Item1, t.Item2))
                          .ToList();
        }

        public void Update(params Tuple<TKey, TValue>[] pairs)
        {
            foreach (var tuple in pairs)
            {
                var existingPair = _pairs.FirstOrDefault(p => p.Key.Equals(tuple.Item1));
                if (default(KeyValuePair<TKey, TValue>).Equals(existingPair))
                {
                    var index = _pairs.IndexOf(existingPair);
                    _pairs[index] = new KeyValuePair<TKey, TValue>(existingPair.Key, tuple.Item2);
                }
                else
                {
                    _pairs.Add(new KeyValuePair<TKey, TValue>(tuple.Item1, tuple.Item2));
                }
            }
        }

        public void UpdateAll(params Tuple<TKey, TValue>[] pairs)
        {
            _pairs = pairs.Select(t => new KeyValuePair<TKey, TValue>(t.Item1, t.Item2))
                          .ToList();
        }
    }
}