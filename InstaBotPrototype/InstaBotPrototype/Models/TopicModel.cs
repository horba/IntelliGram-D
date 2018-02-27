using System.Collections.Generic;

namespace InstaBotPrototype.Models
{
    public class TopicModel : IComparer<TopicModel>
    {
        public TopicModel(int id, string topic) {
            TopicId = id;
            Topic = topic;
        }
        public int? TopicId { get; set; }
        public string Topic { get; set; }
        public int Compare(TopicModel x, TopicModel y)
        {
            return x.Topic.CompareTo(y.Topic);
        }
    }
}