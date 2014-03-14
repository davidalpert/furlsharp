
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
                    Username = result.Item2.Value.Item1.Item,
                    Password = result.Item2.Value.Item2.Item,
                    Host = result.Item3.Value.Item,
                    Port = result.Item4.Value.Item,
                };

            return f;
        }

        public string Scheme { get; private set; }
        public string Username { get; private set; }
        public string Password { get; private set; }
        public string Host { get; private set; }
        public int Port { get; private set; }
    }
}