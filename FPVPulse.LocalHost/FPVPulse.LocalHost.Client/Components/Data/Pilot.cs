using System.ComponentModel.DataAnnotations;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class Pilot
	{
		[Key]
		public int PilotId;

		public int InjestPilotId;

		[MaxLength(30)]
		public string DisplayName = string.Empty;

		[MaxLength(30)]
		public string FirstName = string.Empty;
		[MaxLength(30)]
		public string LastName = string.Empty;
		[MaxLength(30)]
		public string NickName = string.Empty;

		[MaxLength(30)]
		public string Nationality = string.Empty;

		[MaxLength(2)]
		public string CountryAlpha2 = string.Empty;
		[MaxLength(3)]
		public string CountryAlpha3 = string.Empty;
		[MaxLength(3)]
		public string Language = string.Empty;
		[MaxLength(6)]
		public string Locale = string.Empty;
	}
}
