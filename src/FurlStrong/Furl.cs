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
            Segments = new List<string>(Decode(pathParts));
        }

        public List<string> Segments { get; set; }

        public override string ToString()
        {
            return "/" + string.Join("/", Segments.Select(Encode));
        }

        public static string Encode(string raw)
        {
            return HttpUtility.UrlEncode(raw).Replace("+", "%20");
        }

        public static string Decode(string encoded)
        {
            return HttpUtility.UrlDecode(encoded);
        }

        private static IEnumerable<string> Decode(IEnumerable<string> pathParts)
        {
            return pathParts.Select(Decode);
        }

        public static FurlPath Parse(string path)
        {
            var result = FurlPathParser.Parse(path);

            var p = new FurlPath(result.Item2);

            return p;
        }

        public static List<string> FromSegments(params string[] pathSegments)
        {
            return new List<string>(Decode(pathSegments));
        }
    }

    public class Furl
    {
        public Furl(string url)
        {
            InitializeWith(url, this);
        }

        public static Furl Parse(string url)
        {
            return new Furl(url);
        }

        private static void InitializeWith(string url, Furl f)
        {
            if (string.IsNullOrWhiteSpace(url))
            {
                f.Path = new FurlPath();
                return;
            }

            var result = FurlParser.Parse(url);

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

            var path = result.Item5 == null
                           ? null
                           : result.Item5.Value.Item2;
            f.Path = new FurlPath(path);
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

        public string Url { get { return ToString(); } }

        public override string ToString()
        {
            var sb = new StringBuilder();
            if (Scheme.IsNotNullOrWhiteSpace())
                sb.AppendFormat("{0}://", Scheme);

            var netloc = NetLoc;
            if (netloc.IsNotNullOrWhiteSpace())
                sb.Append(netloc);

            var path = Path.ToString();
            if (path.IsNotNullOrWhiteSpace())
                sb.Append(path);

            return sb.ToString();
        }
    }

    public static class StringExtensions
    {
        public static bool IsNotNullOrWhiteSpace(this string input)
        {
            return string.IsNullOrWhiteSpace(input) == false;
        }
    }
}
