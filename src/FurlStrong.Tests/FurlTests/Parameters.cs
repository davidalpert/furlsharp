using NUnit.Framework;

namespace Furlstrong.Tests.FurlTests
{
    [TestFixture]
    public class Parameters
    {
        [Test]
        public void Empty_strings_produce_empty_arguments()
        {
            var f = new Furlstrong.Furl("http://sprop.su");

            f.Query["param"] = "";

            Assert.AreEqual("http://sprop.su/?param=", f.Url);
        }

        [Test]
        public void Null_values_produce_empty_arguments_without_the_trailing_slash()
        {
            var f = new Furlstrong.Furl("http://sprop.su");

            f.Query["param"] = null;

            Assert.AreEqual("http://sprop.su/?param", f.Url);
        }

        [Test]
        public void Encode_can_be_used_to_encode_query_strings_with_custom_delimeters()
        {
            var f = new Furlstrong.Furl();

            f.Query = FurlQuery.Parse("space=jams&woofs=squeeze%20dog");

            Assert.AreEqual("space=jams&woofs=squeeze%20dog", f.Query.Encode());

            Assert.AreEqual("space=jams;woofs=squeeze%20dog", f.Query.Encode(';'));
        }

        [Test]
        public void Query_parameters_can_store_multiple_values_for_the_same_key()
        {
            var f = new Furlstrong.Furl("http://www.google.com/?space=jams&space=slams");
            Assert.AreEqual("jams", f.Query["space"]);
            CollectionAssert.AreEqual(new[] {"jams", "slams"}, f.Query.GetList("space"));

            f.Query.AddList("repeated", "1", "2", "3");
            Assert.AreEqual("space=jams&space=slams&repeated=1&repeated=2&repeated=3", f.Query.ToString());

            Assert.AreEqual("slams", f.Query.PopValue("space"));
            Assert.AreEqual("2", f.Query.PopValue("repeated", "2"));
            Assert.AreEqual("space=jams&repeated=1&repeated=3", f.Query.ToString());
        }

        [Test]
        public void Query_parameters_are_one_dimensional_list_values_are_interpreted_as_multiple_values()
        {
            var f = new Furlstrong.Furl();
            f.Query.AddList("repeated", "1", "2", "3");
            f.Query.AddList("space", "jams", "slams");
            Assert.AreEqual("repeated=1&repeated=2&repeated=3&space=jams&space=slams", f.Query.ToString());
        }

        [Test]
        public void To_produce_an_empty_query_argument_use_an_empty_string_as_a_value()
        {
            var f = new Furlstrong.Furl("http://sprop.su");

            f.Query["param"] = "";

            Assert.AreEqual("http://sprop.su/?param=", f.Url);
        }

        [Test]
        public void To_produce_an_empty_argument_without_a_trailing_equal_use_null_as_a_value()
        {
            var f = new Furlstrong.Furl("http://sprop.su");
            f.Query["param"] = null;
            Assert.AreEqual("http://sprop.su/?param", f.Url);
        }

        [Test]
        public void Encode_with_delimeter_can_be_used_to_encode_query_strings_with_delimeters()
        {
            var f = new Furlstrong.Furl
                {
                    Query = FurlQuery.Parse("space=jams&woofs=squeeze+dog")
                };

            Assert.AreEqual("space=jams&woofs=squeeze%20dog", f.Query.Encode());

            Assert.AreEqual("space=jams;woofs=squeeze%20dog", f.Query.Encode(';'));
        }
    }
}