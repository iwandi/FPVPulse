using FPVPulse.Ingest;
using FPVPulse.LocalHost.Client;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Signal;
using System.Collections.Concurrent;

namespace FPVPulse.LocalHost.Generator
{
	public abstract class BaseTransformer<T> : BackgroundService
	{
		readonly IServiceProvider serviceProvider;
		protected readonly ChangeSignaler changeSignaler;

		ConcurrentQueue<ChangeEventArgs<T>> queue = new ConcurrentQueue<ChangeEventArgs<T>>();
		readonly SemaphoreSlim signal = new(0);

		public BaseTransformer(ChangeSignaler changeSignaler, IServiceProvider serviceProvider)
		{
			this.changeSignaler = changeSignaler;
			this.serviceProvider = serviceProvider;

			Bind(changeSignaler);
		}

		protected void OnChanged(object? sender, ChangeEventArgs<T> @event)
		{
			queue.Enqueue(@event);
			signal.Release();
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
						while(!await Process(db, @event.Data, @event.Id, @event.ParentId))
						{

						}
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

		public abstract void Bind(ChangeSignaler changeSignaler);

		protected abstract Task CheckExisting(EventDbContext db, InjestDbContext injestDb);
		protected abstract Task<bool> Process(EventDbContext db, T data, int id, int parentId);
	}
}
