namespace ResponsiveBackendCLI.Models
{
    public class ApiDefinition
    {
        public string Title { get; set; }
        public string Version { get; set; }
        public List<Endpoint> Endpoints { get; set; }
    }

    public class Endpoint
    {
        public string Path { get; set; }
        public string Method { get; set; }
        public string Description { get; set; }
        public AuthDefinition Auth { get; set; }
        public Dictionary<int, ResponseDefinition> Response { get; set; }
    }

    public class AuthDefinition
    {
        public bool Enforce { get; set; }
        public List<string> Roles { get; set; }
    }

    public class ResponseDefinition
    {
        public Dictionary<string, string> Json { get; set; }
    }
}