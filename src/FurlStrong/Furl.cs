using System.Collections.Generic;
using System.Text;

namespace Furlstrong
{
    public class FurlPath
    {
        public FurlPath(IEnumerable<string> pathParts)
        {
            Segments = new List<string>(pathParts);
        }

        public List<string> Segments { get; private set; }

        public override string ToString()
        {
            return "/" + string.Join("/", Segments);
        }
    }

    public class Furl
    {
        public static Furl Parse(string url)
        {
            var result = FurlParser.Parse(url);

            var f = new Furl()
                {
                    Scheme = result.Item1.Value.Item.ToLowerInvariant(),
                    Host = result.Item3.Value.Item,
                };

            var credentials = result.Item2;
            if (credentials != null)
            {
                f.Username = credentials.Value.Item1.Item;
                f.Password = credentials.Value.Item2.Item;
            }

            var port = result.Item4;
            f.Port = port != null 
                ? port.Value.Item 
                : GetDefaultPortFor(f.Scheme);

            var path = result.Item5;
            f.Path = path != null
                         ? new FurlPath(path.Value.Item)
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
