namespace VigilAgent.Api.Commons.AgentCardSpecs
{
    public class AgentCard
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Url { get; set; } = "";
        public string Version { get; set; } = "";
        public string IconUrl { get; set; } = "";
        public string DocumentationUrl { get; set; } = "";
        public Capability Capabilities { get; set; }
        public string[] DefaultInputModes { get; set; }
        public string[] DefaultOutputModes { get; set; }
        public Skill[] Skills { get; set; }
        public AgentProvider Provider { get; set; }
        public List<Dictionary<string, List<string>>> Security { get; set; }
        public Dictionary<string, SecurityScheme> SecuritySchemes { get; set; }
        public bool SuppostsAuthenticatedExtendedCard { get; set; }
    }
}
