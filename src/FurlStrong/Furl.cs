using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;

namespace Furlstrong
{
    public class FurlPath
    {
        public FurlPath(IEnumerable<string> pathParts = null)
        {
            pathParts = pathParts ?? Enumerable.Empty<string>();
            Segments = new List<string>(Unencode(pathParts));
        }

        private static IEnumerable<string> Unencode(IEnumerable<string> pathParts)
        {
            return pathParts.Select(HttpUtility.UrlDecode);
        }

        public List<string> Segments { get; private set; }

        public override string ToString()
        {
            return "/" + string.Join("/", Segments.Select(HttpUtility.UrlPathEncode));
        }

        public static FurlPath Parse(string path)
        {
            var result = FurlPathParser.Parse(path);

            var p = new FurlPath(result.Item2);

            return p;
        }
    }

    public class Furl
    {
        public static Furl Parse(string url)
        {
            var result = FurlParser.Parse(url);

            var f = new Furl();

            var scheme = result.Item1;
            f.Scheme = scheme != null
                           ? scheme.Value.Item.ToLowerInvariant()
                           : null;

            var credentials = result.Item2;
            if (credentials != null)
            {
                f.Username = credentials.Value.Item1.Item;
                f.Password = credentials.Value.Item2.Item;
            }

            var host = result.Item3;
            f.Host = host != null
                           ? host.Value.Item
                           : null;

            var port = result.Item4;
            f.Port = port != null 
                ? port.Value.Item 
                : GetDefaultPortFor(f.Scheme);

            var path = result.Item5;
            f.Path = path != null
                         ? new FurlPath(path.Value.Item2)
                         : null;

            return f;
        }

        private static int? GetDefaultPortFor(string scheme)
        {
            return CommonSchemes.ContainsKey(scheme)
                       ? CommonSchemes[scheme]
                       : (int?) null;
        }

        private static readonly Dictionary<string, int> CommonSchemes =
            new Dictionary<string, int>
                {
                    {"http", 80},
                    {"https", 443},
                };

        public string Scheme { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Host { get; private set; }
        public int? Port { get; private set; } 

        public string NetLoc
        {
            get { 
                var sb = new StringBuilder();
                if (Username.IsNotNullOrWhiteSpace() && Password.IsNotNullOrWhiteSpace())
                    sb.AppendFormat("{0}:{1}@", Username, Password);
                if (Host.IsNotNullOrWhiteSpace())
                    sb.Append(Host);
                if (Port.HasValue && Port.Value != GetDefaultPortFor(Scheme))
                    sb.AppendFormat(":{0}", Port);
                return sb.ToString();
            }
        }

        public FurlPath Path { get; set; }
    }

    public static class StringExtensions
    {
        public static bool IsNotNullOrWhiteSpace(this string input)
        {
            return string.IsNullOrWhiteSpace(input) == false;
        }
    }
}
