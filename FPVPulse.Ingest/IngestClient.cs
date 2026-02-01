using Newtonsoft.Json;
using System.Text;

namespace FPVPulse.Ingest
{
    public class IngestClient
    {
        string injestId;
        string apiUrlRoot;
        string token;
        System.Net.Http.Headers.AuthenticationHeaderValue tokenHeader;

        HttpClient httpClient = new HttpClient();

        string EventUrl;
        string RaceUrl;
        string PilotResultUrl;

        JsonSerializerSettings jsonSerializerSettings;

        public IngestClient(string injestId, string apiUrlRoot, string token, HttpClient? client = null)
        {
            this.injestId = injestId;
            this.apiUrlRoot = apiUrlRoot;
            this.token = token;
            if (client != null)
                httpClient = client;
            else
                client = new HttpClient();

            tokenHeader = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            jsonSerializerSettings = new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            };

            BuildUrls();
        }

        void BuildUrls()
        {
            EventUrl = $"{apiUrlRoot}/injest/event/";
            RaceUrl = $"{apiUrlRoot}/injest/race/";
            PilotResultUrl = $"{apiUrlRoot}/injest/race/";
        }

        HttpRequestMessage BuildRequest<T>(T data, string url) where T : class
        {
            var json = JsonConvert.SerializeObject(data, jsonSerializerSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = content,
            };
            //request.Headers.Authorization = tokenHeader;
            //request.Headers.Add("Injest-ID", injestId);

            Console.WriteLine(url);
            Console.WriteLine(json);

            return request;
        }

        public async Task<HttpResponseMessage> PutEvent(InjestEvent injestEvent)
        {
            if (string.IsNullOrWhiteSpace(injestEvent.InjestEventId))
            {
                Console.WriteLine("Error no valid ids");
            }

            var url = string.Concat(EventUrl, injestEvent.InjestEventId);
            var request = BuildRequest(injestEvent, url);
            var response = await httpClient.SendAsync(request);
            return response.EnsureSuccessStatusCode();
        }

        public async Task<HttpResponseMessage> PutRace(InjestRace injestRace)
        {
            if(string.IsNullOrWhiteSpace(injestRace.InjestEventId) ||
                string.IsNullOrWhiteSpace(injestRace.InjestRaceId))
            {
                Console.WriteLine("Error no valid ids");
            }

            var url = string.Concat(RaceUrl, injestRace.InjestRaceId);
            var request = BuildRequest(injestRace, url);
            var response = await httpClient.SendAsync(request);
            return response.EnsureSuccessStatusCode();
        }

        public async Task<HttpResponseMessage> PilotResult(InjestPilotResult injestPilotResult)
        {
            if (string.IsNullOrWhiteSpace(injestPilotResult.InjestRaceId))
            {
                Console.WriteLine("Error no valid ids");
            }

            var url = string.Concat(PilotResultUrl, injestPilotResult.InjestRaceId, "/result/", injestPilotResult.InjestPilotId);
            var request = BuildRequest(injestPilotResult, url);
            var response = await httpClient.SendAsync(request);
            return response.EnsureSuccessStatusCode();
        }
    }
}
