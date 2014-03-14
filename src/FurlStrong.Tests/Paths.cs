using NUnit.Framework;

namespace Furlstrong.Tests
{
    [TestFixture]
    public class Paths
    {
        /// <summary>
        /// URL paths in furl are Path objects that have segments, a list of zero or more path segments that can be manipulated directly. Path segments in segments are maintaned decoded and all interaction with segments should take place with decoded segment strings. 
        /// </summary>
        [Test]
        public void Url_paths_are_Path_objecst_that_have_segments()
        {
            var f = Furl.Parse("http://www.google.com/a/larg%20ish/path");

            Assert.IsInstanceOf<FurlPath>(f.Path);

            CollectionAssert.AreEqual(
                new[] {"a", "larg%20ish", "path"}, f.Path.Segments
                );

            Assert.AreEqual("/a/larg%20ish/path", f.Path.ToString());
        }
    }
}