using System;
using System.IO;
using CommandLine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using ResponsiveBackendCLI.Models;

namespace ResponsiveBackendCLI.Commands
{
    [Verb("generate", HelpText = "Generates API scaffolding from YAML/Markdown.")]
    public class GenerateCommand
    {
        [Option('l', "language", Required = false, HelpText = "Specify the target language (csharp, ruby, javascript). Default is csharp.")]
        public string Language { get; set; } = "csharp";

        public void Execute()
        {
            string apiFilePath = "api/api.yaml";

            if (!File.Exists(apiFilePath))
            {
                Console.WriteLine("Error: No API definition found (api/api.yaml missing). Run `rb init` first.");
                return;
            }

            Console.WriteLine($"Parsing API definition for {Language}...");
            var apiDefinition = ParseApiDefinition(apiFilePath);

            if (apiDefinition == null)
            {
                Console.WriteLine("Error: Failed to parse API definition.");
                return;
            }

            switch (Language.ToLower())
            {
                case "csharp":
                    Console.WriteLine("Generating C# API scaffolding...");
                    GenerateCSharpScaffolding(apiDefinition);
                    break;
                case "ruby":
                    Console.WriteLine("Generating Ruby API scaffolding...");
                    GenerateRubyScaffolding(apiDefinition);
                    break;
                case "javascript":
                    Console.WriteLine("Generating JavaScript API scaffolding...");
                    GenerateJavaScriptScaffolding(apiDefinition);
                    break;
                default:
                    Console.WriteLine($"Error: Unsupported language '{Language}'. Use 'csharp', 'ruby', or 'javascript'.");
                    break;
            }
        }

        private ApiDefinition ParseApiDefinition(string filePath)
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(CamelCaseNamingConvention.Instance)
                .Build();

            var yamlContent = File.ReadAllText(filePath);
            return deserializer.Deserialize<ApiDefinition>(yamlContent);
        }

        private void GenerateCSharpScaffolding(ApiDefinition apiDefinition)
        {
            string outputDir = "src/GeneratedControllers";
            string partnerDir = "src/Controllers";

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            if (!Directory.Exists(partnerDir))
                Directory.CreateDirectory(partnerDir);

            foreach (var endpoint in apiDefinition.Endpoints)
            {
                string controllerName = GetControllerName(endpoint.Path);
                string generatedFileName = $"{controllerName}.Generated.cs";
                string partnerFileName = $"{controllerName}.cs";

                string generatedFilePath = Path.Combine(outputDir, generatedFileName);
                string partnerFilePath = Path.Combine(partnerDir, partnerFileName);

                Console.WriteLine($"Generating: {generatedFileName}...");
                File.WriteAllText(generatedFilePath, GenerateCSharpGeneratedController(endpoint));

                if (!File.Exists(partnerFilePath))
                {
                    Console.WriteLine($"Creating developer partial class: {partnerFileName}...");
                    File.WriteAllText(partnerFilePath, GenerateCSharpPartnerController(endpoint));
                }
            }
        }

        private string GenerateCSharpGeneratedController(Endpoint endpoint)
        {
            // Ensure authentication is always enforced
            string authAttribute = "[Authorize]";

            // Enforce role-based authorization if roles are defined
            if (endpoint.Auth?.Enforce == true && endpoint.Auth.Roles != null && endpoint.Auth.Roles.Count > 0)
            {
                authAttribute = $"[Authorize(Roles = \"{string.Join(",", endpoint.Auth.Roles)}\")]";
            }

            string httpMethod = $"Http{endpoint.Method.Substring(0, 1).ToUpper() + endpoint.Method.Substring(1).ToLower()}"; // Fix casing issue

            return $@"
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace GeneratedControllers
{{
    {authAttribute} // Enforce JWT authentication and optional role-based authorization
    [ApiController]
    [Route(""{endpoint.Path}"")]
    public partial class {GetControllerName(endpoint.Path)} : ControllerBase
    {{
        [{httpMethod}(""{endpoint.Path}"")]
        public IActionResult {GetMethodName(endpoint.Method)}()
        {{
            return {GetMethodName(endpoint.Method)}Implementation();
        }}

        partial IActionResult {GetMethodName(endpoint.Method)}Implementation();
    }}
}}";
        }

        private string GenerateCSharpPartnerController(Endpoint endpoint)
        {
            string controllerName = GetControllerName(endpoint.Path);
            string responseType = "IActionResult";

            return $@"
using Microsoft.AspNetCore.Mvc;

namespace Controllers
{{
    public partial class {controllerName}
    {{
        partial {responseType} {GetMethodName(endpoint.Method)}Implementation()
        {{
            // TODO: Implement business logic here
            return Ok(new {{ message = ""Replace this with actual logic"" }});
        }}
    }}
}}";
        }
        
        private void GenerateRubyScaffolding(ApiDefinition apiDefinition)
        {
            string generatedDir = "src/generated";
            string controllerDir = "src/controllers";

            if (!Directory.Exists(generatedDir))
                Directory.CreateDirectory(generatedDir);
            if (!Directory.Exists(controllerDir))
                Directory.CreateDirectory(controllerDir);

            foreach (var endpoint in apiDefinition.Endpoints)
            {
                string className = GetRubyClassName(endpoint.Path);
                string generatedFilePath = Path.Combine(generatedDir, $"{className}.rb");
                string controllerFilePath = Path.Combine(controllerDir, $"{className}.rb");

                Console.WriteLine($"Generating: {generatedFilePath}...");
                File.WriteAllText(generatedFilePath, GenerateRubyBaseController(endpoint));

                if (!File.Exists(controllerFilePath))
                {
                    Console.WriteLine($"Creating developer mixin: {controllerFilePath}...");
                    File.WriteAllText(controllerFilePath, GenerateRubyMixin(endpoint));
                }
            }
        }

        private string GenerateRubyBaseController(Endpoint endpoint)
        {
            string className = GetRubyClassName(endpoint.Path);
            string route = endpoint.Path.Replace("{", ":").Replace("}", "");
    
            // Always enforce authentication
            string authCheck = "before do authenticate_request end";

            // Enforce role-based access if roles are defined
            if (endpoint.Auth?.Enforce == true && endpoint.Auth.Roles != null && endpoint.Auth.Roles.Count > 0)
            {
                string roles = string.Join("\", \"", endpoint.Auth.Roles);
                authCheck += $"\nbefore do authorize_roles([\"{roles}\"]) end";
            }

            return $@"
require 'sinatra/base'
require_relative '../controllers/{className}'

class {className} < Sinatra::Base
  {authCheck}

  get '{route}' do
    {className}Implementation.new.handle_request(params)
  end
end";
        }

        private string GenerateRubyMixin(Endpoint endpoint)
        {
            string className = GetRubyClassName(endpoint.Path);

            return $@"
module {className}Implementation
  def handle_request(params)
    # TODO: Implement business logic here
    {{ message: 'Replace this with actual logic' }}.to_json
  end
end";
        }

        private void GenerateJavaScriptScaffolding(ApiDefinition apiDefinition)
        {
            string generatedDir = "src/generated";
            string controllerDir = "src/controllers";

            if (!Directory.Exists(generatedDir))
                Directory.CreateDirectory(generatedDir);
            if (!Directory.Exists(controllerDir))
                Directory.CreateDirectory(controllerDir);

            foreach (var endpoint in apiDefinition.Endpoints)
            {
                string className = GetJavaScriptClassName(endpoint.Path);
                string generatedFilePath = Path.Combine(generatedDir, $"{className}.js");
                string controllerFilePath = Path.Combine(controllerDir, $"{className}.js");

                Console.WriteLine($"Generating: {generatedFilePath}...");
                File.WriteAllText(generatedFilePath, GenerateJavaScriptBaseController(endpoint));

                if (!File.Exists(controllerFilePath))
                {
                    Console.WriteLine($"Creating developer decorator: {controllerFilePath}...");
                    File.WriteAllText(controllerFilePath, GenerateJavaScriptDecorator(endpoint));
                }
            }
        }

        private string GenerateJavaScriptBaseController(Endpoint endpoint)
        {
            string className = GetJavaScriptClassName(endpoint.Path);
            string route = endpoint.Path.Replace("{", ":");

            // Always enforce authentication middleware
            string authCheck = "authenticateMiddleware, ";

            // Enforce role-based access if roles are defined
            if (endpoint.Auth?.Enforce == true && endpoint.Auth.Roles != null && endpoint.Auth.Roles.Count > 0)
            {
                string roles = string.Join("\", \"", endpoint.Auth.Roles);
                authCheck += $"authorizeRoles([\"{roles}\"]), ";
            }

            return $@"
import express from 'express';
import {{ {className}Implementation }} from '../controllers/{className}.js';
import {{ authenticateMiddleware, authorizeRoles }} from '../middleware/auth.js';

const router = express.Router();

router.{endpoint.Method.ToLower()}('{route}', {authCheck} async (req, res) => {{
    const result = await new {className}Implementation().handleRequest(req.params);
    res.json(result);
}});

export default router;";
        }

        private string GenerateJavaScriptDecorator(Endpoint endpoint)
        {
            string className = GetJavaScriptClassName(endpoint.Path);

            return $@"
export class {className}Implementation {{
    async handleRequest(params) {{
        // TODO: Implement business logic here
        return {{ message: 'Replace this with actual logic' }};
    }}
}}";
        }

        private string GetJavaScriptClassName(string path)
        {
            return path
                .Trim('/')
                .Replace("{", "")
                .Replace("}", "")
                .Split('/')
                .Select(part => char.ToUpper(part[0]) + part.Substring(1))
                .Aggregate((a, b) => a + b) + "Controller";
        }

        private string GetRubyClassName(string path)
        {
            return path
                .Trim('/')
                .Replace("{", "")
                .Replace("}", "")
                .Split('/')
                .Select(part => char.ToUpper(part[0]) + part.Substring(1))
                .Aggregate((a, b) => a + b) + "Controller";
        }

        private string GetControllerName(string path)
        {
            return path.Replace("/", "").Replace("{", "").Replace("}", "") + "Controller";
        }

        private string GetMethodName(string method)
        {
            return method switch
            {
                "GET" => "Get",
                "POST" => "Create",
                "PUT" => "Update",
                "DELETE" => "Delete",
                _ => "HandleRequest"
            };
        }
    }
}