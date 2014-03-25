using NUnit.Framework;

namespace Furlstrong.Tests.FurlTests
{
    [TestFixture]
    public class Miscellaneous
    {
        [Test]
        public void Copy_creates_and_returns_a_new_furl_object_with_an_identical_URL()
        {
            var f = new Furlstrong.Furl("http://www.google.com");

            var f2 = f.Copy();
            Assert.AreNotSame(f,f2);

            f2.Path = "/new/path";
            Assert.AreEqual("http://www.google.com/new/path", f2.Url);
            Assert.AreEqual("http://www.google.com/", f.Url);
        }

        /// <summary>
        /// Join results in the same url as if you had clicked on the provided relative or absolute URL in a browser.
        /// </summary>
        [Test]
        public void Join_joins_a_url_with_the_provided_relative_or_absolute_url_and_is_chainable()
        {
            var f = new Furlstrong.Furl("http://www.google.com");

            Assert.AreEqual("http://www.google.com/new/path", f.Join("new/path").Url);
            Assert.AreEqual("http://www.google.com/new/replaced", f.Join("replaced").Url);
            Assert.AreEqual("http://www.google.com/parent", f.Join("../parent").Url);
            Assert.AreEqual("http://www.google.com/path?query=yes#fragment", f.Join("path?query=yes#fragment").Url);
            Assert.AreEqual("unknown://www.yahoo.com/new/url/", f.Join("unknown://www.yahoo.com/new/url/").Url);
        }
    }
}