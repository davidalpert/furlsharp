using System;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;

namespace Furlstrong.Tests.OMDictionary
{
    public static class ApprovalHelpers
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

            Assert.AreEqual("[(1, 1), (2, 2)]", omd.Items().FormatForApproval());

            Assert.AreEqual("[(1, 1), (2, 2), (1, 11)]", omd.AllItems().FormatForApproval());

            Assert.AreEqual("1", omd.Get("1"));

            CollectionAssert.AreEqual(new[] {"1", "11"}, omd.GetList("1"));
        }

        #region Initialization and updates

        [Test]
        public void Initialization_and_Updates()
        {
            //omdict objects can be initialized from a dictionary or a list of key:value items.
            var omd = new OMDict();

            Assert.AreEqual("[]", omd.AllItems().FormatForApproval());

            omd = new OMDict("1", "1",
                             "2", "2",
                             "3", "3");

            Assert.AreEqual("[(1, 1), (2, 2), (3, 3)]", omd.AllItems().FormatForApproval());

            omd = new OMDict("1", "1",
                             "2", "2",
                             "3", "3",
                             "1", "1");

            Assert.AreEqual("[(1, 1), (2, 2), (3, 3), (1, 1)]", omd.AllItems().FormatForApproval());
        }

        [Test]
        public void Load_can_be_used_to_reinitialize_an_omdic()
        {
            var omd = new OMDict();

            omd.Load("4", "4", "5", "5");
            Assert.AreEqual("[(4, 4), (5, 5)]", omd.AllItems().FormatForApproval());

            omd = new OMDict("1", "1",
                             "2", "2",
                             "3", "3");
            Assert.AreEqual("[(1, 1), (2, 2), (3, 3)]", omd.AllItems().FormatForApproval());

            omd.Load("6", "6",
                     "6", "6");
            Assert.AreEqual("[(6, 6), (6, 6)]", omd.AllItems().FormatForApproval());
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

            Assert.AreEqual("[(1, 11), (2, 22)]", omd.Items().FormatForApproval());

            Assert.AreEqual("[(1, 11), (2, 22)]", omd.AllItems().FormatForApproval());

            omd.UpdateAll("2", "replaced",
                          "1", "replaced",
                          "2", "added",
                          "1", "added");

            Assert.AreEqual("[(1, replaced), (2, replaced), (2, added), (1, added)]", omd.AllItems().FormatForApproval());
        }

        #endregion

        #region Indexing

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

            Assert.AreEqual("[(1, 1)]", omd.AllItems().FormatForApproval());
        }

        /// <summary>
        /// If *key* has multiple values, 
        /// all of them will be deleted.
        /// </summary>
        [Test]
        public void Remove_key_behaves_like_dictionary()
        {
            var omd = new OMDict("1", "1",
                                 "1", "1");

            omd.Remove("1");

            Assert.AreEqual("[]", omd.AllItems().FormatForApproval());
        }

        #endregion

        #region Getters, Setters, Adders

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

            CollectionAssert.AreEqual("[(1, 11), (2, 2)]", omd.AllItems().FormatForApproval());
        }

        [Test]
        public void SetList_sets_a_list_of_values_for_key_and_is_chainable()
        {
            var omd = new OMDict("1", "1",
                                 "2", "2");

            omd.SetList("1", "replaced", "appended");

            Assert.AreEqual("[(1, replaced), (2, 2), (1, appended)]", omd.AllItems().FormatForApproval());

            omd.SetList("1", "onlyme");

            Assert.AreEqual("[(1, onlyme), (2, 2)]", omd.AllItems().FormatForApproval());
        }

        /// <summary>
        /// If key is in the dictionary, return its value. If not, insert key with a 
        /// value of default and return default. Default defaults to null.
        /// </summary>
        [Test]
        public void SetDefault()
        {
            var omd = new OMDict("1", "1");

            Assert.AreEqual("1", omd.SetDefault("1"));

            omd.SetDefault("2", null);

            Assert.AreEqual("[(1, 1), (2, )]", omd.AllItems().FormatForApproval());
        }

        [Test]
        public void SetDefaultList_is_like_SetDefault_except_a_list_of_values_is_adopted()
        {
            var omd = new OMDict("1", "1");

            CollectionAssert.AreEqual(new[] {"1"}, omd.SetDefaultList("1"));

            CollectionAssert.AreEqual(new[] {"2", "22"}, omd.SetDefaultList("2", "2", "22"));

            Assert.AreEqual("[(1, 1), (2, 2), (2, 22)]", omd.AllItems().FormatForApproval());

            CollectionAssert.AreEqual(new string[] {null}, omd.SetDefaultList("3"));

            Assert.AreEqual(null, omd["3"]);
        }

        [Test]
        public void Add_adds_value_to_the_list_of_values_for_key_and_is_chainable()
        {
            var omd = new OMDict();
            omd.Add("1", "1");

            Assert.AreEqual("[(1, 1)]", omd.AllItems().FormatForApproval());

            omd.Add("1", "11").Add("2", "2");

            Assert.AreEqual("[(1, 1), (1, 11), (2, 2)]", omd.AllItems().FormatForApproval());
        }

        [Test]
        public void AddList_adds_the_values_in_list_to_key_and_is_chainable()
        {
            var omd = new OMDict("1", "1");
            omd.AddList("1", "11", "111");

            Assert.AreEqual("[(1, 1), (1, 11), (1, 111)]", 
                            omd.AllItems().FormatForApproval());

            omd.AddList("2", "2").AddList("3", "3", "33");

            Assert.AreEqual("[(1, 1), (1, 11), (1, 111), (2, 2), (3, 3), (3, 33)]", 
                            omd.AllItems().FormatForApproval());
        }

        #endregion

        #region Groups and Group Iteration

        [Test]
        public void Items_accepts_an_optional_key_to_filter_the_items()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            Assert.AreEqual("[(1, 1), (2, 2), (3, 3)]", omd.Items().FormatForApproval());

            Assert.AreEqual("[(1, 1), (1, 11), (1, 111)]", omd.Items("1").FormatForApproval());
        }

        [Test]
        public void Keys_returns_an_iterator_over_keys()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            CollectionAssert.AreEqual(new[] {"1", "2", "3"}, omd.Keys);
        }

        [Test]
        public void Values_iterates_over_values_with_an_optional_key_focus()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            CollectionAssert.AreEqual(new[] {"1", "2", "3"}, omd.Values());

            CollectionAssert.AreEqual(new[] {"1", "11", "111"}, omd.Values("1"));
        }

        [Test]
        public void Lists_returns_an_iterator_over_valuelist_items()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            CollectionAssert.AreEqual(new[]
                {
                    new [] {"1", "11", "111"},
                    new [] {"2"},
                    new [] {"3"}
                }, 
                omd.Lists());
        }

        [Test]
        public void AllKeys_returns_a_list_of_the_keys_of_every_item_in_the_dictionary()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            CollectionAssert.AreEqual(new[] {"1", "1", "1", "2", "3"},
                                      omd.AllKeys);
        }

        [Test]
        public void AllValues_returns_a_list_of_the_values_of_every_item_in_the_dictionary()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            CollectionAssert.AreEqual(new[] {"1", "11", "111", "2", "3"},
                                      omd.AllValues);
        }

        #endregion

        #region Pops

        [Test]
        public void Pop_pops_a_set_of_keyvalues_out_of_the_dictionary_returning_only_the_first_value()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            Assert.AreEqual("1", omd.Pop("1"));

            Assert.AreEqual("[(2, 2), (3, 3)]", omd.AllItems().FormatForApproval());
        }

        [Test]
        public void Pop_can_provide_a_default_value_in_case_key_is_not_in_the_dictionary()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            Assert.AreEqual("sup", omd.Pop("404", "sup"));
        }

        [Test]
        public void Pop_raises_an_InvalidOperationException_if_key_is_not_in_the_dictionary_and_default_is_not_provided()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            Assert.Throws<InvalidOperationException>(() => omd.Pop("404"));
        }

        [Test]
        public void PopList_pops_a_set_of_keyvalues_out_of_the_dictionary_returning_all_the_values()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            CollectionAssert.AreEqual(new [] {"1", "11", "111"}, omd.PopList("1"));

            Assert.AreEqual("[(2, 2), (3, 3)]", omd.AllItems().FormatForApproval());

            CollectionAssert.AreEqual(new [] {"2"}, omd.PopList("2"));

            Assert.AreEqual("[(3, 3)]", omd.AllItems().FormatForApproval());
        }

        [Test]
        public void PopList_can_provide_a_default_value_in_case_key_is_not_in_the_dictionary()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            Assert.AreEqual(new [] {"sup"}, omd.PopList("nonexistant key", "sup"));
        }

        [Test]
        public void PopList_raises_an_InvalidOperationException_if_key_is_not_in_the_dictionary_and_default_is_not_provided()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            Assert.Throws<InvalidOperationException>(() => omd.PopList("nonexistant key"));
        }

        [Test]
        public void PopValue()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3",
                                 "2", "22");

            Assert.AreEqual("111", omd.PopValue("1"));

            Assert.AreEqual("[(1, 1), (1, 11), (2, 2), (3, 3), (2, 22)]",
                            omd.AllItems().FormatForApproval());

            Assert.AreEqual("1", omd.PopValue("1", last:false));

            Assert.AreEqual("22", omd.PopValue("2", "2"));

            Assert.AreEqual("[(1, 11), (2, 2), (3, 3)]",
                            omd.AllItems().FormatForApproval());

            Assert.AreEqual("11", omd.PopValue("1", "11"));

            Assert.AreEqual("[(2, 2), (3, 3)]",
                            omd.AllItems().FormatForApproval());

            Assert.AreEqual("sup", omd.PopValue("not a key", "sup"));
        }

        [Test]
        public void Indexer_with_int_provides_key_value_pair_by_zero_based_position()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var x = omd[-1];
                });
            Assert.AreEqual("[1, 1]", omd[0].ToString());
            Assert.AreEqual("[1, 11]", omd[1].ToString());
            Assert.AreEqual("[1, 111]", omd[2].ToString());
            Assert.AreEqual("[2, 2]", omd[3].ToString());
            Assert.AreEqual("[3, 3]", omd[4].ToString());
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                {
                    var x = omd[5];
                });
        }

        [Test]
        public void PopItem_pops_and_returns_a_key_value_item()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            Assert.AreEqual("[3, 3]", omd.PopItem().ToString());
            Assert.AreEqual("[2, 2]", omd.PopItem().ToString());
            Assert.AreEqual("[1, 1]", omd.PopItem().ToString());
            Assert.AreEqual("[]", omd.AllItems().FormatForApproval());

            omd = new OMDict("1", "1",
                             "1", "11",
                             "1", "111",
                             "2", "2",
                             "3", "3");

            /*  If __fromall__ is False, items()[0] is popped if __last__ is False or
                items()[-1] is popped if __last__ is True. All remaining items with the same key
                are removed.

                If __fromall__ is True, allitems()[0] is popped if __last__ is False or
                allitems()[-1] is popped if __last__ is True. No other remaining items are
                removed, even if they have the same key.
             */

            Assert.AreEqual("[1, 1]", omd.PopItemFromAll(last:false).ToString());
            Assert.AreEqual("[1, 11]", omd.PopItemFromAll(last:false).ToString());
            Assert.AreEqual("[3, 3]", omd.PopItemFromAll(last:true).ToString());
            Assert.AreEqual("[1, 111]", omd.PopItemFromAll(last:false).ToString());
        }

        /// <summary>
        /// __poplistitem([key], last=True)__ pops and returns a key:valuelist item
        /// comprised of a key and that key's list of values. If __last__ is False, a
        /// key:valuelist item comprised of keys()[0] and its list of values is popped and
        /// returned. If __last__ is True, a key:valuelist item comprised of keys()[-1] and
        /// its list of values is popped and returned. KeyError is raised if the dictionary
        /// is empty or if __key__ is provided and not in the dictionary.
        /// </summary>
        [Test]
        public void PopListItem(string key = null, bool last = true)
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            var pair = omd.PopListItem(last: true);
            Assert.AreEqual("3", pair.Key);
            CollectionAssert.AreEqual(new [] {"3"}, pair.Value);

            pair = omd.PopListItem(last: false);
            Assert.AreEqual("1", pair.Key);
            CollectionAssert.AreEqual(new [] {"1", "11", "111"}, pair.Value);
        }

        #endregion

        #region Miscellaneous

        [Test]
        public void Copy_returns_a_shallow_copy()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "2", "2",
                                 "3", "3");

            var copy = omd.Copy();

            Assert.AreNotSame(omd, copy);

            CollectionAssert.AreEqual(omd.AllItems(), copy.AllItems());
        }

        [Test]
        public void Clear_clears_all_items()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "2", "2",
                                 "3", "3");

            omd.Clear();

            Assert.AreEqual("[]", omd.AllItems().FormatForApproval());
        }

        [Test]
        public void Length_returns_the_number_of_keys_in_the_dictionary()
        {
            var omd = new OMDict("1", "1",
                                 "2", "2",
                                 "1", "11");

            Assert.AreEqual(2, omd.Length);
        }

        [Test]
        public void Size_returns_the_number_of_all_items_in_the_dictionary()
        {
            var omd = new OMDict("1", "1",
                                 "2", "2",
                                 "1", "11",
                                 "1", "111");

            Assert.AreEqual(4, omd.Size);
        }

        [Test]
        public void Reverse_reverses_the_items_and_is_chainable()
        {
            var omd = new OMDict("1", "1",
                                 "2", "2",
                                 "3", "3");

            Assert.AreEqual("[(1, 1), (2, 2), (3, 3)]", omd.AllItems().FormatForApproval());

            omd.Reverse();

            Assert.AreEqual("[(3, 3), (2, 2), (1, 1)]", omd.AllItems().FormatForApproval());

            omd.Reverse().Reverse();

            Assert.AreEqual("[(3, 3), (2, 2), (1, 1)]", omd.AllItems().FormatForApproval());
        }

        #endregion
    }
}