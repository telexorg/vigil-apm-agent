namespace VigilAgent.Api.Commons
{
    public class Setting
    {
        public string Label { get; set; }
        public string Type { get; set; }
        public bool? Required { get; set; }
        public object Default { get; set; }
    }
}
