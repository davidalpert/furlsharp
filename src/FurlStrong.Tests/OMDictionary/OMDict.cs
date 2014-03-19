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

        public OMDict(params string[] pairs)
        {
            _items = new Dictionary<string, SortedDictionary<int, string>>();
            Load(pairs);
        }

        public IEnumerable<string> Keys { get { return _items.Keys; } }

        public IEnumerable<string> Values(string key = null)
        {
            return Items(key).Select(v => v.Value);
        }

        public IEnumerable<string> AllKeys { get { return AllItems().Select(x => x.Key); } }

        public IEnumerable<string> AllValues { get { return AllItems().Select(x => x.Value); } }

        public string this[string key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }

        public IEnumerable<KeyValuePair<string, string>> Items(string key = null)
        {
            return key.IsNotNullOrWhiteSpace()
                       ? AllItems(key)
                       : _items.Select(k => new KeyValuePair<string, string>(k.Key, k.Value.First().Value));
        }

        public IEnumerable<KeyValuePair<string, string>> AllItems(string key = null)
        {
            var items = key.IsNotNullOrWhiteSpace()
                            ? _items.Where(i => i.Key == key)
                            : _items;
            return
                items.SelectMany(k => k.Value.Select(v => Tuple.Create(v.Key, k.Key, v.Value)))
                      .OrderBy(x => x.Item1)
                      .Select(x => new KeyValuePair<string, string>(x.Item2, x.Item3));
        }

        public IEnumerable<IEnumerable<string>> Lists()
        {
            return _items.Select(k => k.Value.OrderBy(v => v.Key).Select(v => v.Value));
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
                       : _items.Where(i => i.Value.Count > 0)
                               .Max(k => k.Value.Keys.Max());
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
            var pairs = values.Select(v => new KeyValuePair<string, string>(key, v));

            UpdateAll(pairs, true);
           
            return this;
        }

        public string SetDefault(string key, string value = null)
        {
            if (_items.ContainsKey(key))
                return _items[key].First().Value;

            var nextAvailableIndex = MaxIndex() + 1;
            _items[key] = new SortedDictionary<int, string>
                {
                    {nextAvailableIndex, value}
                };

            return value;
        }

        public IEnumerable<string> SetDefaultList(string key, params string[] values)
        {
            if (_items.ContainsKey(key))
                return _items[key].Select(x => x.Value);

            _items[key] = new SortedDictionary<int, string>();

            values = values.Length > 0
                         ? values
                         : new string[] {null};

            var nextAvailableIndex = MaxIndex() + 1;
            values.ForEach(x => _items[key].Add(nextAvailableIndex++, x));

            return values;
        }

        public OMDict Add(string key, string value = null)
        {
            var nextAvailableIndex = MaxIndex() + 1;

            if (_items.ContainsKey(key) == false)
                _items[key] = new SortedDictionary<int, string>();

            _items[key].Add(nextAvailableIndex, value);

            return this;
        }

        public OMDict AddList(string key, params string[] values)
        {
            var nextAvailableIndex = MaxIndex() + 1;

            if (_items.ContainsKey(key) == false)
                _items[key] = new SortedDictionary<int, string>();

            values = values.Length > 0
                         ? values
                         : new string[] {null};

            var dict = _items[key];
            values.ForEach(v => dict.Add(nextAvailableIndex++, v));

            return this;
        }

        public string Pop(string key, string defaultValue = null)
        {
            if (_items.ContainsKey(key) == false && defaultValue == null) 
                throw new InvalidOperationException("Must provide a defaultValue when key is not present in the dictionary.");

            var value = Get(key, defaultValue);

            Remove(key);

            return value;
        }

        public IEnumerable<string> PopList(string key, params string[] defaultValues)
        {
            if (_items.ContainsKey(key) == false && defaultValues.Length == 0) 
                throw new InvalidOperationException("Must provide a defaultValue when key is not present in the dictionary.");

            var values = GetList(key, defaultValues)
                                .ToArray(); // force enumeration before removing the list of values.

            Remove(key);

            return values;
        }

        public string PopValue(string key, string defaultValue = null, bool last = true)
        {
            if (_items.ContainsKey(key) == false)
            {
                if (defaultValue == null)
                    throw new InvalidOperationException(
                        "Must provide a default value when key is not present in the dictionary.");

                return defaultValue;
            }

            var list = _items[key];
            if (list.Values.Any())
            {
                var lastItem = last ? list.Last() : list.First();
                list.Remove(lastItem.Key);
                return lastItem.Value;
            }

            return defaultValue;
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
                    pair.Key, 
                    pair.Value,
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
            var pairs = PairUp(args);

            UpdateAll(pairs);
        }

        private void UpdateAll(IEnumerable<KeyValuePair<string, string>> pairs, bool removeExtra = false)
        {
            var updatedIndexes = new Dictionary<string, int>();
            var maxIndex = MaxIndex();

            foreach (var pair in pairs)
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

            if (removeExtra)
            {
                updatedIndexes.Keys.ForEach(key =>
                    {
                        var list = _items[key];
                        var maxUpdatedIndex = updatedIndexes[key];
                        var itemsToRemove = list.Keys
                                                .Where(index => index > maxUpdatedIndex)
                                                .ToArray();

                        itemsToRemove.ForEach(index => list.Remove(index));
                    });
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