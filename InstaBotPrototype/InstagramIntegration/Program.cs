using InstaBotPrototype.Services.Instagram;
using InstaBotPrototype.Services.AI;
using System;
using System.Net.Http;
using System.Linq;
namespace InstagramIntegration
{
    class Program
    {
        static void Main(string[] args)
        {
            IInstagramService instagramService = new InstagramService();
            IRecognizer recognizer = new MicrosoftImageRecognizer();
            var client = new HttpClient();
            String[] users = new String[3] { "intelli_test", "yakovets_vlad", "vlod_ahotnek" };
            foreach (var user in users)
            {
                Console.WriteLine(String.Format("User : {0}", user));
                int counter = 1;
                foreach (var url in instagramService.GetLatestPosts(user))
                {
                    byte[] response = client.GetByteArrayAsync(url).Result;
                    Console.WriteLine("Image # " + counter);
                    Console.WriteLine(String.Format("Url : {0}", url));
                    Console.Write("Topics : ");
                    foreach (var topic in recognizer.RecognizeTopic(response).Take(5))
                        Console.Write(topic + " ");
                    Console.WriteLine();
                    ++counter;
                }
            }
            Console.ReadKey();
        }
    }
}
