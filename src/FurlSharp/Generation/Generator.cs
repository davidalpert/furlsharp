using System;
using System.IO;
using System.Linq;
using System.Reflection;
using RazorTemplates.Core;

namespace FurlSharp.Generation
{
    public class Generator
    {
        public string GenerateStrongUrls(string routes)
        {
            var routeMap = FurlRouteMap.Parse(routes);

            return GenerateStrongUrls(routeMap);
        }

        public string GenerateStrongUrls(FurlRouteMap map)
        {
            var model = new StronglyTypedUrlsViewModel()
                {
                    Map = map
                };

            var templateText = LoadFromResource();
            var template = Template.Compile<StronglyTypedUrlsViewModel>(templateText);
            return template.Render(model);
        }

        private static string LoadFromResource()
        {
            string templateResourceName = "FurlSharp.Generation.StronglyTypedUrls.cshtml";
            string templateText;
            var stream = Assembly.GetExecutingAssembly()
                                 .GetManifestResourceStream(templateResourceName);
            if (stream == null)
            {
                Assembly.GetExecutingAssembly().GetManifestResourceNames()
                        .Select(x =>
                            {
                                Console.WriteLine(x);
                                return x;
                            }).
                         ToList();
            }

            using (var reader = new StreamReader(stream))
            {
                templateText = reader.ReadToEnd();
            }

            return templateText;
        }
    }
}