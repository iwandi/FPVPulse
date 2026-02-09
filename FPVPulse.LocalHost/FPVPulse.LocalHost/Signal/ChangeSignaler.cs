using Microsoft.AspNetCore.SignalR;
using FPVPulse.LocalHost.Client;
using Microsoft.AspNetCore.Components;
using FPVPulse.Ingest;
using FPVPulse.LocalHost.Injest.Db;
using FPVPulse.LocalHost.Client.Components.Data;

namespace FPVPulse.LocalHost.Signal
{
    public class ChangeSignaler
    {
        public event EventHandler<ChangeEventArgs<object>>? OnChange;
        public event EventHandler<ChangeEventArgs<DbInjestEvent>>? OnInjestEventChanged;
        public event EventHandler<ChangeEventArgs<DbInjestRace>>? OnInjestRaceChanged;
		public event EventHandler<ChangeEventArgs<DbInjestRacePilot>>? OnInjestRacePilotChanged;
		public event EventHandler<ChangeEventArgs<DbInjestPilotResult>>? OnInjestPilotResultChanged;
		public event EventHandler<ChangeEventArgs<DbInjestLeaderboard>>? OnInjestLeaderboardChanged;
		public event EventHandler<ChangeEventArgs<DbInjestLeaderboardPilot>>? OnInjestLeaderboardPilotChanged;

		public event EventHandler<ChangeEventArgs<Event>>? OnEventChanged;
		public event EventHandler<ChangeEventArgs<EventShedule>>? OnEventSheduleChanged;
		public event EventHandler<ChangeEventArgs<Leaderboard>>? OnLeaderboardChanged;
		public event EventHandler<ChangeEventArgs<LeaderboardPilot>>? OnLeaderboardPilotChanged;
		public event EventHandler<ChangeEventArgs<Pilot>>? OnPilotChanged;
		public event EventHandler<ChangeEventArgs<Race>>? OnRaceChanged;
		public event EventHandler<ChangeEventArgs<RacePilot>>? OnRacePilotChanged;
		public event EventHandler<ChangeEventArgs<RacePilotResult>>? OnRacePilotResultChanged;

		readonly IHubContext<ChangeHub> changeHub;

        public ChangeSignaler(IHubContext<ChangeHub> changeHub)
        {
            this.changeHub = changeHub;

			BuildLookupTable();
		}

        public async Task SignalChangeAsync<T>(ChangeGroup group, int id, int parentId, T data)
        {
            var change = new ChangeEventArgs<T>(group, id, parentId, data);
            await SignalChangeAsync(change);
        }

		Dictionary<ChangeGroup, Handler> lookup = new Dictionary<ChangeGroup, Handler>();

		class Handler
		{
			public string Group;
			public string GroupData;
			public string DataMethod;
			public Action<object> OnChangeTypedHandler;
		}

		void BuildLookupTable()
		{
			//var ownFields = GetFields<ChangeGroup>(false);
			var messageFields = GetFields(typeof(ChangeSignalMessages), true);

			var changeHandler = new Dictionary<ChangeGroup, Action<object>>();

			changeHandler.Add(ChangeGroup.InjestEvent, (obj) => OnInjestEventChanged?.Invoke(this, (ChangeEventArgs<DbInjestEvent>)obj));
			changeHandler.Add(ChangeGroup.InjestRace, (obj) => OnInjestRaceChanged?.Invoke(this, (ChangeEventArgs<DbInjestRace>)obj));
			changeHandler.Add(ChangeGroup.InjestRacePilot, (obj) => OnInjestRacePilotChanged?.Invoke(this, (ChangeEventArgs<DbInjestRacePilot>)obj));
			changeHandler.Add(ChangeGroup.InjestPilotResult, (obj) => OnInjestPilotResultChanged?.Invoke(this, (ChangeEventArgs<DbInjestPilotResult>)obj));
			changeHandler.Add(ChangeGroup.InjestLeaderboard, (obj) => OnInjestLeaderboardChanged?.Invoke(this, (ChangeEventArgs<DbInjestLeaderboard>)obj));
			changeHandler.Add(ChangeGroup.InjestLeaderboardPilot, (obj) => OnInjestLeaderboardPilotChanged?.Invoke(this, (ChangeEventArgs<DbInjestLeaderboardPilot>)obj));

			changeHandler.Add(ChangeGroup.Event, (obj) => OnEventChanged?.Invoke(this, (ChangeEventArgs<Event>)obj));
			changeHandler.Add(ChangeGroup.EventShedule, (obj) => OnEventSheduleChanged?.Invoke(this, (ChangeEventArgs<EventShedule>)obj));
			changeHandler.Add(ChangeGroup.Leaderboard, (obj) => OnLeaderboardChanged?.Invoke(this, (ChangeEventArgs<Leaderboard>)obj));
			changeHandler.Add(ChangeGroup.LeaderboardPilot, (obj) => OnLeaderboardPilotChanged?.Invoke(this, (ChangeEventArgs<LeaderboardPilot>)obj));
			changeHandler.Add(ChangeGroup.Pilot, (obj) => OnPilotChanged?.Invoke(this, (ChangeEventArgs<Pilot>)obj));
			changeHandler.Add(ChangeGroup.Race, (obj) => OnRaceChanged?.Invoke(this, (ChangeEventArgs<Race>)obj));
			changeHandler.Add(ChangeGroup.RacePilot, (obj) => OnRacePilotChanged?.Invoke(this, (ChangeEventArgs<RacePilot>)obj));
			changeHandler.Add(ChangeGroup.RacePilotResult, (obj) => OnRacePilotResultChanged?.Invoke(this, (ChangeEventArgs<RacePilotResult>)obj));

			foreach (var changeGroup in Enum.GetValues<ChangeGroup>())
			{
				if(changeGroup == ChangeGroup.None)
					continue;

				bool isData = changeGroup.ToString().EndsWith("Data");
				if (isData)
					continue;

				var dataChangeGroup = Enum.Parse<ChangeGroup>(changeGroup.ToString() + "Data");

				// var eventFieldName = $"On{changeGroup}Changed";
				//if(!ownFields.TryGetValue(eventFieldName, out var eventField))
				//	throw new Exception($"Missing event field {eventFieldName} for change group {changeGroup}");

				var messageFieldName = $"Change{dataChangeGroup}";
				if (!messageFields.TryGetValue(messageFieldName, out var messageField))
					throw new Exception($"Missing message field {messageFieldName} for change group {changeGroup}");

				var onChangeTypedHandler = changeHandler[changeGroup];

				var handler = new Handler
				{
					Group = changeGroup.ToString(),
					GroupData = dataChangeGroup.ToString(),
					DataMethod = messageField.GetValue(null).ToString(),
					OnChangeTypedHandler = onChangeTypedHandler,
				};
				lookup.Add(changeGroup, handler);
				lookup.Add(dataChangeGroup, handler);
			}
		}

		Dictionary<string, System.Reflection.FieldInfo> GetFields(Type t, bool staticFields)
		{
			System.Reflection.FieldInfo[]? fields = null;
			if (staticFields)
				fields = t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
			else
				fields = t.GetFields(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);

			return fields.ToDictionary(f => f.Name, f => f);
		}

		Dictionary<string, System.Reflection.FieldInfo> GetFields<T>(bool staticFields)
		{
			var t = typeof(T);
			return GetFields(t, staticFields);
		}

		public async Task SignalChangeAsync<T>(ChangeEventArgs<T> change)
		{
			OnChange?.Invoke(this, new ChangeEventArgs<object>(
				change.Group,
				change.Id,
				change.ParentId,
				change.Data
			));

			if(lookup.TryGetValue(change.Group, out var handler))
			{
				var group = handler.Group;
				var groupData = handler.GroupData;
				var dataMethod = handler.DataMethod;

				handler.OnChangeTypedHandler(change);

				await changeHub.Clients.Group(group)
				.SendAsync(ChangeSignalMessages.Change, group, change.Id, change.ParentId);
				await changeHub.Clients.Group(groupData)
					.SendAsync(dataMethod, change.Id, change.ParentId, change.Data);
			}
		}
    }
}
