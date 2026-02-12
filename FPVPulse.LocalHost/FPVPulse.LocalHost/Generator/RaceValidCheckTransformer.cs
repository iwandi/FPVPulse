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
			// TODO : Check detect if race is a valid race

			await Task.CompletedTask;
			return true;
		}
	}
}
