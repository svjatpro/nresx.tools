using System.Reflection;

namespace nresx.Tools
{
    public class ResourceManager
    {
        public static string GetVersion()
        {
            var assembly = Assembly.GetAssembly( typeof( ResourceManager  ) );
            var ver = assembly.GetName().Version;
            var version = $"v{ver.Major}.{ver.Minor}.{ver.Revision}";

            return version;
        }
    }
}
