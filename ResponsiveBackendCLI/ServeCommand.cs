using System;
using System.IO;
using CommandLine;

namespace ResponsiveBackendCLI.Commands
{
    [Verb("serve", HelpText = "Runs a mock server for API testing.")]
    public class ServeCommand
    {
        public void Execute()
        {
            Console.WriteLine("Starting mock API server...");
            // TODO: Implement mock server logic
        }
    }
}
