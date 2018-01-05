using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstaBotPrototype.Services.AI
{
    public abstract class ImageRecognizer : IRecognizer
    {
        public abstract IEnumerable<string> RecognizeTopic(byte[] imageBytes);
        public abstract IEnumerable<string> RecognizeTopic(string imageFilePath);
        public abstract Task<IEnumerable<string>> RecognizeTopicAsync(byte[] imageBytes);
        public abstract Task<IEnumerable<string>> RecognizeTopicAsync(string imageFilePath);
    }
}