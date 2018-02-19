using System;
using System.Threading;

namespace Worker
{
    class Program
    {
        static void Main(string[] args)
        {
            var bot = new TelegramBot();
            bot.Run();

            var period = 60_000;

            var creator = new MessageCreator();
            var sender = new MessageSender(bot);

            var timer = new Timer((object o) =>
            {
                creator.Start();
                sender.Start();
            }, null, 0, period);

            Console.ReadLine();
        }
    }
}