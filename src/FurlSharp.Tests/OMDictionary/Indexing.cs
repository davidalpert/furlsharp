using System;
using NUnit.Framework;

namespace FurlSharp.Tests.OMDictionary
{
    [TestFixture]
    public class Indexing
    {
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

        [Test]
        public void Indexer_with_int_gets_a_key_value_pair_by_zero_based_position()
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
    }
}