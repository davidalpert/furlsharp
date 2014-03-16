using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web;
using FurlStrong.AOP;

namespace Furlstrong
{
    public class FurlPath
    {
        private string _netloc;
        private bool _isAbsolute;

        public FurlPath(IEnumerable<string> pathParts = null)
        {
            pathParts = pathParts ?? Enumerable.Empty<string>();
            Segments = new List<string>(Decode(pathParts));
        }

        public FurlPath(bool isAbsolute, IEnumerable<string> pathParts = null)
        {
            IsAbsolute = isAbsolute;
            pathParts = pathParts ?? Enumerable.Empty<string>();
            Segments = new List<string>(Decode(pathParts));
        }

        public List<string> Segments { get; set; }

        public bool IsAbsolute
        {
            get { return _isAbsolute; }
            set
            {
                if (_netloc.IsNotNullOrWhiteSpace() && value == false)
                    throw new InvalidOperationException("Path.IsAbsolute is True and read-only for URLs with a Netloc (a username, password, host, and/or port). URL paths must be absolute if a netloc exists.");

                _isAbsolute = value;
            }
        }

        public override string ToString()
        {
            var path = string.Join("/", Segments.Select(Encode));

            return IsAbsolute
                       ? "/" + path
                       : path;
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

            var p = new FurlPath(result.Item1, result.Item2);

            return p;
        }

        public static List<string> FromSegments(params string[] pathSegments)
        {
            return new List<string>(Decode(pathSegments));
        }

        public void UseNetloc(string netLoc)
        {
            _netloc = netLoc;
            if (_netloc.IsNotNullOrWhiteSpace())
            {
                IsAbsolute = true;
            }
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
            f.Path = new FurlPath();

            if (string.IsNullOrWhiteSpace(url))
            {
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

            var path = result.Item5;
            f.Path = path == null
                         ? new FurlPath()
                         : new FurlPath(path.Value.Item1, path.Value.Item2);
        }

        private static int? GetDefaultPortFor(string scheme)
        {
            if (string.IsNullOrWhiteSpace(scheme))
                return null;

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

        private string _host;
        private string _scheme;
        private string _username;
        private string _password;
        private int? _port;
        private FurlPath _path;

        public string Scheme
        {
            get { return _scheme; }
            [UpdatePathNetlocOnExit] private set { _scheme = value; }
        }

        public string Username
        {
            get { return _username; }
            [UpdatePathNetlocOnExit] private set { _username = value; }
        }

        public string Password
        {
            get { return _password; }
            [UpdatePathNetlocOnExit] private set { _password = value; }
        }

        public string Host
        {
            get { return _host; }
            [UpdatePathNetlocOnExit] set { _host = value; }
        }

        public int? Port
        {
            get { return _port; }
            [UpdatePathNetlocOnExit] private set { _port = value; }
        }

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

        public FurlPath Path
        {
            get { return _path; }
            set { _path = value; 
                Path.UseNetloc(NetLoc);
            }
        }

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
