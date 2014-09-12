using NUnit.Framework;

namespace FurlSharp.Tests.FurlTests
{
    [TestFixture]
    public class Fragments
    {
        [Test]
        public void URL_fragments_in_Furl_are_Fragment_objects_that_have_a_Path_and_Query()
        {
            var f = new Furl("http://www.google.com/#/fragment/path?with=params");

            Assert.AreEqual("/fragment/path?with=params", f.Fragment.ToString());

            Assert.AreEqual("/fragment/path", f.Fragment.Path.ToString());

            Assert.AreEqual("with=params", f.Fragment.Query.ToString());

            Assert.AreEqual(true, f.Fragment.HasSeparator);
        }

        [Test]
        public void Manipulationg_of_fragments_is_done_through_the_Fragments_Path_and_Query_instances()
        {
            var f = new Furl("http://www.google.com/#/fragment/path?with=params");
            Assert.AreEqual("/fragment/path?with=params", f.Fragment.ToString());

            f.Fragment.Path.Append("file.exe");
            Assert.AreEqual("/fragment/path/file.exe?with=params", f.Fragment.ToString());

            f = new Furl("http://www.google.com/#/fragment/path?with=params");
            Assert.AreEqual("/fragment/path?with=params", f.Fragment.ToString());

            f.Fragment.Query["new"] = "yep";
            Assert.AreEqual("/fragment/path?with=params&new=yep", f.Fragment.ToString());
        }

        [Test]
        public void Creating_hash_bang_fragments_illustrates_the_use_of_HasSeparator()
        {
            var f = new Furl("http://www.google.com/");
            f.Fragment.Path = "!";
            f.Fragment.Query.UpdateAll("a", "dict", "of", "args");
            Assert.IsTrue(f.Fragment.HasSeparator, f.Fragment + " should have a separator.");
            Assert.AreEqual("!?a=dict&of=args", f.Fragment.ToString());

            f.Fragment.HasSeparator = false;
            Assert.AreEqual("!a=dict&of=args", f.Fragment.ToString());
            Assert.AreEqual("http://www.google.com/#!a=dict&of=args", f.Url);
        }
    }

    /// <summary>
    /// Furl handles encoding for you, and its philosophy on encoding is simple.
    /// </summary>
    [TestFixture]
    public class Encoding
    {
        [Test]
        public void Whole_path_query_and_fragment_strings_are_always_decoded()
        {
            var f = new Furl();

            f.Path = "supply%20encoded/whole%20path%20strings";
            CollectionAssert.AreEqual(new[] {"supply encoded", "whole path strings"},
                                      f.Path.Segments);

            f.Query = "supply+encoded=query+strings,+too";
            Assert.AreEqual("query strings, too", f.Query["supply encoded"]);

            f.Fragment = "encoded%20path%20string?and+encoded=query+string+too";
            CollectionAssert.AreEqual(new[] {"encoded path string"}, f.Fragment.Path.Segments);
            Assert.AreEqual("query string too", f.Fragment.Query["and encoded"]);

            f = new Furl();

            f.Path = new[] {"path segments are", "decoded", "<>[]\"#"};
            Assert.AreEqual("/path%20segments%20are/decoded/%3C%3E%5B%5D%22%23", f.Path.ToString());

            f.Query.UpdateAll("query parameters", "and values", "are", "decoded, too");
            Assert.AreEqual("query%20parameters=and%20values&are=decoded%2C%20too", f.Query.ToString());

            f.Fragment.Path.Segments = new[] {"decoded", "path segments"};
            f.Fragment.Query["and decoded"] = "query parameters and values";
            Assert.AreEqual("decoded/path%20segments?and%20decoded=query%20parameters%20and%20values",
                            f.Fragment.ToString());
        }
    }
}