using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;
using Microsoft.AspNetCore.Components.Forms;
using System.Collections.Concurrent;

namespace FPVPulse.LocalHost.Generator
{
	public abstract class BaseTransformer<T> : BackgroundService
	{
		readonly IServiceProvider serviceProvider;
		protected readonly ChangeSignaler changeSignaler;

		ConcurrentQueue<ChangeEventArgs<T>> queue = new ConcurrentQueue<ChangeEventArgs<T>>();
		readonly SemaphoreSlim signal = new(0);
		readonly object lockObj = new object();
		Dictionary<int, SemaphoreSlim> parentLocks = new Dictionary<int, SemaphoreSlim>();

		public BaseTransformer(ChangeSignaler changeSignaler, IServiceProvider serviceProvider)
		{
			this.changeSignaler = changeSignaler;
			this.serviceProvider = serviceProvider;

			Bind(changeSignaler);
		}

		protected void OnChanged(object? sender, ChangeEventArgs<T> @event)
		{
			lock (lockObj)
			{
				queue.Enqueue(@event);
				signal.Release();
			}
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			using var scope = serviceProvider.CreateScope();
			var db = scope.ServiceProvider.GetRequiredService<EventDbContext>();

			{
				using var init = serviceProvider.CreateScope();
				var injest = init.ServiceProvider.GetRequiredService<InjestDbContext>();

				await CheckExisting(db, injest);
			}

			while (!stoppingToken.IsCancellationRequested)
			{
#if !DEBUG
				try
#endif
				{
					if (queue.TryDequeue(out var @event))
					{
						await ProcessUntilDone(db, @event.Data, @event.Id, @event.ParentId);
					}

					await signal.WaitAsync(stoppingToken);
				}
#if !DEBUG
				catch (Exception ex)
				{
					Console.WriteLine($"Error in Transformer: {ex}");
				}				
#endif
			}
		}

		protected async Task ProcessUntilDone(EventDbContext db, T data, int id, int parentId)
		{
			while (!await Process(db, data, id, parentId))
			{

			}

			SignalParentDone(parentId);
		}

		public abstract void Bind(ChangeSignaler changeSignaler);

		protected abstract Task CheckExisting(EventDbContext db, InjestDbContext injestDb);		
		protected abstract Task<bool> Process(EventDbContext db, T data, int id, int parentId);

		public void SignalParentDone(int parentId)
		{
			if (parentLocks.Count <= 0)
				return;

			lock (lockObj)
			{
				if (parentLocks.TryGetValue(parentId, out var parentLock))
				{
					var parentCleard = !queue.Any(e => e.ParentId == parentId);
					if (parentCleard)
					{
						parentLock.Release();
						parentLocks.Remove(parentId);
					}
				}
			}
		}

		public Task WaitForAllParentsDone(int parentId)
		{
			SemaphoreSlim parentLock = null;
			lock (lockObj)
			{
				var parentCleard = !queue.Any(e => e.ParentId == parentId);
				if (parentCleard)
					return Task.CompletedTask;
				if (!parentLocks.TryGetValue(parentId, out parentLock))
				{
					parentLock = new SemaphoreSlim(1);
					parentLocks.Add(parentId, parentLock);
				}
			}
			return parentLock.WaitAsync();
		}
	}
}
