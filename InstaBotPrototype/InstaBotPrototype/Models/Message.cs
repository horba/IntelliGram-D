using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace InstaBotPrototype.Models
{
   public class Message
    {
        public int Id { get; private set; }
        public long ChatId { get; private set; }
        public string Text { get; private set; }
        public string PostId { get; private set; }
        public List<int> Tags { get; set; }
        public List<int> Topics { get; set; }
        public Message(long chatId, string message)
        {
            this.Id = 0;
            this.ChatId = chatId;
            this.Text = message;
            Tags = new List<int>();
            Topics = new List<int>();
        }
        public Message(int id, long chatId, string message)
        {
            this.Id = id;
            this.ChatId = chatId;
            this.Text = message;
            Tags = new List<int>();
            Topics = new List<int>();
        }

        public Message(long chatId, string message,string postId)
        {
            this.Id = 0;
            this.ChatId = chatId;
            this.Text = message;
            this.PostId = postId;
            Tags = new List<int>();
            Topics = new List<int>();
        }
    }
}
