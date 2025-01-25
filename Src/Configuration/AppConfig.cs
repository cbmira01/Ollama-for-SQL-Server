
/********************************************************************

    Use this class to configure all solution projects.

********************************************************************/

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

        // API settings
        public static string ApiUrl => "http://127.0.0.1:11434";
        public static string GenerateEndpointUrl => $"{ApiUrl}/api/generate";
        public static string TagEndpointUrl => $"{ApiUrl}/api/tags";
        public static int ApiTimeoutMs => 100000; // 100 seconds

        // Connection strings and properties
        public static string SqlClrContextConnection => "context connection=true";
        public static string SqlServerConnection = "Server=localhost;Trusted_Connection=True;";
        public static int SqlCommandTimeoutSecs => 300; // 5 minutes

        public static int CacheTimeoutMins => 2;
        public static int QueryProductionRetryLimit => 3;

        // Location of various deployment directories
        public static string RepoRootDirectory => FindRepoRoot();
        public static string ScriptsDirectory => Path.Combine(RepoRootDirectory, "Src", "DeploymentManager", "Scripts");
        public static string ImagesDirectory => Path.Combine(RepoRootDirectory, "Images");

        // "Sanity check" items (to get an initial completion after deployment)
        public static string SanityCommment => "Make sure the llama3.2 model is available.";
        public static string SanityModelName => "llama3.2";
        public static string SanityPrompt1 => "Would SQL Server, Ollama and Llama3.2 make a good team?";
        public static string SanityPrompt2 => "Answer me briefly.";

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

