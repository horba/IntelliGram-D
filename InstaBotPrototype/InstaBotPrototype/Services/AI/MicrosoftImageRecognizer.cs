using Newtonsoft.Json;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Configuration;

namespace InstaBotPrototype.Services.AI
{
    public class MicrosoftImageRecognizer : ImageRecognizer
    {
       
        string subscriptionKey = "9130b53bc6c846979760aed320a58e6b";
        string uriBase = "https://westcentralus.api.cognitive.microsoft.com/vision/v1.0/analyze";

        public override async Task<IEnumerable<string>> RecognizeTopicAsync(byte[] imageBytes)
        {
            
            var client = new HttpClient();

            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", subscriptionKey);

            var requestParameters = "visualFeatures=Description&language=en";

            var uri = uriBase + "?" + requestParameters;

            HttpResponseMessage response;

            using (var content = new ByteArrayContent(imageBytes))
            {
                content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

                response = await client.PostAsync(uri, content);

                var contentString = await response.Content.ReadAsStringAsync();

                var request = JsonConvert.DeserializeObject<Request>(contentString);

                return request.Description.Tags;
            }
        }

        class Request
        {
            [JsonProperty("description")]
            public Description Description { get; set; }
            [JsonProperty("requestId")]
            public string RequestId { get; set; }
            [JsonProperty("metadata")]
            public Metadata Metadata { get; set; }
        }

        public class Description
        {
            [JsonProperty("tags")]
            public List<string> Tags { get; set; }
            [JsonProperty("captions")]
            public List<Caption> Captions { get; set; }

        }
        public class Caption
        {
            [JsonProperty("text")]
            public string Text { get; set; }
            [JsonProperty("confidence")]
            public double Confidence { get; set; }
        }

        public class Metadata
        {
            [JsonProperty("height")]
            public int Height { get; set; }
            [JsonProperty("width")]
            public int Width { get; set; }
            [JsonProperty("format")]
            public string Format { get; set; }
        }
    }
}