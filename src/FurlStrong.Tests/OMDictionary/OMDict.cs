using System;
using System.Collections.Generic;
using System.Linq;

namespace Furlstrong.Tests.OMDictionary
{
    public class OMDict
    {
        private List<KeyValuePair<string, string>> _pairs;

        public OMDict(params string[] pairs)
        {
            Load(pairs);
        }

        public string this[string key]
        {
            get { return Get(key); }
            set { Set(key, value); }
        }

        public IEnumerable<KeyValuePair<string, string>> Items()
        {
            return _pairs.GroupBy(p => p.Key)
                         .Select(g => new KeyValuePair<string, string>(g.Key, g.First().Value));
        }

        public IEnumerable<KeyValuePair<string, string>> AllItems()
        {
            return _pairs;
        }

        public string Get(string key)
        {
            return GetList(key).FirstOrDefault();
        }

        public string Get(string key, string defaultValue)
        {
            return Get(key) ?? defaultValue;
        }

        public IEnumerable<string> GetList(string key, params string[] defaultValues)
        {
            var result = _pairs.Where(p => p.Key == key).Select(p => p.Value);
            return result.Any() ? result : defaultValues;
        }

        public OMDict Set(string key, string value)
        {
            var newPair = new KeyValuePair<string, string>(key, value);
            var q = _pairs.Where(p => p.Key == key).ToArray();
            if (q.Any())
            {
                var first = q.First();
                var index = _pairs.IndexOf(first);
                _pairs[index] = newPair;

                var r = q.Skip(1).ToArray();
                _pairs.RemoveAll(r.Contains);
            }   
            else
            {
                _pairs.Add(newPair);
            }

            return this;
        }

        public OMDict SetList(string key, params string[] values)
        {
            var replaced = new List<KeyValuePair<string, string>>();

            var pairs = values.Select(v => new KeyValuePair<string, string>(key, v))
                              .ToArray();
            foreach (var pair in pairs)
            {
                var q = _pairs.Where(p => p.Key == pair.Key && replaced.Contains(p) == false)
                              .ToArray();

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

            return this;
        }

        public void Remove(string key)
        {
            var toRemove = _pairs.Where(p => p.Key == key).ToArray();
            _pairs.RemoveAll(toRemove.Contains);
        }

        public void Load(params string[] args)
        {
            _pairs = PairUp(args).ToList();
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

        public void Update(params string[] args)
        {
            foreach (var pair in PairUp(args))
            {
                if (_pairs.Any(p => p.Key == pair.Key))
                {
                    var existingPair = _pairs.First(p => p.Key == pair.Key);
                    var index = _pairs.IndexOf(existingPair);
                    _pairs[index] = pair;
                }
                else
                {
                    _pairs.Add(pair);
                }
            }
        }

        public void UpdateAll(params string[] args)
        {
            var pairs = PairUp(args);
            var replaced = new List<KeyValuePair<string, string>>();

            foreach (var pair in pairs)
            {
                var q = _pairs.Where(p => p.Key == pair.Key && replaced.Contains(p) == false)
                              .ToArray();

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
        }
    }
}