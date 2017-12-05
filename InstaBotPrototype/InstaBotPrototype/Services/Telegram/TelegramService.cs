using InstaBotPrototype.Services.DB;
using System;
using System.IO;

namespace InstaBotPrototype.Services.Telegram
{
    public class TelegramService : ITelegramService
    {
        public int Connect(string username)
        {
            throw new NotImplementedException();
        }

        public void SendMessage(string message, string username)
        {
            if (username != null)
                File.AppendAllText("../../TelegramLog.txt", String.Format("[{0}] Receiver {1}: {2} {3}", DateTime.Now, username, message, Environment.NewLine));
            else
                throw new Exception("There is no user with this username");
        }
    }
}
