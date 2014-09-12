using NUnit.Framework;

namespace FurlSharp.Tests.OMDictionary
{
    [TestFixture]
    public class Initialization_And_Updates
    {
        [Test]
        public void Can_be_initialized_with_a_collection_of_key_value_pairs()
        {
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
    }
}