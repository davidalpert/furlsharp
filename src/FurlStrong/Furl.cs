using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Web;
using FurlStrong;
using FurlStrong.AOP;
using FurlStrong.Internal;
using FurlStrong.Parsing.AST;
using Microsoft.FSharp.Collections;
using ASTPath = FurlStrong.Parsing.AST.Path;

namespace Furlstrong
{
    public class FurlPath
    {
        /// <summary>
        /// Supports implicit conversion from string[] to list of strings.
        /// </summary>
        public class FurlPathSegments : List<string>
        {
            public FurlPathSegments(IEnumerable<string> segments) 
            {
                if (segments != null)
                    AddRange(segments);
            }

            public static implicit operator FurlPathSegments(string[] segments)
            {
                return new FurlPathSegments(segments);
            }
        }

        private string _netloc;
        private bool _isAbsolute;

        public FurlPath(IEnumerable<string> pathParts = null)
        {
            pathParts = pathParts ?? Enumerable.Empty<string>();
            Segments = new FurlPathSegments((FurlUtility.Decode(pathParts)));
        }

        public FurlPath(bool isAbsolute, IEnumerable<string> pathParts = null, bool hasTrailingSlash = false)
        {
            Initialize(isAbsolute, pathParts, hasTrailingSlash);
        }

        internal FurlPath(ASTPath path)
        {
            var isAbsolute = path.Item1;
            var pathNodes = path.Item2;
            var lastPathNode = pathNodes.LastOrDefault();
            var hasTrailingSlash = lastPathNode != null && lastPathNode.Item2;
            var pathParts = pathNodes.Select(n => n.Item1).ToList();

            Initialize(isAbsolute, pathParts, hasTrailingSlash);
        }

        private void Initialize(bool isAbsolute, IEnumerable<string> pathParts, bool hasTrailingSlash)
        {
            IsAbsolute = isAbsolute;
            pathParts = pathParts ?? Enumerable.Empty<string>();
            Segments = new FurlPathSegments(FurlUtility.Decode(pathParts));

            if (hasTrailingSlash) Segments.Add("");
        }

        public FurlPathSegments Segments { get; set; }

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

            Segments = new FurlPathSegments(normalizedSegments);

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

            var p = new FurlPath(result);

            return p;
        }

        public static FurlPathSegments FromSegments(params string[] pathSegments)
        {
            return new FurlPathSegments(FurlUtility.Decode(pathSegments));
        }

        public void UseNetloc(string netLoc)
        {
            _netloc = netLoc;
            if (_netloc.IsNotNullOrWhiteSpace())
            {
                IsAbsolute = true;
            }
        }

        public FurlPath Append(string pathSegment)
        {
            var decoded = FurlUtility.Decode(pathSegment);
            if (Segments.Any() && Segments.Last() == "")
            {
                var targetIndex = Segments.Count - 1;
                Segments.Insert(targetIndex, decoded);
            }
            else
            {
                Segments.Add(decoded);
            }

            return this;
        }

        public static implicit operator FurlPath(string path)
        {
            return Parse(path);
        }

        public static implicit operator FurlPath(string[] segments)
        {
            return new FurlPath {Segments = FromSegments(segments), IsAbsolute = true};
        }
    }

    public static class FurlUtility
    {
        public static string Encode(string raw)
        {
            return raw == null 
                ? null 
                : HttpUtility.UrlEncode(raw).Replace("+", "%20");
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
        public Furl(string url = null)
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
            f.Fragment = new FurlFragment();

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
                : new FurlPath(path.Value);

            var query = result.Item6;
            if (query != null)
            {
                f.Query = new FurlQuery(query.Value.Item);
            }

            var fragment = result.Item7;
            var fragmentPath = fragment == null ? null : fragment.Value.Item1;
            f.Fragment = fragmentPath == null
                             ? new FurlFragment()
                             : new FurlFragment(fragmentPath);

            var fragmentQuery = fragment == null ? null : fragment.Value.Item2;
            f.Fragment.Query = fragmentQuery == null
                             ? new FurlQuery()
                             : new FurlQuery(fragmentQuery.Value.Item);
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

        public Furl Copy()
        {
            return new Furl(Url);
        }

        public Furl Join(string newPath)
        {
            var newUrl = Parse(newPath);
            if (newUrl.NetLoc.IsNotNullOrWhiteSpace())
            {
                return newUrl;
            }

            Query = newUrl.Query;
            Fragment = newUrl.Fragment;

            if (newUrl.Path.IsAbsolute || Path.Segments.Count == 0)
            {
                Path = newUrl.Path;
            }
            else
            {
                if (Path.IsFile)
                {
                    Path.Segments.RemoveLast();
                }

                var newSegments = newUrl.Path.Segments;
                while (Path.Segments.Count > 0 
                    && newSegments.FirstOrDefault() == "..")
                {
                    Path.Segments.RemoveLast();
                    newSegments.RemoveFirst();
                }
                Path.Segments.AddRange(newSegments);
            }

            return this;
        }
    }

    public class FurlQuery : OMDict
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
                Add(key, value);
            }
        }

        public override string ToString()
        {
            return Serialize();
        }

        public string Serialize(char delimeter = '&', char separator = '=')
        {
            if (Keys.Any() == false)
            {
                return null;
            }

            var sb = new StringBuilder();
            foreach (var item in AllItems())
            {
                var key = FurlUtility.Encode(item.Key);
                var value = FurlUtility.Encode(item.Value);
                if (value == null)
                {
                    sb.Append(key);
                }
                else
                {
                    sb.AppendFormat("{0}{1}{2}",key,separator,value);
                }
                sb.Append(delimeter);
            }
            return sb.ToString().TrimEnd(delimeter);
        }

        public string Encode(char delimeter = '&')
        {
            return Serialize(delimeter: delimeter);
        }

        public static FurlQuery Parse(string querystring)
        {
            var result = FurlQueryStringParser.Parse("?" + querystring.TrimStart('?'));

            var p = new FurlQuery(result.Item);

            return p;
        }

        public static implicit operator FurlQuery(string querystring)
        {
            return Parse(querystring);
        }
    }

    public class FurlFragment
    {
        public FurlFragment()
        {
            Path = new FurlPath();
            Query = new FurlQuery();
            HasSeparator = true;
        }

        internal FurlFragment(ASTPath path)
        {
            Path = new FurlPath(path);
            Query = new FurlQuery();
            HasSeparator = true;
        }

        public FurlPath Path { get; set; }
        public FurlQuery Query { get; set; }
        public bool HasSeparator { get; set; }

        public override string ToString()
        {
            var path = Path.ToString();
            var query = Query.Serialize();
            var separator = HasSeparator ? "?" : "";
            return query.IsNotNullOrWhiteSpace()
                       ? path + separator + query
                       : path;
        }

        public static implicit operator FurlFragment(string fragment)
        {
            if (string.IsNullOrWhiteSpace(fragment)) return new FurlFragment();

            fragment = fragment.StartsWith("#") ? fragment : "#" + fragment;

            // TODO: refator this duplication from the Furl.Initialize method
            var x = FurlParser.ParseFragment(fragment);
            var fragmentPath = x == null ? null : x.Item1;
            var frag = fragmentPath == null
                             ? new FurlFragment()
                             : new FurlFragment(fragmentPath);

            var fragmentQuery = x == null ? null : x.Item2;
            frag.Query = fragmentQuery == null
                             ? new FurlQuery()
                             : new FurlQuery(fragmentQuery.Value.Item);

            return frag;
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
