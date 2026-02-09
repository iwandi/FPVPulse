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
		string LeaderboardUrl;

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
			LeaderboardUrl = $"{apiUrlRoot}/injest/leaderboard/";
		}

        HttpRequestMessage BuildRequest<T>(T data, string url) where T : class
        {
            var json = JsonConvert.SerializeObject(data, jsonSerializerSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var request = new HttpRequestMessage(HttpMethod.Put, url)
            {
                Content = content,
            };
            request.Headers.Authorization = tokenHeader;
            request.Headers.Add("Injest-ID", injestId);

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

            bool hasPilotId = !string.IsNullOrWhiteSpace(injestPilotResult.InjestPilotId);
            bool hasPilotEntryId = !string.IsNullOrWhiteSpace(injestPilotResult.InjestPilotEntryId);

			if (!hasPilotId && !hasPilotEntryId)
			{
				Console.WriteLine("Error no valid pilot ids");
			}

			var url = string.Concat(PilotResultUrl, injestPilotResult.InjestRaceId, "/result");
            var request = BuildRequest(injestPilotResult, url);
            var response = await httpClient.SendAsync(request);
            return response.EnsureSuccessStatusCode();
        }

		public async Task<HttpResponseMessage> PutLeaderboard(InjestLeaderboard leaderboard)
		{
			if (string.IsNullOrWhiteSpace(leaderboard.InjestEventId) ||
				(string.IsNullOrWhiteSpace(leaderboard.InjestLeaderboardId) || leaderboard.RaceType == RaceType.Unknown))
			{
				Console.WriteLine("Error no valid ids");
			}

			var url = string.Concat(LeaderboardUrl, leaderboard.InjestEventId);
			var request = BuildRequest(leaderboard, url);
			var response = await httpClient.SendAsync(request);
			return response.EnsureSuccessStatusCode();
		}
	}
}
