using System;
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
                new[] { "o", "hi", "there", "with some encoding", ""}, f.Path.Segments
                );

            Assert.AreEqual("/o/hi/there/with%20some%20encoding/", f.Path.ToString());

            Assert.AreEqual("http://www.google.com/o/hi/there/with%20some%20encoding/", f.ToString());

            f.Path.Segments = FurlPath.FromSegments("segments", "are", "maintained", "decoded", @"^`<>[]""#/?");
            Assert.AreEqual("/segments/are/maintained/decoded/%5E%60%3C%3E%5B%5D%22%23%2F%3F".ToLowerInvariant(), f.Path.ToString());
        }

        [Test]
        public void A_path_that_starts_with_backslash_is_considered_absolute()
        {
            var f = new Furl("/url/path");
            Assert.IsTrue(f.Path.IsAbsolute, "/url/path should be absolute.");
        }

        [Test]
        public void A_path_can_be_absolute_or_not_as_specified_or_set()
        {
            var f = new Furl("/url/path");

            f.Path.IsAbsolute = false;
            Assert.AreEqual("url/path",f.Path.ToString());

            f.Path.IsAbsolute = true;
            Assert.AreEqual("/url/path",f.Path.ToString());
        }

        /// <remarks>
        /// This restriction exists because a URL path must start with '/' 
        /// to separate itself from a netloc. 
        /// </remarks>
        [Test]
        public void URL_paths_must_be_absolute_if_a_netloc_is_present()
        {
            var f = new Furl("/url/path");

            f.Path.IsAbsolute = false;
            Assert.AreEqual("url/path",f.Path.ToString());

            f.Host = "arc.io";
            Assert.AreEqual("arc.io/url/path", f.Url);
            Assert.IsTrue(f.Path.IsAbsolute, "arc.io/url/path should be absolute.");

            var ex = Assert.Throws<InvalidOperationException>(() =>
                {
                    f.Path.IsAbsolute = false;
                });

            Assert.AreEqual(
                "Path.IsAbsolute is True and read-only for URLs with a Netloc (a username, password, host, and/or port). URL paths must be absolute if a netloc exists.",
                ex.Message);

            Assert.AreEqual("arc.io/url/path", f.Url);
        }


        [Test]
        public void Here_is_a_fragment_path_example()
        {
            var f = new Furl("http://www.google.com/#/absolute/fragment/path/");

            Assert.IsTrue(f.Path.IsAbsolute, "http://www.google.com/ is an absolute path");
            Assert.AreEqual("/", f.Path.ToString(), "The '#' character was mistakenly parsed as a path node.");

            Assert.IsTrue(f.Fragment.Path.IsAbsolute, "#/absolute/fragment/path/ describes an absolute fragment path.");

            f.Fragment.Path.IsAbsolute = false;
            Assert.AreEqual("http://www.google.com/#absolute/fragment/path/", f.Url);

            f.Fragment.Path.IsAbsolute = true;
            Assert.AreEqual("http://www.google.com/#/absolute/fragment/path/", f.Url);
        }



        /// <summary>
        /// A path that ends with '/' is considered a directory, and otherwise considered a file. 
        /// The Path attribute isdir returns True if the path is a directory, False otherwise. 
        /// Conversely, the attribute isfile returns True if the path is a file, False otherwise.
        /// </summary>
        [TestCase("http://www.google.com/a/directory/", true, false)]
        [TestCase("http://www.google.com/a/file", false, true)]
        [Theory]
        public void A_path_that_ends_in_a_slash_is_considered_a_directory_and_otherwise_a_file(string url, bool isDirectory, bool isFile)
        {
            var f = new Furl(url);

            Assert.AreEqual(isDirectory, f.Path.IsDirectory, url + " is a directory.");
            Assert.AreEqual(isFile, f.Path.IsFile, url + " is a file.");
        }
/*

A path can be normalized with normalize(). normalize() returns the Path object for method chaining.

>>> f = furl('http://www.google.com////a/./b/lolsup/../c/')
>>> f.path.normalize()
>>> f.url
'http://www.google.com/a/b/c/'
         */
    }
}















