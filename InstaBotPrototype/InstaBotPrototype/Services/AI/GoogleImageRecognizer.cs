using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstaBotPrototype.Services.AI
{
    public class GoogleImageRecognizer : ImageRecognizer
    {
        public override IEnumerable<string> RecognizeTopic(byte[] imageBytes) => throw new NotImplementedException();
        public override IEnumerable<string> RecognizeTopic(string imageFilePath) => throw new NotImplementedException();
        public override Task<IEnumerable<string>> RecognizeTopicAsync(byte[] imageBytes) => throw new NotImplementedException();
        public override Task<IEnumerable<string>> RecognizeTopicAsync(string imageFilePath) => throw new NotImplementedException();
    }
}