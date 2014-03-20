using System.Collections.Generic;
using FurlStrong;
using NUnit.Framework;

namespace Furlstrong.Tests.OMDictionary
{
    /// <summary>
    /// Describes the basic behavior of an <see cref="OMDict"/>.
    /// </summary>
    [TestFixture]
    public class Overview
    {
        [Test]
        public void Can_store_multiple_values_per_key()
        {
            var omd = new OMDict();

            omd["1"] = "1";
            Assert.AreEqual("1", omd["1"]);

            omd.Add("1", "11");
            CollectionAssert.AreEqual(new [] {"1", "11"}, omd.GetList("1"));

            omd.AddList("1", "111", "1111");
            CollectionAssert.AreEqual(new [] {"1", "11", "111", "1111"}, omd.GetList("1"));

            Assert.AreEqual("[(1, 1), (1, 11), (1, 111), (1, 1111)]", omd.AllItems().FormatForApproval());
        }

        [Test]
        public void Retains_insertion_and_deletion_order()
        {
            var omd = new OMDict();

            omd["2"] = "2";
            omd["1"] = "1";
            Assert.AreEqual("[(2, 2), (1, 1)]", omd.Items().FormatForApproval());

            omd["2"] = "sup";
            Assert.AreEqual("[(2, sup), (1, 1)]", omd.Items().FormatForApproval());
        }

        [Test]
        public void Method_parity_with_dictionary_is_retained_omdict_can_be_a_drop_in_replacement()
        {
            var omd = new OMDict();
            var dict = new Dictionary<string, string>();

            Assert.Inconclusive("Not actually implementing IDictionary<string,string> yet.");
            Assert.IsInstanceOf<IDictionary<string, string>>(omd);
            
            dict.Add("1", "1");
            dict.Add("2", "2");
            dict["3"] = "3";

            omd.Add("1", "1");
            omd.Add("2", "2");
            omd["3"] = "3";

            Assert.AreEqual(dict["3"], omd["3"]);
            //CollectionAssert.AreEqual(dict, omd);
        }

        /// <summary>
        /// Many of <see cref="OMDict"/>'s methods contain the word <code>List</code> or 
        /// <code>All</code>.
        /// 
        /// <code>List</code> in a method name indicates that method interacts with a 
        /// list of values instead of a single value. <code>All</code> in a method name 
        /// indicates that method interacts with the ordered list of all items, 
        /// including multiple items with the same key.
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

            CollectionAssert.AreEqual(new[] { "1", "11" }, omd.GetList("1"));
        }
    }
}