using System.Collections.Generic;
using NUnit.Framework;

namespace Furlstrong.Tests
{
    /// <summary>
    /// URL paths in furl are Path objects that have segments, 
    /// a list of zero or more path segments that can be manipulated 
    /// directly. Path segments in segments are maintained 
    /// decoded and all interaction with segments should take place 
    /// with decoded segment strings. 
    /// </summary>
    [TestFixture]
    public class Paths
    {
        [Test]
        public void Url_paths_are_FurlPath_objects_that_have_segments()
        {
            var f = Furl.Parse("http://www.google.com/a/larg%20ish/path");

            Assert.IsInstanceOf<FurlPath>(f.Path);

            CollectionAssert.AreEqual(
                new[] { "a", "larg ish", "path" }, f.Path.Segments
                );

            Assert.AreEqual("/a/larg%20ish/path", f.Path.ToString());
        }

        [Test]
        public void URL_path_segments_can_be_manipulated_directly_as_decoded_strings()
        {
            var f = new Furl("http://www.google.com/");

            f.Path.Segments = FurlPath.FromSegments("a", "new", "path", "");
            Assert.AreEqual("/a/new/path/", f.Path.ToString());

            f.Path = FurlPath.Parse("o/hi/there/with%20some%20encoding/");
            
            CollectionAssert.AreEqual(
                new[] { "o", "hi", "there", "with some encoding", "" }, f.Path.Segments
                );

            Assert.AreEqual("/o/hi/there/with%20some%20encoding/", f.Path.ToString());

            Assert.AreEqual("http://www.google.com/o/hi/there/with%20some%20encoding/", f.ToString());

            f.Path.Segments = FurlPath.FromSegments("segments", "are", "maintained", "decoded", @"^`<>[]""#/?");
            Assert.AreEqual("/segments/are/maintained/decoded/%5E%60%3C%3E%5B%5D%22%23%2F%3F".ToLowerInvariant(), f.Path.ToString());
        }
    }
}















