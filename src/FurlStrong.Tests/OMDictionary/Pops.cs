using System;
using FurlStrong;
using NUnit.Framework;

namespace Furlstrong.Tests.OMDictionary
{
    [TestFixture]
    public class Pops
    {
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

            CollectionAssert.AreEqual(new[] {"1", "11", "111"}, omd.PopList("1"));

            Assert.AreEqual("[(2, 2), (3, 3)]", omd.AllItems().FormatForApproval());

            CollectionAssert.AreEqual(new[] {"2"}, omd.PopList("2"));

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

            Assert.AreEqual(new[] {"sup"}, omd.PopList("nonexistant key", "sup"));
        }

        [Test]
        public void
            PopList_raises_an_InvalidOperationException_if_key_is_not_in_the_dictionary_and_default_is_not_provided()
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

            Assert.AreEqual("1", omd.PopValue("1", last: false));

            Assert.AreEqual("2", omd.PopValue("2", "2"));

            Assert.AreEqual("[(1, 11), (3, 3), (2, 22)]",
                            omd.AllItems().FormatForApproval());

            Assert.AreEqual("11", omd.PopValue("1", "11"));

            Assert.AreEqual("[(3, 3), (2, 22)]",
                            omd.AllItems().FormatForApproval());

            Assert.AreEqual("sup", omd.PopValue("not a key", "sup"));
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

            Assert.AreEqual("[1, 1]", omd.PopItemFromAll(last: false).ToString());
            Assert.AreEqual("[1, 11]", omd.PopItemFromAll(last: false).ToString());
            Assert.AreEqual("[3, 3]", omd.PopItemFromAll(last: true).ToString());
            Assert.AreEqual("[1, 111]", omd.PopItemFromAll(last: false).ToString());
        }

        [Test]
        public void PopListItem()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111",
                                 "2", "2",
                                 "3", "3");

            var pair = omd.PopListItem(last: true);
            Assert.AreEqual("3", pair.Key);
            CollectionAssert.AreEqual(new[] {"3"}, pair.Value);
            Assert.AreEqual("[(1, 1), (1, 11), (1, 111), (2, 2)]",
                            omd.AllItems().FormatForApproval());

            pair = omd.PopListItem(last: false);
            Assert.AreEqual("1", pair.Key);
            CollectionAssert.AreEqual(new[] {"1", "11", "111"}, pair.Value);
            Assert.AreEqual("[(2, 2)]",
                            omd.AllItems().FormatForApproval());

        }
    }
}