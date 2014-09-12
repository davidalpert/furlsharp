using NUnit.Framework;

namespace FurlSharp.Tests.OMDictionary
{
    [TestFixture]
    public class Miscellaneous
    {
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
    }
}