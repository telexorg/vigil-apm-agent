namespace VigilAgent.Api.Dtos
{
    public class ProjectRegistrationRequest
    {
        public string ProjectName { get; set; }
        public string OrgId { get; set; }
        public string? Description { get; set; }
    }
}
