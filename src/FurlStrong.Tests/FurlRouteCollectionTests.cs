using System.Linq;
using FurlStrong;
using NUnit.Framework;

namespace Furlstrong.Tests
{
    [TestFixture]
    public class FurlRouteCollectionTests
    {
        [Test]
        public void FurlRouteCollection_parses_a_collection_of_routes()
        {
            var file = @"
GET /path/to/something
GET /path/to/something/else
GET /path/to/something/else/again
";
            var routes = FurlRouteCollection.Parse(file);

            Assert.AreEqual(3, routes.Count);
        }

        [Test]
        public void A_route_collection_can_contain_comments()
        {
            var file = @"
GET /path/to/something              # something special
GET /path/to/something/else         # this one is to register widgets
GET /path/to/something/else/again   # this one is to get widget detail
";
            var routes = FurlRouteCollection.Parse(file);

            Assert.AreEqual(3, routes.Count);

            var r = routes.First();

            Assert.AreEqual("something special", r.Comment);
        }
    }
}