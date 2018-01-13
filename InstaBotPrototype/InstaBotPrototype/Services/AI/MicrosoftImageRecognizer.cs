using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Configuration;
using System.Web.Script.Serialization;

namespace InstaBotPrototype.Services.AI
{
    public class MicrosoftImageRecognizer : ImageRecognizer
    {
        string subscriptionKey = WebConfigurationManager.OpenWebConfiguration(null).AppSettings.Settings["MicrosoftSubscriptionKey"].Value;
        string uriBase = WebConfigurationManager.OpenWebConfiguration(null).AppSettings.Settings["MicrosoftUriBase"].Value;

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

                var request = new JavaScriptSerializer().Deserialize<Request>(contentString);

                return request.description.tags;
            }
        }

        class Request
        {
            public Description description;
            public string requestId;
            public Metadata metadata;

            public class Description
            {
                public List<string> tags;
                public List<Caption> captions;

                public class Caption
                {
                    public string text;
                    public double confidence;
                }
            }

            public class Metadata
            {
                public int height;
                public int width;
                public string format;
            }
        }
    }
}