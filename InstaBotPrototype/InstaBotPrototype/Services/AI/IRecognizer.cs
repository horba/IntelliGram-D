using System.Collections.Generic;
using System.Threading.Tasks;
using static InstaBotPrototype.Services.AI.MicrosoftImageRecognizer;

namespace InstaBotPrototype.Services.AI
{
    public interface IRecognizer
    {
        Description RecognizeTopic(byte[] imageBytes);
        Description RecognizeTopic(string imageFilePath);
        Task<Description> RecognizeTopicAsync(byte[] imageBytes);
        Task<Description> RecognizeTopicAsync(string imageFilePath);
    }
}