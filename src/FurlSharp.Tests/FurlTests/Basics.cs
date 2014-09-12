using NUnit.Framework;

/// <summary>
/// A port of the furl python library's API: https://github.com/gruns/furl/blob/master/API.md
/// </summary>
namespace FurlSharp.Tests.FurlTests
{
    /// <summary>
    /// Given a url:
    /// 
    /// - scheme://username:password@host:port/path?query#fragment
    /// 
    /// Furl will parse that into:
    /// 
    /// - **scheme** is the scheme string (all lowercase) or None. None means no scheme. An empty string means a protocol relative URL, like //www.google.com.
    /// - **username** is the username string for authentication.
    /// - **password** is the password string for authentication with username.
    /// - **host** is the domain name, IPv4, or IPv6 address as a string. Domain names are all lowercase.
    /// - **port** is an integer or None. A value of None means no port specified and the default port for the given scheme should be inferred, if possible.
    /// - **path** is a Path object comprised of path segments.
    /// - **query** is a Query object comprised of query arguments.
    /// - **fragment** is a Fragment object comprised of a Path and Query object separated by an optional '?' separator.
    /// </summary>
    [TestFixture]
    public class Basics
    {
        /// <summary>
        /// furl objects let you access and modify the components of a URL
        /// </summary>
        [Test]
        public void Furl_objects_let_you_access_and_modify_the_components_of_a_url()
        {
            var f = Furl.Parse("http://user:pass@www.google.com:90/");

            Assert.AreEqual("http", f.Scheme);
            Assert.AreEqual("user", f.Username);
            Assert.AreEqual("pass", f.Password);
            Assert.AreEqual("www.google.com", f.Host);
            Assert.AreEqual(90, f.Port);
        }

        /// <summary>
        /// furl infers the default port for common schemes.
        /// </summary>
        [TestCase("http://www.google.com/", 80)]
        [TestCase("https://secure.google.com/", 443)]
        [TestCase("unknown://secure.google.com/", null)]
        [Theory]
        public void Furl_infers_the_default_port_for_common_schemes(string url, int? expectedPort)
        {
            var f = Furl.Parse(url);

            Assert.AreEqual(expectedPort, f.Port);
        }

        /// <summary>
        /// netloc is the string combination of username, password, host, and port, not including port if it is None or the default port for the provided scheme.
        /// </summary>
        [TestCase("http://www.google.com/", "www.google.com")]
        [TestCase("http://www.google.com:99/", "www.google.com:99")]
        [TestCase("http://user:pass@www.google.com:99/", "user:pass@www.google.com:99")]
        [Theory]
        public void Netloc_is_the_string_combination_of_username_password_host_and_port(string url, string expectedNetloc)
        {
            var f = Furl.Parse(url);

            Assert.AreEqual(expectedNetloc, f.NetLoc);
        }
    }
}
