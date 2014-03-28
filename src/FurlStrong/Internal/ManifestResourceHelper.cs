using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace FurlStrong.Internal
{
    public static class ManifestResourceHelper
    {
        public static string[] GetManifestResourceNames(Assembly asm = null)
        {
            asm = asm ?? Assembly.GetCallingAssembly();

            return asm.GetManifestResourceNames();
        }

        public static IEnumerable<string> GetManifestResourcePaths(Assembly asm = null)
        {
            asm = asm ?? Assembly.GetCallingAssembly();
            return asm.GetManifestResourceNames();
        }

        public static FileInfo ExtractResourceToDisk(string relativePathToResource, string targetPathOnDisk, bool overwrite = true)
        {
            var content = ExtractResourceToString(relativePathToResource, Assembly.GetCallingAssembly());
            var file = new FileInfo(targetPathOnDisk);
            if (File.Exists(file.FullName) && overwrite)
            {
                File.Delete(file.FullName);
            }
            File.WriteAllText(file.FullName, content);
            return file;
        }

        public static string ExtractRelativeResourceToString(string relativePathToResource, Assembly asm = null)
        {
            if (string.IsNullOrWhiteSpace(relativePathToResource)) return "";

            asm = asm ?? Assembly.GetCallingAssembly();
            var resourceName = asm.GetName().Name + "." + relativePathToResource.Replace('/','\\').Replace('\\', '.');
            return ExtractResourceToString(resourceName, asm);
        }

        public static string ExtractResourceToString(string resourceName, Assembly asm = null)
        {
            if (string.IsNullOrWhiteSpace(resourceName)) return "";

            asm = asm ?? Assembly.GetCallingAssembly();
            var resourceInfo = asm.GetManifestResourceInfo(resourceName);
            if (resourceInfo == null)
            {
                var message = "Could not find the requested resource among: ";
                var names = asm.GetManifestResourceNames();
                message += string.Join(";", names);
                throw new FileNotFoundException(message, resourceName);
            }

            string result;
            using (var reader = new StreamReader(asm .GetManifestResourceStream(resourceName)))
            {
                result = reader.ReadToEnd();
            }
            return result;
        }
    }
}