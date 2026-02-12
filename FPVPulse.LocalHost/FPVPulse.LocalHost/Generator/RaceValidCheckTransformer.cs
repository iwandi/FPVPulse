using FPVPulse.LocalHost.Client.Components.Data;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;

namespace FPVPulse.LocalHost.Generator
{
	public class RaceValidCheckTransformer : BaseTransformer<Race>
	{
		public RaceValidCheckTransformer(ChangeSignaler changeSignaler, IServiceProvider serviceProvider) : base(changeSignaler, serviceProvider)
		{

		}

		public override void Bind(ChangeSignaler changeSignaler)
		{
			changeSignaler.OnRaceChanged += OnChanged;
		}

		protected override async Task CheckExisting(EventDbContext db, InjestDbContext injestDb)
		{
			foreach (var race in db.Races)
			{
				while (!await Process(db, race, race.RaceId, race.EventId))
				{

				}
			}
		}

		protected override async Task<bool> Process(EventDbContext db, Race data, int id, int parentId)
		{
			bool invalid = false;

			if (data.RaceType == Ingest.RaceType.Unknown)
				invalid = true;
			else if(data.RaceType == Ingest.RaceType.Qualifying && (data.FirstOrderPoistion == 0 || data.SecondOrderPosition == 0))
				invalid = true;
			else if (data.RaceType == Ingest.RaceType.Practice && (data.FirstOrderPoistion == 0 || data.SecondOrderPosition == 0))
				invalid = true;
			else if (data.RaceType == Ingest.RaceType.Mains && (data.FirstOrderPoistion == 0 && data.SecondOrderPosition == 0))
				invalid = true;

			data.Invalid = invalid;

			var hasChanges = await db.SaveChangesAsync() > 0;


			// DANGER : this is a selve invokation.
			if (hasChanges)
				await changeSignaler.SignalChangeAsync(Client.ChangeGroup.Race, id, parentId, data);

			return true;
		}
	}
}
