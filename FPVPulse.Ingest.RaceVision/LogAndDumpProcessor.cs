
using Newtonsoft.Json.Linq;

namespace FPVPulse.Ingest.RaceVision
{
    public static class LogAndDumpProcessor
    {
        public static void OnMessageReceived(string type, string json)
        {
            var jObject = JObject.Parse(json);
            var @event = jObject["Event"] as JObject;
            var race = jObject["Race"] as JObject;
            var raceEntries = jObject["LiveRaceEntries"] as JArray;
            if (jObject.TryGetValue("RaceLID", out var raceLID) &&
                raceLID.Type == JTokenType.Integer)
            {
                Console.WriteLine($"{type}:{raceLID}");
                File.WriteAllText($"{type}_{raceLID}.json", json);
            }
            // Event.LID
            else if (@event != null && @event["LID"] != null)
            {
                var eventLID = @event["LID"].Value<int>();
                Console.WriteLine($"{type}:{eventLID}");
                File.WriteAllText($"{type}_{eventLID}.json", json);
            }
            else if (race != null && race["LID"] != null)
            {
                raceLID = race["LID"].Value<int>();
                Console.WriteLine($"{type}:{raceLID}");
                File.WriteAllText($"{type}_{raceLID}.json", json);
            }
            else if (raceEntries != null)
            {
                foreach (JObject entry in raceEntries)
                {
                    if (entry.TryGetValue("RaceEntryLID", out var raceEntryLID) &&
                        raceEntryLID.Type == JTokenType.Integer)
                    {
                        Console.WriteLine($"{type}:{raceEntryLID}");
                        File.WriteAllText($"{type}_{raceEntryLID}.json", json);
                    }
                }
            }
            // LiveRaceEntries[..].RaceEntryLID
            else
            {
                Console.WriteLine($"{type}");
                File.WriteAllText($"{type}.json", json);
            }
        }
    }
}
