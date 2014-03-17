using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using FurlStrong.AOP;
using FurlStrong.Parsing.AST;
using Microsoft.FSharp.Collections;
using ASTPath = Microsoft.FSharp.Collections.FSharpList<System.Tuple<string, bool>>;

namespace Furlstrong
{
    public class FurlPath
    {
        private string _netloc;
        private bool _isAbsolute;

        public FurlPath(IEnumerable<string> pathParts = null)
        {
            pathParts = pathParts ?? Enumerable.Empty<string>();
            Segments = new List<string>(FurlUtility.Decode(pathParts));
        }

        public FurlPath(bool isAbsolute, IEnumerable<string> pathParts = null, bool hasTrailingSlash = false)
        {
            Initialize(isAbsolute, pathParts, hasTrailingSlash);
        }

        internal FurlPath(bool isAbsolute, ASTPath path)
        {
            var lastPathNode = path.LastOrDefault();
            var hasTrailingSlash = lastPathNode != null && lastPathNode.Item2;
            var pathParts = path.Select(n => n.Item1).ToList();

            Initialize(isAbsolute, pathParts, hasTrailingSlash);
        }

        private void Initialize(bool isAbsolute, IEnumerable<string> pathParts, bool hasTrailingSlash)
        {
            IsAbsolute = isAbsolute;
            pathParts = pathParts ?? Enumerable.Empty<string>();
            Segments = new List<string>(FurlUtility.Decode(pathParts));

            if (hasTrailingSlash) Segments.Add("");
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

        public bool IsDirectory {
            get {
                return Segments.Any() == false 
                    ||  string.IsNullOrWhiteSpace(Segments.Last());
            }
        }

        public bool IsFile { get { return !IsDirectory; } }

        public FurlPath Normalize()
        {
            // TODO: refactor paths so that they are stored like a linked list
            //       path (isFile) -> path (isDirectory) ...
            //       so that path.ParentDirectory is like stripping off a node in the url

            var nodesToClear = this.Segments.Select((node, i) => node == ".." ? i - 1 : -1).ToArray();

            var normalizedSegments = this.Segments
                                         .Select((node, i) => nodesToClear.Any(j => j == i) ? "" : node.Replace('.', ' '))
                                         .Where(StringExtensions.IsNotNullOrWhiteSpace)
                                         .ToList();

            if (IsDirectory) 
                normalizedSegments.Add("");

            Segments = normalizedSegments;

            return this;
        }

        public override string ToString()
        {
            var path = string.Join("/", Segments.Select(FurlUtility.Encode));

            return IsAbsolute
                       ? "/" + path
                       : path;
        }

        public static FurlPath Parse(string path)
        {
            var result = FurlPathParser.Parse(path);

            var p = new FurlPath(result.Item1, result.Item2);

            return p;
        }

        public static List<string> FromSegments(params string[] pathSegments)
        {
            return new List<string>(FurlUtility.Decode(pathSegments));
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

    public static class FurlUtility
    {
        public static string Encode(string raw)
        {
            return HttpUtility.UrlEncode(raw).Replace("+", "%20");
        }

        public static string Decode(string encoded)
        {
            return HttpUtility.UrlDecode(encoded);
        }

        public static IEnumerable<string> Decode(IEnumerable<string> pathParts)
        {
            return pathParts.Select(Decode);
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
            f.Query = new FurlQuery();

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

            var query = result.Item6;
            if (query != null)
            {
                f.Query = new FurlQuery(query.Value.Item);
            }

            var fragment = result.Item7;
            var fragmentPath = fragment == null ? null : fragment.Value.Item;
            f.Fragment = fragmentPath == null
                             ? new FurlFragment()
                             : new FurlFragment(fragmentPath.Item1, fragmentPath.Item2);
        }

        private static int? GetDefaultPortFor(string scheme)
        {
            if (string.IsNullOrWhiteSpace(scheme))
                return null;

            return CommonSchemes.ContainsKey(scheme)
                       ? CommonSchemes[scheme]
                       : (int?) null;
        }

        public FurlQuery Query { get; set; }

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

        public FurlFragment Fragment { get; set; }

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

            var query = Query.Serialize();
            if (query.IsNotNullOrWhiteSpace())
                sb.AppendFormat("?{0}", query);

            var fragment = Fragment.ToString();
            if (fragment.IsNotNullOrWhiteSpace())
                sb.AppendFormat("#{0}", fragment);

            return sb.ToString();
        }
    }

    public class FurlQuery : NameValueCollection
    {
        public FurlQuery()
        {
        }

        public FurlQuery(IEnumerable<QueryStringParameter> parameters)
        {
            foreach (var param in parameters)
            {
                var key = FurlUtility.Decode(param.Item1);
                var value = FurlUtility.Decode(param.Item2.Value);
                this[key] = value;
            }
        }

        public override string ToString()
        {
            return Serialize();
        }

        public string Serialize(char separator = '=')
        {
            if (HasKeys() == false)
            {
                return null;
            }

            var sb = new StringBuilder();
            var n = Count;
            for(var i = 0;i < n;i++)
            {
                var key = FurlUtility.Encode(GetKey(i));
                var value = FurlUtility.Encode(Get(i));
                if (value == null)
                {
                    sb.Append(key);
                }
                else
                {
                    sb.AppendFormat("{0}{1}{2}",key,separator,value);
                }
                sb.Append('&');
            }
            return sb.ToString().TrimEnd('&');
        }

        public new string this[string name]
        {
            get { return base[name]; }
            set
            {
                if (value == null)
                    Remove(name);
                else
                    base[name] = value;
            }
        }

        public IEnumerable<string> GetList(string key)
        {
            return null;
        }

        public void AddList(string key, params string[] values)
        {
        }

        public string PopValue(string key)
        {
            return null;
        }

        public string PopValue(string key, string value)
        {
            return null;
        }

        public static FurlQuery Parse(string querystring)
        {
            var result = FurlQueryStringParser.Parse("?" + querystring.TrimStart('?'));

            var p = new FurlQuery(result.Item);

            return p;
        }
    }

    public class FurlFragment
    {
        public FurlFragment()
        {
            Path = new FurlPath();
        }

        internal FurlFragment(bool item1, ASTPath item2)
        {
            Path = new FurlPath(item1, item2);
        }

        public FurlPath Path { get; set; }

        public override string ToString()
        {
            return Path.ToString();
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
