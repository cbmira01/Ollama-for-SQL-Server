using System;
using System.Configuration;
using System.IO;
using System.Reflection;

namespace Configuration
{
    public static class AppConfig
    {
        public static string GetAppSetting(string key)
        {
            return ConfigurationManager.AppSettings[key];
        }

        public static string GetConnectionString(string name)
        {
            return ConfigurationManager.ConnectionStrings[name]?.ConnectionString;
        }

        // API settings
        public static string ApiUrl => GetAppSetting("ApiUrl");
        public static string GenerateEndpointUrl => $"{ApiUrl}/api/generate";
        public static string TagEndpointUrl => $"{ApiUrl}/api/tags";

        public static int QueryProductionRetryLimit => 
            int.TryParse(GetAppSetting("QueryProductionRetryLimit"), out var limit) ? limit : 0;

        public static int ApiTimeoutMs => 
            int.TryParse(GetAppSetting("ApiTimeoutMs"), out var timeout) ? timeout : 0;

        public static int CacheTimeoutMins => 
            int.TryParse(GetAppSetting("CacheTimeoutMins"), out var timeout) ? timeout : 0;

        // Connection strings and properties
        public static string SqlClrContextConnection => GetConnectionString("SqlClrContextConnection");
        public static string SqlServerConnection => GetConnectionString("SqlServerConnection");
        public static int SqlCommandTimeoutSecs => 
            int.TryParse(GetAppSetting("SqlCommandTimeoutSecs"), out var timeout) ? timeout : 0;

        // Locate various deployment directories
        public static string RepoRootDirectory => FindRepoRoot();
        public static string ScriptsDirectory => Path.Combine(RepoRootDirectory, "Src", "DeploymentManager", "Scripts");
        public static string ImagesDirectory => Path.Combine(RepoRootDirectory, "Images");

        // Helper to find the repository root directory
        private static string FindRepoRoot()
        {
            string currentDir = AppDomain.CurrentDomain.BaseDirectory;
            while (!Directory.Exists(Path.Combine(currentDir, ".git")) &&
                   Directory.GetParent(currentDir) != null)
            {
                currentDir = Directory.GetParent(currentDir).FullName;
            }

            return currentDir;
        }

        // Helper to allow symbols to be found by string value
        public static object GetSymbolValue(string symbolName)
        {
            var property = typeof(AppConfig)
                .GetProperty(symbolName, BindingFlags.Public | BindingFlags.Static);

            if (property != null)
            {
                return property.GetValue(null);
            }
            else
            {
                throw new ArgumentException($"No symbol found with the name '{symbolName}'.");
            }
        }
    }
}
