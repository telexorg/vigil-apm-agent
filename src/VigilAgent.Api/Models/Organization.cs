﻿using VigilAgent.Api.IRepositories;

namespace VigilAgent.Api.Models
{
    public class Organization : IEntity
    {
        public string Id { get; set; }
        public string OrgId { get; set; }
        public string ProjectName { get; set; }
        public string ApiKey { get; set; }
        public DateTime CreatedAt { get; set; }

    }
  
}
