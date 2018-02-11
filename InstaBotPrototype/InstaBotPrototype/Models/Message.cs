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
        public Message(long chatId, string message)
        {
            this.Id = 0;
            this.ChatId = chatId;
            this.Text = message;
        }
        public Message(int id, long chatId, string message)
        {
            this.Id = id;
            this.ChatId = chatId;
            this.Text = message;
        }
    }
}
