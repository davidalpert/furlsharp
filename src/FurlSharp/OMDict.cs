using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using FurlSharp.Internal;
using FurlSharp;

namespace FurlSharp
{
    /// <summary>
    /// <see type="OMDict"/> is a .NET implementation of a python-based 
    /// Ordered Multivalue Dictionary (https://github.com/gruns/orderedmultidict).
    /// 
    /// A Multivalue Dictionary behaves like a Dictionary but can store 
    /// multiple values for each key.
    /// 
    /// An Ordered Multivalue Dictionary retains the order of insertions and deletions.
    /// </summary>
    /// <remarks>
    /// For simpliicity, this <see cref="OMDict"/> is constrained to keys and
    /// values of type <see cref="String"/>.
    /// </remarks>
    [DebuggerDisplay("{DebuggerDisplay,nq}")]
    public class OMDict 
    {
        // Provides a serialized view of the dictionary's contents for display in the debugger
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

        /// <summary>
        /// Create an <see cref="OMDict"/> from an array of key value pairs.
        /// </summary>
        /// <param name="pairs">key, value pairs as a collection of of string arguments.</param>
        public OMDict(params string[] pairs)
        {
            _items = new Dictionary<string, SortedDictionary<int, string>>();
            Load(pairs);
        }

        /// <summary>
        /// Returns each unique key in the dictionary.
        /// </summary>
        public IEnumerable<string> Keys { get { return _items.Keys; } }

        /// <summary>
        /// Returns each value in the dictionary.  If <paramref name="key"/>
        /// is provided, only the values for that key are returned.
        /// </summary>
        public IEnumerable<string> Values(string key = null)
        {
            return Items(key).Select(v => v.Value);
        }

        /// <summary>
        /// Returns the key of each value in the dictionary.  If a key has
        /// more than one value, it will be returned once for each value.
        /// </summary>
        public IEnumerable<string> AllKeys { get { return AllItems().Select(x => x.Key); } }

        /// <summary>
        /// Returns all the values in sequential order.
        /// </summary>
        public IEnumerable<string> AllValues { get { return AllItems().Select(x => x.Value); } }

        /// <summary>
        /// Returns the number of unique keys in the dictionary.
        /// </summary>
        public int Length {
            get { return _items.Keys.Count; }
        }

        /// <summary>
        /// Returns the number of values in the dictionary.
        /// </summary>
        public int Size {
            get { return _items.Sum(itemList => itemList.Value.Count); }
        }

        /// <summary>
        /// Returns the number of values in the dictionary.
        /// </summary>
        public int Count()
        {
            return _items.Sum(k => k.Value.Count);
        }

        /// <summary>
        /// Returns the highest index in use internally 
        /// by the dictionary.  Use this to ensure that
        /// added items are given an index that sorts
        /// them sequentially higher than the existing
        /// ones.  Not garunteed to match <see cref="Size"/>.
        /// </summary>
        private int MaxIndex()
        {
            return _items.Count == 0
                       ? -1
                       : _items.Where(i => i.Value.Count > 0)
                               .Max(k => k.Value.Keys.Max());
        }

        /// <summary>
        /// Provides key-based indexing into the stored values.
        /// 
        /// If <paramref name="key"/> has multiple values, 
        /// only the first value is returned.
        /// 
        /// If <paramref name="key"/> has multiple values,
        /// they will all be deleted and replaced with
        /// <code>value</code>.
        /// </summary>
        public string this[string key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }

        /// <summary>
        /// Provides zero-based indexing into the ordered 
        /// set of <see cref="KeyValuePair{string,string}"/>
        /// pairs.
        /// </summary>
        public KeyValuePair<string, string> this[int index]
        {
            get { return AllItems().ElementAt(index); }
        }

        /// <summary>
        /// Returns an ordered set of <see cref="KeyValuePair{string,string}"/>s 
        /// representing the first value for each key stored in the dictionary.
        /// 
        /// If a <paramref name="key"/> is provided, all the items for that key
        /// are returned.
        /// </summary>
        public IEnumerable<KeyValuePair<string, string>> Items(string key = null)
        {
            return key.IsNotNullOrWhiteSpace()
                       ? AllItems(key)
                       : _items.Select(k => new KeyValuePair<string, string>(k.Key, k.Value.First().Value));
        }

        /// <summary>
        /// Returns an ordered set of <see cref="KeyValuePair{string,string}"/>s 
        /// representing all the values in the dictionary, sorted in sequence.
        /// 
        /// If a <paramref name="key"/> is provided, all the items for that key
        /// are returned.
        /// </summary>
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

        /// <summary>
        /// Returns a a list of the values associated with each key,
        /// grouped such that each entry in the returned list is itself
        /// a list of values.
        /// </summary>
        public IEnumerable<IEnumerable<string>> Lists()
        {
            return _items.Select(k => k.Value.OrderBy(v => v.Key).Select(v => v.Value));
        }

        /// <summary>
        /// Gets the value associated with a key. If key has multiple values, only the first value is returned.
        /// </summary>
        public string Get(string key)
        {
            return GetList(key).FirstOrDefault();
        }

        /// <summary>
        /// Gets the value associated with a key. If key has multiple values, 
        /// only the first value is returned.  If the is not present in the 
        /// dictionary, the <paramref name="defaultValue"/> is returned instead.
        /// </summary>
        public string Get(string key, string defaultValue)
        {
            return Get(key) ?? defaultValue;
        }

        /// <summary>
        /// GetList is like <see cref="Get"/> except that it returns the 
        /// list of values associated with each key.  If the key is not present 
        /// in the dictionary, the given <paramref name="defaultValues"/> are
        /// returned.
        /// </summary>
        public IEnumerable<string> GetList(string key, params string[] defaultValues)
        {
            return _items.ContainsKey(key)
                       ? _items[key].Values.AsEnumerable()
                       : defaultValues;
        }

        /// <summary>
        /// Set is identical in function to the indexer set and is chainable.
        /// </summary>
        /// <returns>The <see cref="OMDict"/> instance to support method chaining.</returns>
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

        /// <summary>
        /// SetList sets a list of values for key and is chainable.  If values 
        /// already existed for the given key, they are replaced with the 
        /// given <paramref name="values"/>.
        /// </summary>
        /// <returns>The <see cref="OMDict"/> instance to support method chaining.</returns>
        public OMDict SetList(string key, params string[] values)
        {
            var pairs = values.Select(v => new KeyValuePair<string, string>(key, v));

            UpdateAll(pairs, true);
           
            return this;
        }

        /// <summary>
        /// If key is in the dictionary, return its value. If not, insert key with a 
        /// value of default and return default. Default defaults to null.
        /// </summary>
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

        /// <summary>
        /// SetDefaultList is like <see cref="SetDefault"/> except a 
        /// list of <paramref name="values"/> is adopted.
        /// </summary>
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

        /// <summary>
        /// Adds a value to the list of values for <paramref name="key"/>
        /// and is chainable.
        /// </summary>
        /// <returns>The <see cref="OMDict"/> instance to support method chaining.</returns>
        public OMDict Add(string key, string value = null)
        {
            var nextAvailableIndex = MaxIndex() + 1;

            if (_items.ContainsKey(key) == false)
                _items[key] = new SortedDictionary<int, string>();

            _items[key].Add(nextAvailableIndex, value);

            return this;
        }

        /// <summary>
        /// Adds the <paramref name="values"/> to the list of values for
        /// <paramref name="key"/> and is chainable.
        /// </summary>
        /// <returns>The <see cref="OMDict"/> instance to support method chaining.</returns>
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

        /// <summary>
        /// Pop pops the value of <paramref name="key"/> out of the dictionary
        /// returning the first value associated with key.  If multiple
        /// values are associated with key, only the first value is returned,
        /// but all values are removed.
        /// 
        /// If key has no values, <paramref name="defaultValue"/> is returned
        /// instead.
        /// </summary>
        public string Pop(string key, string defaultValue = null)
        {
            if (_items.ContainsKey(key) == false && defaultValue == null) 
                throw new InvalidOperationException("Must provide a defaultValue when key is not present in the dictionary.");

            var value = Get(key, defaultValue);

            Remove(key);

            return value;
        }

        /// <summary>
        /// PopList is like <see cref="Pop"/> but returns the list of values
        /// for <paramref name="key"/>.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="defaultValues"></param>
        /// <returns></returns>
        public IEnumerable<string> PopList(string key, params string[] defaultValues)
        {
            if (_items.ContainsKey(key) == false && defaultValues.Length == 0) 
                throw new InvalidOperationException("Must provide a defaultValue when key is not present in the dictionary.");

            var values = GetList(key, defaultValues)
                                .ToArray(); // force enumeration before removing the list of values.

            Remove(key);

            return values;
        }

        /// <summary>
        /// Pops a the last sequential value for a given key out of the dictionary.
        /// 
        /// If the optional parameter <paramref name="last"/> is set to false, this
        /// method pops the first sequential value for a given key.
        /// 
        /// If the given key has no values, <paramref name="defaultValue"/> is
        /// returned instead.
        /// 
        /// If <paramref name="defaultValue"/> is provided, and it exists for the 
        /// given key, then that is the value that is popped.
        /// </summary>
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
            if (defaultValue.IsNotNullOrWhiteSpace() && list.ContainsValue(defaultValue))
            {
                var itemToPop = list.First(x => x.Value == defaultValue);
                list.Remove(itemToPop.Key);
                return itemToPop.Value;
            }
            else if (list.Values.Any())
            {
                var lastItem = last ? list.Last() : list.First();
                list.Remove(lastItem.Key);
                return lastItem.Value;
            }

            return defaultValue;
        }

        /// <summary>
        /// Pops a key value pair composed of the last key sequentially and the first
        /// associated value.  All values for that key are removed.
        /// </summary>
        public KeyValuePair<string, string> PopItem()
        {
            var lastKey = AllKeys.Last();
            var lastKeysFirstValue = Pop(lastKey);
            return new KeyValuePair<string, string>(lastKey, lastKeysFirstValue);
        }

        /// <summary>
        /// Pops the last individual value, sequentially, from the dictionary.
        /// 
        /// If <paramref name="last"/> is <code>false</code>, pops the first
        /// individual value, sequentially.
        /// </summary>
        public KeyValuePair<string, string> PopItemFromAll(bool last = true)
        {
            var keyToPop = last ? Keys.Last() : Keys.First();

            var poppedValue = PopValue(keyToPop, last: last);

            return new KeyValuePair<string, string>(keyToPop, poppedValue);
        }

        /// <summary>
        /// Pops and returns a key:valuelist item comprised of
        /// a key and that key's list of values.
        /// 
        /// If <paramref name="last"/> is true, <see cref="PopListItem"/>
        /// returns the list of items for the last sequential key.
        /// 
        /// If <paramref name="last"/> is false, <see cref="PopListItem"/>
        /// returns the list of items for the first sequential key.
        /// </summary>
        public KeyValuePair<string,IEnumerable<string>> PopListItem(bool last = true)
        {
            var keyToPop = last ? Keys.Last() : Keys.First();

            var result = Tuple.Create(keyToPop, _items[keyToPop].Values
                                                                .ToArray()
                                                                .AsEnumerable())
                              .ToKeyValuePair();

            Remove(keyToPop);

            return result;
        }

        /// <summary>
        /// Removes all values for the given <paramref name="key"/>.
        /// </summary>
        public void Remove(string key)
        {
            if (_items.ContainsKey(key))
            {
                _items[key].Clear();    
                _items.Remove(key);
            }
        }

        /// <summary>
        /// Reinitialize an <see cref="OMDict"/> with the given values.
        /// </summary>
        /// <remarks>
        /// <see cref="Clear"/> then re-populate the existing instance.
        /// </remarks>
        /// <param name="args">key, value pairs as a collection of of string arguments.</param>
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

        /// <summary>
        /// updates the dictionary with items, one item per key.
        /// </summary>
        /// <param name="args">key, value pairs as a collection of of string arguments.</param>
        public void Update(params string[] args)
        {
            foreach (var pair in PairUp(args))
            {
                Set(pair.Key, pair.Value);
            }
        }

        /// <summary>
        /// Updates the dictionary with all the given items.
        /// Key order is preserved.  Existing keys are updated
        /// with values from mapping before any new items are added.
        /// </summary>
        /// <param name="args">key, value pairs as a collection of of string arguments.</param>
        public void UpdateAll(params string[] args)
        {
            var pairs = PairUp(args);

            UpdateAll(pairs);
        }

        /// <summary>
        /// Updates the dictionary with all the given items.
        /// Key order is preserved.  Existing keys are updated
        /// with values from mapping before any new items are added.
        /// 
        /// If <paramref name="removeExtra"/> is <code>true</code>,
        /// pre-existing values for the given keys that have not
        /// been updated by this command are removed.
        /// </summary>
        /// <param name="pairs">key, value pairs as a collection of of string arguments.</param>
        /// <param name="removeExtra">Should the existing values not updated be removed?</param>
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

        /// <summary>
        /// Helper method to take an array of strings ("key1", "value1", "key2", "value2", etc)
        /// and project it into an set of KeyValuePairs for processing.
        /// </summary>
        private IEnumerable<KeyValuePair<string, string>> PairUp(string[] items)
        {
            var result = new List<KeyValuePair<string, string>>();
            for (var i = 0; i+1 < items.Length; i += 2)
            {
                result.Add(new KeyValuePair<string, string>(items[i],items[i+1]));
            }

            return result;
        }

        /// <summary>
        /// Reverses the sequential order of the values in the dictionary.
        /// </summary>
        public OMDict Reverse()
        {
            var currentItems = AllItems().ToArray();

            Clear();

            UpdateAll(currentItems.Reverse());

            return this;
        }

        /// <summary>
        /// Removes all items from the dictionary.
        /// </summary>
        public void Clear()
        {
            var keys = _items.Keys.ToArray();

            keys.ForEach(Remove);
        }

        /// <summary>
        /// Creates a clone of this dictionary.
        /// </summary>
        /// <returns>
        /// A new <see cref="OMDict"/> instance with the same keys and
        /// values in the same sequential order as the original.
        /// </returns>
        public OMDict Copy()
        {
            var newOmd = new OMDict();

            AllItems().ForEach(pair => newOmd.Add(pair.Key, pair.Value));

            return newOmd;
        }
    }
}