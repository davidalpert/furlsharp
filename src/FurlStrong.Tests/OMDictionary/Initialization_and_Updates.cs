﻿using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Furlstrong.Tests.OMDictionary
{
    public static class ApprovalHelpers
    {
        public static string FormatForApprovals<TKey, TValue>(this IEnumerable<Tuple<TKey, TValue>> items)
        {
            return string.Format("[{0}]", string.Join(", ", items.Select(p => p.ToString())));
        }

        public static string FormatForApprovals<TKey, TValue>(this IEnumerable<KeyValuePair<TKey, TValue>> items)
        {
            return string.Format("[{0}]", string.Join(", ", items.Select(p => string.Format("({0}, {1})", p.Key, p.Value))));
        }
    }

    [TestFixture]
    public class OmDictionaryTests
    {
        /// <summary>
        /// Many of omdict's methods contain the word __list__ or __all__. __list__ in a
        /// method name indicates that method interacts with a list of values instead of a
        /// single value. __all__ in a method name indicates that method interacts with the
        /// ordered list of all items, including multiple items with the same key.
        /// 
        /// Here's an example illustrating __getlist(key, default=[])__, a __list__ method,
        /// and __allitems()__, an __all__ method.
        /// 
        /// So __list__ denotes a list of values, and __all__ denotes all items.
        /// 
        /// Simple.
        /// 
        /// ### Method parity with dict
        ///
        /// All [dict](http://docs.python.org/library/stdtypes.html#dict) methods behave
        /// identically on omdict objects.
        /// </summary>
        [Test]
        public void Nomenclature()
        {
            var omd = new OMDict("1", "1",
                                 "2", "2",
                                 "1", "11");

            Assert.AreEqual("[(1, 1), (2, 2)]", omd.Items().FormatForApprovals());

            Assert.AreEqual("[(1, 1), (2, 2), (1, 11)]", omd.AllItems().FormatForApprovals());

            Assert.AreEqual("1", omd.Get("1"));

            CollectionAssert.AreEqual(new[] {"1", "11"}, omd.GetList("1"));
        }

        [Test]
        public void Initialization_and_Updates()
        {
            //omdict objects can be initialized from a dictionary or a list of key:value items.
            var omd = new OMDict();

            Assert.AreEqual("[]", omd.AllItems().FormatForApprovals());

            omd = new OMDict("1", "1",
                             "2", "2",
                             "3", "3");

            Assert.AreEqual("[(1, 1), (2, 2), (3, 3)]", omd.AllItems().FormatForApprovals());

            omd = new OMDict("1", "1",
                             "2", "2",
                             "3", "3",
                             "1", "1");

            Assert.AreEqual("[(1, 1), (2, 2), (3, 3), (1, 1)]", omd.AllItems().FormatForApprovals());
        }

        [Test]
        public void Load_can_be_used_to_reinitialize_an_omdic()
        {
            var omd = new OMDict();

            omd.Load("4", "4", "5", "5");
            Assert.AreEqual("[(4, 4), (5, 5)]", omd.AllItems().FormatForApprovals());

            omd = new OMDict("1", "1",
                             "2", "2",
                             "3", "3");
            Assert.AreEqual("[(1, 1), (2, 2), (3, 3)]", omd.AllItems().FormatForApprovals());

            omd.Load("6", "6",
                     "6", "6");
            Assert.AreEqual("[(6, 6), (6, 6)]", omd.AllItems().FormatForApprovals());
        }

        /// <summary>
        /// Update() updates the dictionary with items, one item per key.
        /// 
        /// UpdateAll() upates the dictionary with all items from the params,
        /// preserving key order, then adds remaining keys to the end.
        /// </summary>
        [Test]
        public void Update_updates_the_dictionary_values_in_sequence()
        {
            var omd = new OMDict();
            omd.Update("1", "1",
                       "2", "2",
                       "1", "11",
                       "2", "22");

            Assert.AreEqual("[(1, 11), (2, 22)]", omd.Items().FormatForApprovals());

            Assert.AreEqual("[(1, 11), (2, 22)]", omd.AllItems().FormatForApprovals());

            omd.UpdateAll("2", "replaced",
                          "1", "replaced",
                          "2", "added",
                          "1", "added");

            Assert.AreEqual("[(1, replaced), (2, replaced), (2, added), (1, added)]", omd.AllItems().FormatForApprovals());
        }

        /// <summary>
        /// If *key* has multiple values, 
        /// only the first value is returned.
        /// </summary>
        [Test]
        public void Indexer_get_behaves_like_dictionary()
        {
            var omd = new OMDict("1", "1",
                                 "1", "not me");

            Assert.AreEqual("1", omd["1"]);
        }

        /// <summary>
        /// If *key* has multiple values, 
        /// they will all be deleted and 
        /// replaced by *value*.
        /// </summary>
        [Test]
        public void Indexer_set_behaves_like_dictionary()
        {
            var omd = new OMDict("1", "deleted",
                                 "1", "deleted");

            omd["1"] = "1";

            Assert.AreEqual("1", omd["1"]);

            Assert.AreEqual("[(1, 1)]", omd.AllItems().FormatForApprovals());
        }

        /// <summary>
        /// If *key* has multiple values, 
        /// all of them will be deleted.
        /// </summary>
        [Test]
        public void Indexer_remove_key_behaves_like_dictionary()
        {
            var omd = new OMDict("1", "1",
                                 "1", "1");

            omd.Remove("1");

            Assert.AreEqual("[]", omd.AllItems().FormatForApprovals());
        }

        [Test]
        public void Get_gets_the_first_value_and_can_provide_a_default()
        {
            var omd = new OMDict("1", "1",
                                 "1", "2");

            Assert.AreEqual("1", omd.Get("1"));

            Assert.AreEqual("sup", omd.Get("404", "sup"));
        }

        [Test]
        public void GetList_is_like_Get_except_it_returns_the_list_of_values_associated_with_key()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "2", "2");

            CollectionAssert.AreEqual(new [] {"1", "11"}, omd.GetList("1"));

            CollectionAssert.AreEqual(new [] {"sup"}, omd.GetList("404", "sup"));
        }

        [Test]
        public void Set_is_identical_in_function_to_the_indexer_Set_and_is_chainable()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111");

            omd.Set("1", "1");

            CollectionAssert.AreEqual(new [] {"1"}, omd.GetList("1"));

            omd.Set("1", "11").Set("2","2");

            CollectionAssert.AreEqual("[(1, 11), (2, 2)]", omd.AllItems().FormatForApprovals());
        }

        [Test]
        public void SetList_sets_a_list_of_values_for_key_and_is_chainable()
        {
            var omd = new OMDict("1", "1",
                                 "2", "2");

            omd.SetList("1", "replaced", "appended");

            Assert.AreEqual("[(1, replaced), (2, 2), (1, appended)]", omd.AllItems().FormatForApprovals());

            omd.SetList("1", "onlyme");

            Assert.AreEqual("[(1, onlyme), (2, 2)]", omd.AllItems().FormatForApprovals());
        }
    }

    /*

### Getters, Setters, and Adders

__setlist(key, values=[])__ sets __key__'s list of values to __values__. Returns
the omdict object for method chaining.

```pycon
>>> omd = omdict([(1,1), (2,2)])
>>> omd.setlist(1, ['replaced', 'appended'])
>>> omd.allitems()
[(1, 'replaced'), (2, 2), (1, 'appended')]
>>> omd.setlist(1, ['onlyme'])
>>> omd.allitems()
[(1, 'onlyme'), (2, 2)]
```

__setdefault(key, default=None)__ behaves identically to [dict.setdefault(key,
default=None)](http://docs.python.org/library/stdtypes.html#dict.setdefault).

```pycon
>>> omd = omdict([(1,1)])
>>> omd.setdefault(1)
1
>>> omd.setdefault(2, None)
>>> omd.allitems()
[(1, 1), (2, None)]
```

__setdefaultlist(key, defaultlist=[None])__ is like setdefault(key, default=None)
except a list of values for __key__ is adopted. If __defaultlist__ isn't
provided, __key__'s value becomes None.

```pycon
>>> omd = omdict([(1,1)])
>>> omd.setdefaultlist(1)
[1]
>>> omd.setdefaultlist(2, [2, 22])
[2, 22]
>>> omd.allitems()
[(1, 1), (2, 2), (2, 22)]
>>> omd.setdefaultlist(3)
[None]
>>> print omd[3]
None
```

__add(key, value=None)__ adds __value__ to the list of values for __key__.
Returns the omdict object for method chaining.

```pycon
>>> omd = omdict()
>>> omd.add(1, 1)
>>> omd.allitems()
[(1, 1)]
>>> omd.add(1, 11).add(2, 2)
>>> omd.allitems()
[(1, 1), (1, 11), (2, 2)]
```

__addlist(key, valuelist=[])__ adds the values in __valuelist__ to the list of
values for __key__. Returns the omdict object for method chaining.

```pycon
>>> omd = omdict([(1,1)])
>>> omd.addlist(1, [11, 111])
>>> omd.allitems()
[(1, 1), (1, 11), (1, 111)]
>>> omd.addlist(2, [2]).addlist(3, [3, 33])
>>> omd.allitems()
[(1, 1), (1, 11), (1, 111), (2, 2), (3, 3), (3, 33)]
```


### Groups and Group Iteration

__items([key])__ behaves identically to
[dict.items()](http://docs.python.org/library/stdtypes.html#dict.items) except
an optional __key__ parameter has been added. If __key__ is provided, only items
with key __key__ are returned. __iteritems([key])__ returns an iterator over
items(key). KeyError is raised if __key__ is provided and not in the dictionary.

```pycon
>>> omd = omdict([(1,1), (1,11), (1,111), (2,2), (3,3)])
>>> omd.items()
[(1, 1), (2, 2), (3, 3)]
>>> omd.items(1)
[(1, 1), (1, 11), (1, 111)]
```

__keys()__ behaves identically to
[dict.keys()](http://docs.python.org/library/stdtypes.html#dict.keys).
__iterkeys()__ returns an iterator over keys().

__values([key])__ behaves identically to
[dict.values()](http://docs.python.org/library/stdtypes.html#dict.values) except
an optional __key__ parameter has been added. If __key__ is provided, only the
values for __key__ are returned. __itervalues([key])__ returns an iterator over
values(key). KeyError is raised if __key__ is provided and not in the
dictionary.

```pycon
>>> omd = omdict([(1,1), (1,11), (1,111), (2,2), (3,3)])
>>> omd.values()
[1, 2, 3]
>>> omd.values(1)
[1, 11, 111]
```

__lists()__ returns a list comprised of the lists of values associated with each
dictionary key. __iterlists()__ returns and iterator over lists().

```pycon
>>> omd = omdict([(1,1), (1,11), (1,111), (2,2), (3,3)])
>>> omd.lists()
[[1, 11, 111], [2], [3]]
```

__listitems()__ returns a list of key:valuelist items. __iterlistitems()__
returns an iterator over listitems().

```pycon
>>> omd = omdict([(1,1), (1,11), (1,111), (2,2), (3,3), (2,22)])
>>> omd.listitems()
[(1, [1, 11, 111]), (2, [2, 22]), (3, [3])]
```

__allitems([key])__ returns a list of every item in the dictionary, including
multiple items with the same key. If __key__ is provided and in the dictionary,
only items with key __key__ are returned . KeyError is raised if __key__ is
provided and not in the dictionary. __iterallitems([key])__ returns an iterator
over allitems(key).

```pycon
>>> omd = omdict([(1,1), (1,11), (1,111), (2,2), (3,3)])
>>> omd.allitems()
[(1, 1), (1, 11), (1, 111), (2, 2), (3, 3)]
```

__allkeys()__ returns a list of the keys of every item in the dictionary.
__iterallkeys()__ returns an iterator over allkeys().

```pycon
>>> omd = omdict([(1,1), (1,11), (1,111), (2,2), (3,3)])
>>> omd.allkeys()
[1, 1, 1, 2, 3]
```

__allvalues()__ returns a list of the values of every item in the dictionary.
__iterallvalues()__ returns an iterator over allvalues().

```pycon
>>> omd = omdict([(1,1), (1,11), (1,111), (2,2), (3,3)])
>>> omd.allvalues()
[1, 11, 111, 2, 3]
```


### Pops

__pop(key[, default])__ behaves identically to [dict.pop(key\[,
default\])](http://docs.python.org/library/stdtypes.html#dict.pop). If __key__
has multiple values, the first value is returned but all items with key __key__
are popped. KeyError is raised if __default__ isn't provided and __key__ isn't
in the dictionary.

```pycon
>>> omd = omdict([(1,1), (2,2), (1,11)])
>>> omd.pop(1)
1
>>> omd.allitems()
[(2, 2)]
```

__poplist(key[, default])__ is like pop(key[, default]) except it returns the
list of values for __key__. KeyError is raised if __default__ isn't provided and
__key__ isn't in the dictionary.

```pycon
>>> omd = omdict([(1,1), (1,11), (1,111), (2,2), (3,3)])
>>> omd.poplist(1)
[1, 11, 111]
>>> omd.allitems()
[(2, 2), (3, 3)]
>>> omd.poplist(2)
[2]
>>> omd.allitems()
[(3, 3)]
>>> omd.poplist('nonexistent key', 'sup')
'sup'
```

__popvalue(key[, value, default], last=True)__ pops a value for __key__.

If __value__ is not provided, the first or last value for __key__ is popped and
returned.

If __value__ is provided, the first or last (__key__,__value__) item is popped
and __value__ is returned.

If __key__ no longer has any values after a popvalue() call, __key__ is removed
from the dictionary. __default__ is returned if provided and __key__ isn't in
the dictionary. KeyError is raised if __default__ isn't provided and __key__
isn't in the dictionary. ValueError is raised if __value__ is provided but isn't a
value for __key__.

```pycon
>>> omd = omdict([(1,1), (1,11), (1,111), (2,2), (3,3), (2,22)])
>>> omd.popvalue(1)
111
>>> omd.allitems()
[(1, 1), (1, 11), (2, 2), (3, 3), (2, 22)]
>>> omd.popvalue(1, last=False)
1
>>> omd.allitems()
[(1, 11), (2, 2), (3, 3), (2, 22)]
>>> omd.popvalue(2, 2)
2
>>> omd.allitems()
[(1, 11), (3, 3), (2, 22)]
>>> omd.popvalue(1, 11)
11
>>> omd.allitems()
[(3, 3), (2, 22)]
>>> omd.popvalue('not a key', default='sup')
'sup'
```

__popitem(fromall=False, last=True)__ pops and returns a key:value item.

If __fromall__ is False, items()[0] is popped if __last__ is False or
items()[-1] is popped if __last__ is True. All remaining items with the same key
are removed.

If __fromall__ is True, allitems()[0] is popped if __last__ is False or
allitems()[-1] is popped if __last__ is True. No other remaining items are
removed, even if they have the same key.

```pycon
>>> omd = omdict([(1,1), (1,11), (1,111), (2,2), (3,3)])
>>> omd.popitem()
(3, 3)
>>> omd.popitem(fromall=False, last=False)
(1, 1)
>>> omd.popitem(fromall=False, last=False)
(2, 2)
>>> omd.allitems()
[]

>>> omd = omdict([(1,1), (1,11), (1,111), (2,2), (3,3)])
>>> omd.popitem(fromall=True, last=False)
(1, 1)
>>> omd.popitem(fromall=True, last=False)
(1, 11)
>>> omd.popitem(fromall=True, last=True)
(3, 3)
>>> omd.popitem(fromall=True, last=False)
(1, 111)
```

__poplistitem([key], last=True)__ pops and returns a key:valuelist item
comprised of a key and that key's list of values. If __last__ is False, a
key:valuelist item comprised of keys()[0] and its list of values is popped and
returned. If __last__ is True, a key:valuelist item comprised of keys()[-1] and
its list of values is popped and returned. KeyError is raised if the dictionary
is empty or if __key__ is provided and not in the dictionary.

```pycon
>>> omd = omdict([(1,1), (1,11), (1,111), (2,2), (3,3)])
>>> omd.poplistitem(last=True)
(3, [3])
>>> omd.poplistitem(last=False)
(1, [1, 11, 111])
```


### Miscellaneous

__copy()__ returns a shallow copy of the dictionary.

```pycon
>>> omd = omdict([(1,1), (1,11), (2,2), (3,3)])
>>> copy = omd.copy()
>>> omd == copy
True
>>> isinstance(copy, omdict)
True
```

__clear()__ clears all items.

```pycon
>>> omd = omdict([(1,1), (1,11), (2,2), (3,3)])
>>> omd.clear()
>>> omd.allitems()
[]
```

__len(omd)__ returns the number of keys in the dictionary, identical to
[len(dict)](http://docs.python.org/library/stdtypes.html#dict).

```pycon
>>> omd = omdict([(1, 1), (2, 2), (1, 11)])
>>> len(omd)
2
```

__size()__ returns the total number of items in the dictionary.

```pycon
>>> omd = omdict([(1, 1), (1, 11), (2, 2), (1, 111)])
>>> omd.size()
4
```

__reverse()__ reverses the order of all items in the dictionary and returns the
omdict object for method chaining.

```pycon
>>> omd = omdict([(1, 1), (2, 2), (3, 3)])
>>> omd.allitems()
[(1, 1), (2, 2), (3, 3)]
>>> omd.reverse()
>>> omd.allitems()
[(3, 3), (2, 2), (1, 1)]
```

__fromkeys(keys[, value])__ behaves identically to [dict.fromkeys(key\[,
value\])](http://docs.python.org/library/stdtypes.html#dict.fromkeys).

__has_key(key)__ behaves identically to
[dict.has_key(key)](http://docs.python.org/library/stdtypes.html#dict.has_key),
but use `key in omd` instead of `omd.has_key(key)` where possible.
     */
}