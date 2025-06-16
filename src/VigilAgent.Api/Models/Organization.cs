using VigilAgent.Api.IRepositories;

namespace VigilAgent.Api.Models
{
    public class Organization : IEntity
    {
        public string Id { get; set; } = null;

        public string Name { get; set; } = null;

        public string Overview { get; set; } = null;

        public string Industry { get; set; } = null;

        public string Website { get; set; } = null;

        public string Tone { get; set; } = null;
        public string TargetAudience { get; set; } = null;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; }
    }
}
