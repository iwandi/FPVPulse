using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPVPulse.Ingest.RaceVision
{
    public class FPVPulseReportProcessor
    {
        RaceVisionClient client;

        public FPVPulseReportProcessor(RaceVisionClient client)
        {
            this.client = client;
        }

        public void ProcessLiveTimeMessage(string type, string json)
        {
            switch(type)
            {
                case "LiveStateResponse":
                    HandleLiveState(json);
                    break;
                case "LiveRaceTimeSyncResponse":
                    HandleLiveRaceTimeSync(json);
                    break;
                case "LiveRaceStateResponse":
                    HandleLiveRaceState(json);
                    break;
                case "LiveRaceEntryResponse":
                    HandleLiveRaceEntry(json);
                    break;
                case "LiveEstimatedPositionResponse":
                    HandleLiveEstimatedPosition(json);
                    break;
                case "RaceEntryByRaceResponse" :
                    HandleRaceEntryByRace(json);
                    break;
            }
        }

        void HandleLiveState(string json)
        {
            var jObject = JObject.Parse(json);

            var eventName = GetString(jObject["EventName"]);
            var LID = GetString(jObject["Event"]?["LID"]);
            var startTime = GetDateTime(jObject["Event"]?["StartDateTime"]);
            var endTime = GetDateTime(jObject["Event"]?["StartDateTime"]);

            Console.WriteLine($"Event {eventName} (LID:{LID}) from {startTime} to {endTime}");
        }

        void HandleLiveRaceTimeSync(string json)
        {
            var jObject = JObject.Parse(json);

            var isTimerRunning = GetBool(jObject["IsTimerRunning"]);
            var timeUntilStart = GetTimeSpan(jObject["TimeUntilStart"]);
            var timeElapsed = GetTimeSpan(jObject["TimeUntilStart"]);

            Console.WriteLine($"Timer running: {isTimerRunning}, Time until start: {timeUntilStart}, Time elapsed: {timeElapsed}");
        }

        void HandleLiveRaceState(string json)
        {
            var jObject = JObject.Parse(json);

            var roundLID = GetInt(jObject["RoundLID"]);
            var raceLID = GetInt(jObject["RaceLID"]);
            var roundType = GetInt(jObject["RoundType"]);
            var raceName = GetString(jObject["RaceName"]);
            var raceOrderNumber = GetInt(jObject["RaceOrderNumber"]);
            var raceClassInformation = GetString(jObject["RaceClassInformation"]);

            if (raceLID.HasValue && raceLID > 0)
            {
                Task.Run(async () =>
                {
                    await RequestRaceEntryByRace(raceLID.Value);
                    await Task.Delay(1000);
                    var nextRaceId = raceLID.Value + 1;
                    await RequestRaceEntryByRace(nextRaceId);
                });
            }
        }

        void HandleLiveRaceEntry(string json)
        {
            var jObject = JObject.Parse(json);

            var raceEntries = jObject["LiveRaceEntries"] as JArray;
            if (raceEntries != null)
            {
                foreach (var raceEntry in raceEntries)
                {
                    var raceEntryLID = GetInt(raceEntry["RaceEntryLID"]);
                    var position = GetInt(raceEntry["Position"]);
                    var frequencyName = GetString(raceEntry["FrequencyName"]);
                    var number = GetString(raceEntry["Number"]);
                    var driverName = GetString(raceEntry["DriverName"]);

                    var isRacingStarted = GetBool(raceEntry["IsRacingStarted"]);
                    var isDisqualified = GetBool(raceEntry["IsDisqualified"]);
                    var isDidNotStart = GetBool(raceEntry["IsDidNotStart"]);
                    var isFalseStart = GetBool(raceEntry["IsFalseStart"]);
                    var isDidNotFinish = GetBool(raceEntry["IsDidNotFinish"]);
                    var isComplete = GetBool(raceEntry["IsComplete"]);

                    var currentSectorNumber = GetInt(raceEntry["CurrentSectorNumber"]);
                    var currentSplitNumber = GetInt(raceEntry["CurrentSplitNumber"]);

                    var laps = GetInt(raceEntry["Laps"]);
                    var time = GetFloat(raceEntry["Time"]);
                    var fastestLap = GetFloat(raceEntry["FastestLap"]);
                    var top2Consecutive = GetFloat(raceEntry["Top2Consecutive"]);
                    var top3Consecutive = GetFloat(raceEntry["Top3Consecutive"]);

                    var liveEstimatedQualifyingPosition = GetInt(raceEntry["LiveEstimatedQualifyingPosition"]);
                    var startEstimatedQualifyingPosition = GetInt(raceEntry["StartEstimatedQualifyingPosition"]);

                    // TODO Unknown type
                    //var personalBest = raceEntry["PersonalBest"]?.ToObject<float>();

                    var isFastestLapInRace = GetBool(raceEntry["IsFastestLapInRace"]);
                    var isFastestHoleShotInRace = GetBool(raceEntry["IsFastestHoleShotInRace"]);

                    var liveRaceEntryLaps = raceEntry["LiveRaceEntryLaps"] as JArray;
                    if (liveRaceEntryLaps != null)
                    {
                        foreach (var lap in liveRaceEntryLaps)
                        {
                            var lapTimeSeconds = GetFloat(lap["LapTimeSeconds"]);
                            var isBadLap = GetBool(lap["IsBadLap"]);
                            var isFastestLap = GetBool(lap["IsFastestLap"]);
                        }
                    }
                }
            }
        }

        void HandleLiveEstimatedPosition(string json)
        {
            var jObject = JObject.Parse(json);

            var liveEstimatedPositions = jObject["LiveEstimatedPositions"] as JArray;
            if (liveEstimatedPositions != null)
            {
                foreach (var estimatedPosition in liveEstimatedPositions)
                {
                    var driverLID = GetInt(estimatedPosition["DriverLID"]);
                    var driverName = GetString(estimatedPosition["DriverName"]);
                    var position = GetInt(estimatedPosition["Position"]);
                    var positionChange = GetInt(estimatedPosition["PositionChange"]);
                    var bestSeedingResult = GetInt(estimatedPosition["BestSeedingResult"]);
                    var tieBreaker = GetFloat(estimatedPosition["TieBreaker"]);
                }
            }
        }

        void HandleRaceEntryByRace(string json)
        {
            var jObject = JObject.Parse(json);

            var race = jObject["Race"] as JObject;

            var raceLID = GetInt(race?["LID"]);
            var roundLID = GetInt(race?["RoundLID"]);

            var isComplete = GetBool(race?["IsComplete"]);
            var startMicrosecondsUTC = GetInt(race?["StartMicrosecondsUTC"]);
            var endMicrosecondsUTC = GetInt(race?["EndMicrosecondsUTC"]);
            var orderNumber = GetInt(race?["OrderNumber"]);
            var roundDisplay = GetString(race?["RoundDisplay"]);
            var raceInformation = GetString(race?["RaceInformation"]);

            var raceEntries = jObject["RaceEntries"] as JArray;
            if (raceEntries != null)
            {
                foreach (var raceEntry in raceEntries)
                {
                    var raceEntryLID = GetInt(raceEntry["LID"]);
                    var oderNumber = GetInt(raceEntry["OderNumber"]);
                    var number = GetInt(raceEntry["Number"]);
                    var driverLID = GetInt(raceEntry["DriverLID"]);
                    var driverName = GetString(raceEntry["DriverName"]);
                    var driverLastName = GetString(raceEntry["DriverLastName"]);
                    var frequencyName = GetString(raceEntry["FrequencyName"]);
                    var seedPosition = GetInt(raceEntry["SeedPosition"]);

                    var finalPositionOverall = GetInt(raceEntry["FinalPositionOverall"]);
                    //var startMicrosecondsUTC = raceEntry["StartMicrosecondsUTC"];

                    var completedLaps = GetInt(raceEntry["CompletedLaps"]);
                    var completedTime = GetFloat(raceEntry["CompletedTime"]);

                    var fastestLap = GetFloat(raceEntry["FastestLap"]);
                    var top2Consecutive = GetFloat(raceEntry["Top2Consecutive"]);
                    var top3Consecutive = GetFloat(raceEntry["Top3Consecutive"]);
                    var averageLap = GetFloat(raceEntry["AverageLap"]);
                }
            }
        }

        bool? GetBool(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;
            var tokenValue = token.ToString().ToLower();
            if (tokenValue == "true" || tokenValue == "1")
                return true;
            else if (tokenValue == "false" || tokenValue == "0")
                return false;
            return null;
        }   

        string? GetString(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;
            return token.ToString();
        }

        float? GetFloat(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            var tokenValue = token.ToString();

            if (float.TryParse(tokenValue, out var value))
                return value;

            return null;
        }

        int? GetInt(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;

            var tokenValue = token.ToString();

            if (int.TryParse(tokenValue, out var value))
                return value;

            return null;
        }

        DateTime? GetDateTime(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;
            var tokenValue = token.ToString();
            if (DateTime.TryParse(tokenValue, out var value))
                return value;
            return null;
        }

        TimeSpan? GetTimeSpan(JToken? token)
        {
            if (token == null || token.Type == JTokenType.Null)
                return null;
            var tokenValue = token.ToString();
            if (TimeSpan.TryParse(tokenValue, out var value))
                return value;
            return null;
        }

        public Task RequestRaceEntryByRace(int raceId)
        {
            client.TransmitPacket("RaceEntryByRaceRequest", $"{{ \"RaceLID\": \"{raceId}\" }}");

            return Task.CompletedTask;
        }
    }
}
