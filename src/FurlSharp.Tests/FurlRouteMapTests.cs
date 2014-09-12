using System.Linq;
using NUnit.Framework;

namespace FurlSharp.Tests
{
    [TestFixture]
    public class FurlRouteMapTests
    {
        [Test]
        public void A_route_map_is_a_list_of_routes()
        {
            var file = @"
GET /path/to/something
GET /path/to/something/else
GET /path/to/something/else/again
";
            var routes = FurlRouteMap.Parse(file);

            Assert.AreEqual(3, routes.Count);
        }

        [Test]
        public void A_route_map_can_contain_comments()
        {
            var file = @"
GET /path/to/something              # something special
GET /path/to/something/else         # this one is to register widgets
GET /path/to/something/else/again   # this one is to get widget detail
";
            var routes = FurlRouteMap.Parse(file);

            Assert.AreEqual(3, routes.Count);

            var r = routes.First();

            Assert.AreEqual("something special", r.Comment);
        }

        [Test]
        public void A_route_map_can_start_with_a_namespace_for_the_generated_code()
        {
            var file = @"
namespace FurlSharp.Sample

GET /path/to/something
GET /path/to/something/else
GET /path/to/something/else/again
";
            var routes = FurlRouteMap.Parse(file);

            Assert.AreEqual(3, routes.Count);
            Assert.AreEqual("FurlSharp.Sample", routes.NamespaceForGeneratedCode);
        }
    }
}