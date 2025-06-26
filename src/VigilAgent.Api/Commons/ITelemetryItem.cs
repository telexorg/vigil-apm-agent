namespace VigilAgent.Api.Commons
{
    public interface ITelemetryItem
    {
        string Id { get; set; }
        DateTime Timestamp { get; set; }
        string ProjectName { get; set; }
        string OrgId { get; set; }
    }

}
