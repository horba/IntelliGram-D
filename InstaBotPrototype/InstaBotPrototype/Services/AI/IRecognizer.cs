using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstaBotPrototype.Services.AI
{
    interface IRecognizer
    {
        IEnumerable<string> RecognizeTopic(byte[] imageBytes);
        IEnumerable<string> RecognizeTopic(string imageFilePath);
        Task<IEnumerable<string>> RecognizeTopicAsync(byte[] imageBytes);
        Task<IEnumerable<string>> RecognizeTopicAsync(string imageFilePath);
    }
}