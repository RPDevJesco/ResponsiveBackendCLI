using System;
using CommandLine;
using ResponsiveBackendCLI.Commands;

namespace ResponsiveBackendCLI
{
    class Program
    {
        static void Main(string[] args)
        {
            Parser.Default.ParseArguments<InitCommand, GenerateCommand, ServeCommand>(args)
                .WithParsed<object>(cmd =>
                {
                    switch (cmd)
                    {
                        case InitCommand init:
                            init.Execute();
                            break;
                        case GenerateCommand generate:
                            generate.Execute();
                            break;
                        case ServeCommand serve:
                            serve.Execute();
                            break;
                    }
                });
        }
    }
}