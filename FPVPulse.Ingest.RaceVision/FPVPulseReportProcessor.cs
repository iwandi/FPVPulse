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
        IngestClient ingestClient;

        readonly object eventLock = new();
        string currentEventId = String.Empty;
        string currentEventName = String.Empty;
        RaceType currentRaceType = RaceType.Unknown;
		bool isValidEvent = false;

        readonly object raceLock = new();
        string currentRaceId = String.Empty;
        bool isValidRace = false;

        AutoScanState autoScan = new AutoScanState();

		public bool IsValidEvent => isValidEvent;
        public bool IsValidRace => isValidRace;

        public FPVPulseReportProcessor(RaceVisionClient client, IngestClient ingestClient)
        {
            this.client = client;
            this.ingestClient = ingestClient;
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

            if (LID == null)
            {
                lock (eventLock)
                {
                    isValidEvent = false;
                    currentEventId = String.Empty;
					currentEventName = String.Empty;
                    currentRaceType = RaceType.Unknown;
					autoScan.Reset();
				}
                return;
            }

            lock (eventLock)
            {
                if(currentEventId != LID)
					autoScan.Reset();

				currentEventId = LID;
                currentEventName = eventName ?? String.Empty;
				isValidEvent = true;
            }

            _ = Task.Run(async () => {
                try
                {
                    _ = await ingestClient.PutEvent(new InjestEvent
                    {
                        InjestEventId = LID,
                        InjestName = eventName,

                        StartDate = startTime,
                        EndDate = endTime,
                    });
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Exception while trying to ingest event {LID}: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            });
        }

        void HandleLiveRaceTimeSync(string json)
        {
            if (!IsValidEvent || !IsValidRace)
                return;

            var jObject = JObject.Parse(json);

            var isTimerRunning = GetBool(jObject["IsTimerRunning"]);
            var timeUntilStart = GetTimeSpan(jObject["TimeUntilStart"]);
            var timeElapsed = GetTimeSpan(jObject["TimeUntilStart"]);

            _ = Task.Run(async () => {
                try
                {
                    _ = await ingestClient.PutEvent(new InjestEvent
                    {
                        InjestEventId = currentEventId,
						CurrentRaceRunTimeSeconds = GetSeconds(timeElapsed),
						NextRaceSheduledStartTime = isTimerRunning.HasValue && isTimerRunning.Value ? DateTime.UtcNow + (timeUntilStart ?? TimeSpan.Zero) : null,
						NextRaceSheduledStartSeconds = GetSeconds(timeUntilStart),
					});
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while trying to ingest LiveRaceTime: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            });
        }

        RaceType RoundTypeToRaceType(int? value)
        {
            switch(value)
            {
                case 1:
                    return RaceType.Practice;
                case 2:
                    return RaceType.Qualifying;
                case 3:
                    return RaceType.Mains;
                default:
                    return RaceType.Unknown;
            }
        }

        RaceType RoundDisplayToRaceType(string? value)
        {
            if (value == null || value.Count() <= 0)
                return RaceType.Unknown;

            switch (value[0].ToString().ToUpper())
            {
                case "P":
                    return RaceType.Practice;
                case "Q":
                    return RaceType.Qualifying;
                case "M":
                    return RaceType.Mains;
                default:
                    return RaceType.Unknown;
            }
        }

        void HandleLiveRaceState(string json)
        {
            if (!IsValidEvent)
                return;

            // TODO make this also a async lock
            var eventId = currentEventId;

            var jObject = JObject.Parse(json);

            // TODO : Detect RaceLayout
            var roundLID = GetInt(jObject["RoundLID"]);
            var raceLID = GetInt(jObject["RaceLID"]);
            var roundType = GetInt(jObject["RoundType"]);
            //var raceName = GetString(jObject["RaceName"]);
            var raceOrderNumber = GetInt(jObject["RaceOrderNumber"]);
            var raceClassInformation = GetString(jObject["RaceClassInformation"]);
            //var roundLetterTypeOrderNumberDisplay = GetString(jObject["RoundLetterTypeOrderNumberDisplay"]);

            var raceType = RoundTypeToRaceType(roundType);
			currentRaceType = raceType;

			if (!raceLID.HasValue && raceLID <= 0)
            {
                lock (raceLock)
                {
                    isValidRace = false;
                    currentRaceId = String.Empty;
                }
                return;
            }
            lock (raceLock)
            {
                currentRaceId = raceLID.ToString()!;
                isValidRace = true;
            }

			_ = Task.Run(async () => {
                try
                {
                    _ = await ingestClient.PutRace(new InjestRace
                    {
                        InjestEventId = eventId,
                        InjestRaceId = raceLID.ToString(),
                        InjestName = raceClassInformation,

                        RaceType = raceType,
                        FirstOrderPoistion = raceOrderNumber,
                    });

                    var nextRaceId = raceLID.Value + 1;

                    _ = await ingestClient.PutEvent(new InjestEvent
                    {
                        InjestEventId = eventId,

                        CurrentInjestRaceId = raceLID.ToString(),
                        NextInjestRaceId = nextRaceId.ToString(),
                    });

					autoScan.MarkRaceId(raceLID.Value, true, true);
					if (autoScan.TryGetNextScanId(out var nextScanId))
					{
						_ = RequestRaceEntryByRace(nextScanId);
					}
					// TODO Rate Limit requests
					/*await RequestRaceEntryByRace(raceLID.Value);
                    await Task.Delay(1000);
                    await RequestRaceEntryByRace(nextRaceId);*/
				}
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while trying to ingest HandleLiveRaceState: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            });
        }

        void HandleLiveRaceEntry(string json)
        {
            if (!IsValidEvent || !IsValidRace)
                return;

            // TODO make this also a async lock
            var eventId = currentEventId;
            var raceId = currentRaceId;

            var jObject = JObject.Parse(json);

            var pilots = new List<InjestRacePilot>();
            var results = new List<InjestPilotResult>();

			var raceEntries = jObject["LiveRaceEntries"] as JArray;
            if (raceEntries != null)
            {
                foreach (var raceEntry in raceEntries)
                {
                    var raceEntryLID = GetInt(raceEntry["RaceEntryLID"]);
                    var position = GetInt(raceEntry["Position"]);
                    var frequencyName = GetString(raceEntry["FrequencyName"]);
                    //var number = GetString(raceEntry["Number"]);
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
                    var sortAverageLap = GetFloat(raceEntry["SortAverageLap"]);

                    var liveEstimatedQualifyingPosition = GetInt(raceEntry["LiveEstimatedQualifyingPosition"]);
                    var startEstimatedQualifyingPosition = GetInt(raceEntry["StartEstimatedQualifyingPosition"]);

                    // TODO Unknown type
                    //var personalBest = raceEntry["PersonalBest"]?.ToObject<float>();

                    var isFastestLapInRace = GetBool(raceEntry["IsFastestLapInRace"]);
                    var isFastestHoleShotInRace = GetBool(raceEntry["IsFastestHoleShotInRace"]);

                    var lapsData = new List<InjestLap>();
                    var liveRaceEntryLaps = raceEntry["LiveRaceEntryLaps"] as JArray;
                    if (liveRaceEntryLaps != null)
                    {
                        int i = 0;
                        foreach (var lap in liveRaceEntryLaps)
                        {
                            var lapTimeSeconds = GetFloat(lap["LapTimeSeconds"]);
                            var isBadLap = GetBool(lap["IsBadLap"]);
                            var isFastestLap = GetBool(lap["IsFastestLap"]);

                            var injestLap = new InjestLap
                            {
                                LapNumber = i,
                                LapTime = lapTimeSeconds,
                                IsInvalid = isBadLap,                                
                            };
							lapsData.Add(injestLap);

							i++;
                        }
                    }

                    var pilot = new InjestRacePilot
                    {
						InjestPilotEntryId = raceEntryLID.ToString(),

                        InjestName = driverName,
                        Channel = frequencyName,

                        Position = position,
                    };
                    pilots.Add(pilot);

                    if (!isRacingStarted.HasValue || !isRacingStarted.Value)
                        continue;

                    ResultFlag resultFlag = ResultFlag.None;
                    if(isDisqualified.HasValue && isDisqualified.Value)
                        resultFlag = ResultFlag.Disqualified;
                    else if (isDidNotStart.HasValue && isDidNotStart.Value)
                        resultFlag = ResultFlag.DidNotStart;
                    else if (isFalseStart.HasValue && isFalseStart.Value)
                        resultFlag = ResultFlag.FalseStart;
                    else if (isDidNotFinish.HasValue && isDidNotFinish.Value)
                        resultFlag = ResultFlag.DidNotFinish;


                    var pilotResult = new InjestPilotResult
                    {
                        InjestRaceId = raceId,
						InjestPilotEntryId = raceEntryLID.ToString(),

						CurrentSector = currentSectorNumber,
                        CurrentSplit = currentSplitNumber,

                        LapCount = laps,
                        TotalTime = time,
                        TopLapTime = fastestLap,
                        Top2ConsecutiveLapTime = top2Consecutive,
                        Top3ConsecutiveLapTime = top3Consecutive,

                        AverageLapTime = sortAverageLap,

                        Flags = resultFlag,

                        Laps = lapsData.ToArray(),
                    };
                    results.Add(pilotResult);

                    if (!isComplete.HasValue || !isComplete.Value)
                        continue;

                    pilotResult.Position = position;
                    pilotResult.IsComplited = true;

                }
            }

            _ = Task.Run(async () => {
                try
                {
                    _ = await ingestClient.PutRace(new InjestRace
                    {
                        InjestEventId = eventId,
                        InjestRaceId = raceId,

                        Pilots = pilots.ToArray(),
                    });
                    foreach(var pilotResult in results)
                    {
                        _ = await ingestClient.PilotResult(pilotResult);
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while trying to ingest HandleLiveRaceState: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            });
        }

        void HandleLiveEstimatedPosition(string json)
        {
            if (!IsValidEvent)
                return; 
            
            var eventId = currentEventId;
            var raceType = currentRaceType;

			var jObject = JObject.Parse(json);

			var pilots = new List<InjestLeaderboardPilot>();

			var liveEstimatedPositions = jObject["LiveEstimatedPositions"] as JArray;
            if (liveEstimatedPositions != null)
            {
                foreach (var estimatedPosition in liveEstimatedPositions)
                {
                    var driverLID = GetInt(estimatedPosition["DriverLID"]);
                    var driverName = GetString(estimatedPosition["DriverName"]);
                    var position = GetInt(estimatedPosition["Position"]);
                    var positionChange = GetInt(estimatedPosition["PositionChange"]);
                    var bestSeedingResult = GetString(estimatedPosition["BestSeedingResult"]);
                    var tieBreaker = GetFloat(estimatedPosition["TieBreaker"]);

                    if (driverLID == null)
                        continue;

					pilots.Add(new InjestLeaderboardPilot
					{
						InjestPilotId = driverLID.ToString(),

						InjestName = driverName,

						Position = position,
                        PositionDelta = positionChange,

                        PositionReason = bestSeedingResult,
					});
				}
            }

			_ = Task.Run(async () => {
				try
				{
					_ = await ingestClient.PutLeaderboard(new InjestLeaderboard
					{
						InjestEventId = eventId,
						RaceType = raceType,

                        Results = pilots.ToArray(),
					});
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Exception while trying to ingest HandleLiveEstimatedPosition: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
				}
			});
		}

        void HandleRaceEntryByRace(string json)
        {
            if (!IsValidEvent)
                return;

            // TODO make this also a async lock
            var eventId = currentEventId;

            var jObject = JObject.Parse(json);

            var race = jObject["Race"] as JObject;
            if (race == null)
            {
                // Asume this is the auto scan result that is invalid
                if (autoScan.IsScanActive)
                {
                    autoScan.MarkRaceId(autoScan.CurrentScanId, false, false);
                    if (autoScan.TryGetNextScanId(out var nextScanId))
					{
						_ = RequestRaceEntryByRace(nextScanId);
					}
				}
				return;
            }

            var raceLID = GetInt(race?["LID"]);
            //var roundLID = GetInt(race?["RoundLID"]);

            //var isComplete = GetBool(race?["IsComplete"]);
            //var startMicrosecondsUTC = GetInt(race?["StartMicrosecondsUTC"]);
            //var endMicrosecondsUTC = GetInt(race?["EndMicrosecondsUTC"]);
            var orderNumber = GetInt(race?["OrderNumber"]);
            var roundDisplay = GetString(race?["RoundDisplay"]);
            var raceInformation = GetString(race?["RaceInformation"]);

            var raceEntries = jObject["RaceEntries"] as JArray;
            var pilots = new List<InjestRacePilot>();
            //var result = new List<InjestPilotResult>();

            bool isValidForCurrentEvent = false;

			if (raceEntries != null)
            {
                foreach (var raceEntry in raceEntries)
                {
                    var raceEntryLID = GetInt(raceEntry["LID"]);
                    //var entryOrderNumber = GetInt(raceEntry["OrderNumber"]);
                    var number = GetInt(raceEntry["Number"]);
                    var driverLID = GetInt(raceEntry["DriverLID"]);
                    var driverName = GetString(raceEntry["DriverName"]);
                    //var driverLastName = GetString(raceEntry["DriverLastName"]);
                    var frequencyName = GetString(raceEntry["FrequencyName"]);
                    var seedPosition = GetInt(raceEntry["SeedPosition"]);
                    var eventName = GetString(raceEntry["EventName"]);

                    if(!isValidForCurrentEvent)
					    isValidForCurrentEvent = eventName == currentEventName;

					//var finalPositionOverall = GetInt(raceEntry["FinalPositionOverall"]);
					//var entryStartMicrosecondsUTC = GetInt(raceEntry["StartMicrosecondsUTC"]);

					//var completedLaps = GetInt(raceEntry["CompletedLaps"]);
					//var completedTime = GetFloat(raceEntry["CompletedTime"]);

					//var fastestLap = GetFloat(raceEntry["FastestLap"]);
					//var top2Consecutive = GetFloat(raceEntry["Top2Consecutive"]);
					//var top3Consecutive = GetFloat(raceEntry["Top3Consecutive"]);
					//var averageLap = GetFloat(raceEntry["AverageLap"]);

					if (driverLID == null || driverLID <= 0)
                        continue;

                    pilots.Add(new InjestRacePilot
                    {
                        InjestPilotId = driverLID.ToString(),
						InjestPilotEntryId = raceEntryLID.ToString(),

						InjestName = driverName,

                        SeedPosition = seedPosition,
                        StartPosition = number,

                        Channel = frequencyName,
                    });

                    // TODO option to ingest pilot results
                }
            }

            if(!isValidForCurrentEvent)
            {
				autoScan.MarkRaceId(raceLID.Value, false, false);
				if (autoScan.TryGetNextScanId(out var nextScanId))
				{
					_ = RequestRaceEntryByRace(nextScanId);
				}
				return;
            }

			_ = Task.Run(async () => {
                try
                {
                    _ = await ingestClient.PutRace(new InjestRace
                    {
                        InjestEventId = eventId,
                        InjestRaceId = raceLID.ToString(),
                        InjestName = raceInformation,

                        RaceType = RoundDisplayToRaceType(roundDisplay),
                        FirstOrderPoistion = orderNumber,

                        Pilots = pilots.ToArray(),
                    });

                    // Do not mark as valid as we do not know
					autoScan.MarkRaceId(raceLID.Value, true, false);
					if (autoScan.TryGetNextScanId(out var nextScanId))
					{
						_ = RequestRaceEntryByRace(nextScanId);
					}
				}
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception while trying to ingest HandleLiveRaceState: {ex.Message}{Environment.NewLine}{ex.StackTrace}");
                }
            });
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

        float? GetSeconds(TimeSpan? timeSpan)
		{
			if (!timeSpan.HasValue)
				return null;
			return Convert.ToSingle(timeSpan.Value.TotalSeconds);
		}

		public Task RequestRaceEntryByRace(int raceId)
        {
            Task.Delay(100).Wait();

			Console.WriteLine($"Reuest Race {raceId}");
            client.TransmitPacket("RaceEntryByRaceRequest", $"{{ \"RaceLID\": \"{raceId}\" }}");

            return Task.CompletedTask;
        }
    }
}
