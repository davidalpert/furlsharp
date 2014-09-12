using System.Collections.Generic;
using System.Linq;
using FurlSharp;

namespace FurlSharp
{
    public class FurlRouteMap : List<FurlRoute>
    {
        public FurlRouteMap() : this(null, Enumerable.Empty<FurlRoute>()) {}

        public FurlRouteMap(string ns, IEnumerable<FurlRoute> routes) : base(routes)
        {
            NamespaceForGeneratedCode = ns;
        }

        public string NamespaceForGeneratedCode { get; set; }

        public static FurlRouteMap Parse(string map)
        {
            var ast = FurlRouteMapParser.Parse(map);

            var ns = ast.Item1.Item == null ? null : ast.Item1.Item.Value;
            var routes = ast.Item2.Select(ASTtoFurl);

            return new FurlRouteMap(ns, routes);
        }

        public static FurlRoute ASTtoFurl(Parsing.AST.Route route)
        {
            var r = new FurlRoute();
            r.Method = route.Item1;
            r.Path = new FurlPath(route.Item2);
            var comment = route.Item3;
            if (comment != null && comment.Item != null)
            {
                r.Comment = comment.Item.Value;
            }

            return r;
        }
    }
}