using System.ComponentModel.DataAnnotations;

namespace FPVPulse.LocalHost.Client.Components.Data
{
	public class Pilot
	{
		[Key]
		public int PilotId { get; set; }

		public string? InjestPilotId { get; set; }

		[MaxLength(30)]
		public string DisplayName { get; set; } = string.Empty;

		[MaxLength(30)]
		public string FirstName { get; set; } = string.Empty;
		[MaxLength(30)]
		public string LastName { get; set; } = string.Empty;
		[MaxLength(30)]
		public string NickName { get; set; } = string.Empty;

		[MaxLength(30)]
		public string Nationality { get; set; } = string.Empty;

		[MaxLength(2)]
		public string CountryAlpha2 { get; set; } = string.Empty;
		[MaxLength(3)]
		public string CountryAlpha3 { get; set; } = string.Empty;
		[MaxLength(3)]
		public string Language { get; set; } = string.Empty;
		[MaxLength(6)]
		public string Locale { get; set; } = string.Empty;
	}
}
