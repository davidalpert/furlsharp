using System.Collections.Generic;
using System.Linq;
using Furlstrong;

namespace FurlStrong
{
    public class FurlRouteCollection : List<FurlRoute>
    {
        public FurlRouteCollection() { }

        public FurlRouteCollection(IEnumerable<FurlRoute> routes) : base(routes) { }

        public static FurlRouteCollection Parse(string file)
        {
            var ast = FurlRouteCollectionParser.Parse(file);

            return new FurlRouteCollection(ast.Select(ASTtoFurl));
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