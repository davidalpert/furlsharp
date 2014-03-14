using NUnit.Framework;

namespace Furlstrong.Tests
{

    /// <summary>
    /// furl objects let you access and modify the components of a URL
    /// </summary>
    /// <remarks>
    /// Given a url:
    /// - scheme://username:password@host:port/path?query#fragment
    /// Furl will parse that into:
    /// - **scheme** is the scheme string (all lowercase) or None. None means no scheme. An empty string means a protocol relative URL, like //www.google.com.
    /// - **username** is the username string for authentication.
    /// - **password** is the password string for authentication with username.
    /// - **host** is the domain name, IPv4, or IPv6 address as a string. Domain names are all lowercase.
    /// - **port** is an integer or None. A value of None means no port specified and the default port for the given scheme should be inferred, if possible.
    /// - **path** is a Path object comprised of path segments.
    /// - **query** is a Query object comprised of query arguments.
    /// - **fragment** is a Fragment object comprised of a Path and Query object separated by an optional '?' separator.
    /// </remarks>
    [TestFixture]
    public class Basics
    {
        /// <summary>
        /// >>> f = furl('http://user:pass@www.google.com:99/')
        // >>> f.scheme, f.username, f.password, f.host, f.port
        // ('http', 'user', 'pass', 'www.google.com', 99)p
        /// </summary>
        [Test]
        public void Http()
        {
            var f = Furl.Parse("http://user:pass@www.google.com:90");

            Assert.AreEqual("http", f.Scheme);
            Assert.AreEqual("user", f.Username);
            Assert.AreEqual("pass", f.Password);
            Assert.AreEqual("www.google.com", f.Host);
            Assert.AreEqual(90, f.Port);
        }
    }
}