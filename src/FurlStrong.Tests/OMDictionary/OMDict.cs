using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Furlstrong.Tests.OMDictionary
{
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class OMDict
    {
        private string DebuggerDisplay
        {
            get
            {
                return string.Format("[{0}]",
                                     string.Join(", ", AllItems().Select(p => string.Format("({0}, {1})",
                                                                                        p.Key, p.Value))));
            }
        }

        private Dictionary<string, SortedDictionary<int, string>> _items;
        private List<KeyValuePair<string, string>> _pairs;

        public OMDict(params string[] pairs)
        {
            _items = new Dictionary<string, SortedDictionary<int, string>>();
            Load(pairs);
        }

        public string this[string key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }

        public IEnumerable<KeyValuePair<string, string>> Items()
        {
            return _items.Select(k => new KeyValuePair<string, string>(k.Key,k.Value.First().Value));
        }

        public IEnumerable<KeyValuePair<string, string>> AllItems()
        {
            return
                _items.SelectMany(k => k.Value.Select(v => Tuple.Create(v.Key, k.Key, v.Value)))
                      .OrderBy(x => x.Item1)
                      .Select(x => new KeyValuePair<string, string>(x.Item2, x.Item3));
        }

        public string Get(string key, string defaultValue)
        {
            return Get(key) ?? defaultValue;
        }

        public string Get(string key)
        {
            return GetList(key).FirstOrDefault();
        }

        public IEnumerable<string> GetList(string key, params string[] defaultValues)
        {
            return _items.ContainsKey(key)
                       ? _items[key].Values.AsEnumerable()
                       : defaultValues;
        }

        public int Count()
        {
            return _items.Sum(k => k.Value.Count);
        }

        private int MaxIndex()
        {
            return _items.Count == 0
                       ? -1
                       : _items.Max(k => k.Value.Keys.Max());
        }

        public OMDict Set(string key, string value)
        {
            if (_items.ContainsKey(key))
            {
                var indexOfFirstValue = _items[key].Keys.First();
                _items[key].Clear();
                _items[key][indexOfFirstValue] = value;
            }
            else
            {
                var indexOfNextValue = MaxIndex() + 1;
                _items[key] = new SortedDictionary<int, string>();
                _items[key][indexOfNextValue] = value;
            }

            return this;
        }

        public OMDict SetList(string key, params string[] values)
        {
            /*
            var numberOfNewValues = values.Length;
            var numberOfExistingValues = _items.ContainsKey(key) ? _items[key].Values.Count : 0;
            var numberOfReplacedValues = 0;

            var targetList = _items.ContainsKey(key) 
                ? _items[key] 
                : new SortedDictionary<int, string>();

            var replaced = new List<int>();

            //var pairs = values.Select(v => new KeyValuePair<string, string>(key, v)) .ToArray();

            foreach (var newValue in values)
            {
                var q = _pairs.Where(p => p.Key == pair.Key && replaced.Contains(p) == false)
                              .ToArray();

                if (_items.ContainsKey(key))
                {

                }
                else
                {
                    
                }

                if (q.Any())
                {
                    var existing = q.First();
                    var index = _pairs.IndexOf(existing);
                    _pairs[index] = pair;
                    replaced.Add(pair);
                }
                else
                {
                    _pairs.Add(pair);
                }
            }
             */

            return this;
        }

        public void Remove(string key)
        {
            if (_items.ContainsKey(key))
            {
                _items[key].Clear();
                _items.Remove(key);
            }
        }

        public void Load(params string[] args)
        {
            _items.Clear();
            var x = PairUp(args).Select((pair, index) => new
                {
                    Key = pair.Key,
                    Value = pair.Value,
                    Index = index
                });
            foreach (var item in x)
            {
                if (_items.ContainsKey(item.Key) == false)
                {
                    _items[item.Key] = new SortedDictionary<int, string>();
                }

                _items[item.Key][item.Index] = item.Value;
            }
        }

        public void Update(params string[] args)
        {
            foreach (var pair in PairUp(args))
            {
                Set(pair.Key, pair.Value);
            }
        }

        public void UpdateAll(params string[] args)
        {
            var updatedIndexes = new Dictionary<string, int>();
            var maxIndex = MaxIndex();

            foreach (var pair in PairUp(args))
            {
                if (updatedIndexes.ContainsKey(pair.Key) == false)
                {
                    updatedIndexes[pair.Key] = -1;
                }

                if (_items.ContainsKey(pair.Key))
                {
                    var highestUpdatedIndex = updatedIndexes[pair.Key];
                    var q = _items[pair.Key].Where(x => x.Key > highestUpdatedIndex).ToArray();
                    var indexToUpdate = q.Any() ? q.First().Key : (maxIndex++) + 1;
                    _items[pair.Key][indexToUpdate] = pair.Value;
                    updatedIndexes[pair.Key] = indexToUpdate; // so that we don't update it again.
                }
                else
                { 
                    _items[pair.Key] = new SortedDictionary<int, string>();
                    var indexToUpdate = (maxIndex++) + 1;
                    _items[pair.Key][indexToUpdate] = pair.Value;
                    updatedIndexes[pair.Key] = indexToUpdate;
                }
            }
        }

        private IEnumerable<KeyValuePair<string, string>> PairUp(string[] items)
        {
            var result = new List<KeyValuePair<string, string>>();
            for (var i = 0; i+1 < items.Length; i += 2)
            {
                result.Add(new KeyValuePair<string, string>(items[i],items[i+1]));
            }

            return result;
        }
    }

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