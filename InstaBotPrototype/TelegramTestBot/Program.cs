using System;
using System.Threading;

namespace TelegramTestBot
{
    class Program
    {
        static void Main()
        {
            Console.WriteLine("Bots/services will be launched soon");

            // Bots work here
            var telegram = new TelegramBot();
            var telegramThread = new Thread(new ThreadStart(telegram.Run));
            telegramThread.Start();

            Console.ReadLine();
        }
    }
}