using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using System.Collections.Concurrent;

namespace FPVPulse.LocalHost.Generator
{
	public class EventDataGenerator : BackgroundService
	{
		class WorkItem
		{
			public ChangeGroup Group { get; set; }
			public int InjestId { get; set; }
			public int InjestParentId { get; set; }
		}

		readonly IServiceProvider serviceProvider;
		readonly ChangeSignaler changeSignaler;

		ConcurrentQueue<WorkItem> queue = new ConcurrentQueue<WorkItem>();
		readonly SemaphoreSlim signal = new(0);

		public EventDataGenerator(ChangeSignaler changeSignaler, IServiceProvider serviceProvider)
		{
			this.changeSignaler = changeSignaler;
			this.serviceProvider = serviceProvider;
			Bind(changeSignaler);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();
			var injestDb = scope.ServiceProvider.GetRequiredService<InjestDbContext>();

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					if (queue.TryDequeue(out var workItem))
					{
						await Process(db, injestDb, workItem);
					}

					await signal.WaitAsync(stoppingToken);
				}
				catch (Exception ex)
				{
					Console.WriteLine($"Error in Transformer: {ex}");
				}
			}
		}

		public void Bind(ChangeSignaler changeSignaler)
		{
			changeSignaler.OnChange += OnChanged;
		}

		protected void OnChanged(object? sender, ChangeEventArgs<object> change)
		{
			foreach(var workItem in queue)
			{
				if (workItem.Group == change.Group && workItem.InjestId == change.Id && workItem.InjestParentId == change.ParentId)
					return;
			}

			var newWorkItem = new WorkItem
			{
				Group = change.Group,
				InjestId = change.Id,
				InjestParentId = change.ParentId
			};
			
			queue.Enqueue(newWorkItem);
			signal.Release();
		}

		Task Process(EventDbContext db, InjestDbContext injestDb, WorkItem workItem)
		{
			switch (workItem.Group)
			{
				case ChangeGroup.InjestEvent:
				case ChangeGroup.InjestEventData:
					return ProcessEvent(db, injestDb, workItem.InjestId);
				default:
					Console.WriteLine($"Unknown change group: {workItem.Group}");
					break;
			}
			return Task.CompletedTask;
		}

		async Task ProcessEvent(EventDbContext db, InjestDbContext injestDb, int injestId)
		{
			var getInjestEventData = injestDb.Events.FindAsync(injestId);
			var getExistingEvent = db.Events.Where(e => e.InjestEventId == injestId).FirstOrDefaultAsync();

			await Task.WhenAll(getInjestEventData.AsTask(), getExistingEvent);

			var injestEvent = getInjestEventData.Result;
			var @event = getExistingEvent.Result;

			if(@event == null)
			{

			}
		}
	}
}
