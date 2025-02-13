using System;
using System.IO;
using CommandLine;

namespace ResponsiveBackendCLI.Commands
{
    [Verb("init", HelpText = "Initializes a new API project.")]
    public class InitCommand
    {
        public void Execute()
        {
            Console.WriteLine("Initializing Responsive Backend Project...");

            if (!Directory.Exists("api"))
            {
                Directory.CreateDirectory("api");
                File.WriteAllText("api/api.yaml", GetDefaultApiDefinition());
                Console.WriteLine("Created api/api.yaml");
            }

            if (!Directory.Exists("config"))
            {
                Directory.CreateDirectory("config");
                File.WriteAllText("config/settings.yaml", GetDefaultConfig());
                Console.WriteLine("Created config/settings.yaml");
            }

            Console.WriteLine("Project initialized successfully!");
        }

        private string GetDefaultApiDefinition()
        {
            return @"title: Sample API
version: 1.0
endpoints:
  - path: '/users/{id}'
    method: GET
    description: 'Fetch user by ID'
    response:
      200:
        json:
          id: int
          name: string
          email: string";
        }

        private string GetDefaultConfig()
        {
            return @"authentication:
  method: 'JWT'
  secret: 'your-secret-key'
logging:
  enabled: true
  log_level: 'info'
rate_limiting:
  requests_per_minute: 60";
        }
    }
}
