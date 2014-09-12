using NUnit.Framework;

namespace FurlSharp.Tests.OMDictionary
{
    [TestFixture]
    public class Groups_And_Group_Iteration
    {
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
                    new[] {"1", "11", "111"},
                    new[] {"2"},
                    new[] {"3"}
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
    }
}