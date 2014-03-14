namespace Furlstrong
{
    public class Furl
    {
        public static Furl Parse(string url)
        {
            var result = FurlParser.Parse(url);

            var f = new Furl()
                {
                    Scheme = result.Item1.Value.Item,
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

            return f;
        }

        private static int? GetDefaultPortFor(string scheme)
        {
            switch (scheme.ToLowerInvariant())
            {
                case "http": return 80;
                case "https": return 443;
                default: return null;
            }
        }

        public string Scheme { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Host { get; private set; }
        public int? Port { get; private set; }
    }
}
