using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InstaBotPrototype.Services.AI
{
    public class GoogleImageRecognizer : ImageRecognizer
    {
        public override Task<IEnumerable<string>> RecognizeTopicAsync(byte[] imageBytes) => throw new NotImplementedException();
    }
}