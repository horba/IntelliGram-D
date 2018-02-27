using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using static InstaBotPrototype.Services.AI.MicrosoftImageRecognizer;

namespace InstaBotPrototype.Services.AI
{
    public abstract class ImageRecognizer : IRecognizer
    {
        public Description RecognizeTopic(byte[] imageBytes) => RecognizeTopicAsync(imageBytes).Result;
        public Description RecognizeTopic(string imageFilePath) => RecognizeTopicAsync(GetImageAsByteArray(imageFilePath)).Result;
        public abstract Task<Description> RecognizeTopicAsync(byte[] imageBytes);
        public async Task<Description> RecognizeTopicAsync(string imageFilePath) => await RecognizeTopicAsync(GetImageAsByteArray(imageFilePath));

        static protected byte[] GetImageAsByteArray(string imageFilePath)
        {
            using (var fileStream = new FileStream(imageFilePath, FileMode.Open, FileAccess.Read))
            using (var binaryReader = new BinaryReader(fileStream))
            {
                return binaryReader.ReadBytes((int)fileStream.Length);
            }
        }
    }
}