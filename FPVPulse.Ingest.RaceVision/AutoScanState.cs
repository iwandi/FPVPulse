using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FPVPulse.Ingest.RaceVision
{
	public class AutoScanState
	{
		int maxOverScanCount = 16;

		int minCheckedId = 0;
		int maxCheckedId = 0;

		int minValidRaceId = 0;
		int maxValidRaceId = 0;

		int leadaheadScanIndex = 0;
		int currentRaceId = 0;

		List<int> validRaceIds = new List<int>();

		int lastScanId = 0;
		int currentScanId = 0;
		public int CurrentScanId => currentScanId;
		bool isScanActive = false;
		public bool IsScanActive => isScanActive;

		object sync = new object();

		public void Reset()
		{
			lock (sync)
			{
				minCheckedId = 0;
				maxCheckedId = 0;
				minValidRaceId = 0;
				maxValidRaceId = 0;
				leadaheadScanIndex = 0;
				currentRaceId = 0;

				lastScanId = 0;
				currentScanId = 0;
				isScanActive = false;

				validRaceIds.Clear();
			}
		}

		public void MarkRaceId(int id, bool isValid, bool isCurrentRace)
		{
			lock (sync)
			{
				if (minCheckedId == 0 || minCheckedId > id)
					minCheckedId = id;
				if (maxCheckedId == 0 || maxCheckedId < id)
					maxCheckedId = id;

				if (isValid)
				{
					if (minValidRaceId == 0 || minValidRaceId > id)
						minValidRaceId = id;
					if (maxValidRaceId == 0 || maxValidRaceId < id)
						maxValidRaceId = id;

					if(!validRaceIds.Contains(id))
						validRaceIds.Add(id);

					if (isCurrentRace)
						currentRaceId = id;
				}

				if (currentScanId == 0 || (currentScanId != 0 && id == currentScanId))
				{
					GenNextScanId();
				}
			}
		}

		void GenNextScanId()
		{
			var nextScanId = 0;

			// Scan outwards from current checked range
			for (var i = 1; i < maxOverScanCount; i++)
			{
				int leadId = maxValidRaceId + i;
				if (!IdAlreadyChecked(leadId))
				{
					Console.WriteLine($"Select maxValidRaceId:{maxValidRaceId} +{i}");
					nextScanId = leadId;
					break;
				}
				int trailId = minValidRaceId - i;
				if (!IdAlreadyChecked(trailId))
				{
					Console.WriteLine($"Select minValidRaceId:{minValidRaceId} -{i}");
					nextScanId = trailId;
					break;
				}
			}
			if (nextScanId == 0)
			{
				// Continures re scan of ldead
				var nextLeadScanIndex = (leadaheadScanIndex + 1) % maxOverScanCount;
				// Use the 0 index to check if there are any missing ids in the valid range
				if (nextLeadScanIndex == 0)
				{
					if(TryGetMissinRaceId(out var missingId))
					{
						Console.WriteLine($"Select missingId:{missingId}");
						nextScanId = missingId;
						leadaheadScanIndex = nextLeadScanIndex;
					}
					else
					{
						leadaheadScanIndex = 1;
					}
				}
				if (nextScanId == 0)
				{
					Console.WriteLine($"Select currentRaceId:{currentRaceId} +{nextLeadScanIndex}");
					nextScanId = currentRaceId + nextLeadScanIndex;
					leadaheadScanIndex = nextLeadScanIndex;
				}
			}

			if (nextScanId == 0)
				nextScanId = 0;

			lastScanId = currentScanId;
			currentScanId = nextScanId;
			isScanActive = false;
		}

		bool IdAlreadyChecked(int id)
		{
			if (id <= 0)
				return true;

			return !(id < minCheckedId || id > maxCheckedId);
		}

		bool TryGetMissinRaceId(out int id)
		{
			var sortedValidIds = validRaceIds.OrderBy(x => x).ToList();
			for (var i = 1; i < sortedValidIds.Count; i++)
			{
				var expectedId = sortedValidIds[i - 1] + 1;
				if (sortedValidIds[i] != expectedId)
				{
					id = expectedId;
					return true;
				}
			}
			id = 0;
			return false;
		}

		public bool TryGetNextScanId(out int raceId)
		{
			lock (sync)
			{
				raceId = currentScanId;
				if (isScanActive || currentScanId == 0)
					return false;
				isScanActive = true;
				return true;
			}
		}
	}
}
