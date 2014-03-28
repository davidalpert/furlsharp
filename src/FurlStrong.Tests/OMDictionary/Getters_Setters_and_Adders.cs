using NUnit.Framework;

namespace FurlStrong.Tests.OMDictionary
{
    [TestFixture]
    public class Getters_Setters_and_Adders
    {
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

            CollectionAssert.AreEqual(new[] {"1", "11"}, omd.GetList("1"));

            CollectionAssert.AreEqual(new[] {"sup"}, omd.GetList("404", "sup"));
        }

        [Test]
        public void Set_is_identical_in_function_to_the_indexer_Set_and_is_chainable()
        {
            var omd = new OMDict("1", "1",
                                 "1", "11",
                                 "1", "111");

            omd.Set("1", "1");

            CollectionAssert.AreEqual(new[] {"1"}, omd.GetList("1"));

            omd.Set("1", "11").Set("2", "2");

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
    }
}