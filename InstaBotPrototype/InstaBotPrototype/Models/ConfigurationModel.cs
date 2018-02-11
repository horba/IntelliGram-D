using System.Collections.Generic;

namespace InstaBotPrototype.Models
{
    public class ConfigurationModel
    {
        public int? ConfigId { get; set; }
        public IEnumerable<TagModel> Tags { get; set; }
        public IEnumerable<TopicModel> Topics { get; set; }
    }
}