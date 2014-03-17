using NUnit.Framework;

namespace Furlstrong.Tests
{
    /// <summary>
    /// URL queries in furl are Query objects that have params, 
    /// a one dimensional ordered multivalue dictionary of 
    /// query keys and values. 
    /// 
    /// Query keys and values in params are maintained decoded 
    /// and all interaction with params should take place with 
    /// decoded strings.
    /// </summary>
    [TestFixture]
    public class Query
    {
        [Test]
        public void URL_queries_in_Furl_are_FurlQuery_objects_that_have_params()
        {
            var f = new Furl("http://www.google.com/?one=1&two=2");
            Assert.AreEqual("http://www.google.com/?one=1&two=2",f.Url);

            Assert.AreEqual("one=1&two=2", f.Query.ToString());

            Assert.AreEqual("1", f.Query["one"]);
            Assert.AreEqual("2", f.Query["two"]);
        }

        [Test]
        public void URL_query_keys_and_values_are_maintained_decoded()
        {
            var f = new Furl("http://www.google.com/?on%20e=1%202&two=2");
            Assert.AreEqual("http://www.google.com/?on%20e=1%202&two=2",f.Url);

            Assert.AreEqual("1 2", f.Query["on e"]);
            Assert.AreEqual("2", f.Query["two"]);
        }
    }
}