using Newtonsoft.Json;
using System.Text;

namespace FPVPulse.Ingest
{
    public class IngestClient
    {
        string injestId;
        string apiUrlRoot;
        string token;

        HttpClient httpClient = new HttpClient();

        string EventUrl;
        string RaceUrl;
        string PilotResultUrl;

        public IngestClient(string injestId, string apiUrlRoot, string token, HttpClient client = null)
        {
            this.injestId = injestId;
            this.apiUrlRoot = apiUrlRoot;
            this.token = token;
            if (client != null)
                httpClient = client;
            else
                client = new HttpClient();

            BuildUrls();
        }

        void BuildUrls()
        {
            EventUrl = $"{apiUrlRoot}/injest/event/";
            RaceUrl = $"{apiUrlRoot}/injest/race/";
            PilotResultUrl = $"{apiUrlRoot}/injest/race/";
        }

        public async Task PutEvent(InjestEvent injestEvent)
        {
            var url = string.Join(EventUrl, injestEvent.InjestEventId);
            var json = JsonConvert.SerializeObject(injestEvent);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task PutRace(InjestRace injestRace)
        {
            var url = string.Join(EventUrl, injestRace.InjestRaceId);
            var json = JsonConvert.SerializeObject(injestRace);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        public async Task PilotResult(InjestPilotResult injestPilotResult)
        {
            var url = string.Join(EventUrl, injestPilotResult.InjestRaceId, "/result/", injestPilotResult.InjestPilotId);
            var json = JsonConvert.SerializeObject(injestPilotResult);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await httpClient.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
        }
    }
}
