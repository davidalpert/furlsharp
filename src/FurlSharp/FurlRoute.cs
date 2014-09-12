using System.Net.Http;
using FurlSharp;

namespace FurlSharp
{
    public class FurlRoute
    {
        public FurlPath Path { get; set; }

        public string Comment { get; set; }

        public HttpMethod Method { get; set; }
    }
}