using VigilAgent.Api;

namespace BloggerAgent.Domain.Commons
{
    public class CollectionType
    {
        public const string User = "Users";
        public const string Convesation = "Conversations";
        public const string Organization = "Organization";
        public const string Project = "Projects";
        public const string Message = "Messages";
        public const string Error = "Errors";
        public const string Metric = "Metrics";
        public const string Trace = "Traces";

        public static string GetCollectionName(Type type)
        {
            return type.Name switch
            {
                nameof(Organization) => CollectionType.Organization,
                nameof(Message) => CollectionType.Message,
                nameof(Trace) => CollectionType.Trace,
                nameof(Error) => CollectionType.Error,
                nameof(Metric) => CollectionType.Metric,
                nameof(Project) => CollectionType.Project,
                _ => null!
            };
        }
    }

    }
