using System.Net.Http;
using Furlstrong;

namespace FurlStrong
{
    public class FurlRoute
    {
        public FurlPath Path { get; set; }

        public string Comment { get; set; }

        public HttpMethod Method { get; set; }
    }
}