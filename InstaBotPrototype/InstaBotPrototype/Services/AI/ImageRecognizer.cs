using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace InstaBotPrototype.Services.AI
{
    public abstract class ImageRecognizer : IRecognizer
    {
        public IEnumerable<string> RecognizeTopic(byte[] imageBytes) => RecognizeTopicAsync(imageBytes).Result;
        public IEnumerable<string> RecognizeTopic(string imageFilePath) => RecognizeTopicAsync(GetImageAsByteArray(imageFilePath)).Result;
        public abstract Task<IEnumerable<string>> RecognizeTopicAsync(byte[] imageBytes);
        public async Task<IEnumerable<string>> RecognizeTopicAsync(string imageFilePath) => await RecognizeTopicAsync(GetImageAsByteArray(imageFilePath));

        static protected byte[] GetImageAsByteArray(string imageFilePath)
        {
            var fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read);
            var binaryReader = new BinaryReader(fileStream);
            return binaryReader.ReadBytes((int)fileStream.Length);
        }
    }
}