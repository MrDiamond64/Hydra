namespace HydraMenu.anticheat
{
	/// <summary>
	/// Common contract shared by every anticheat check. Each check can be toggled on or off individually,
	/// which is surfaced to the user in the Anticheat section of the menu.
	/// </summary>
	internal interface ICheck
	{
		/// <summary>When false, the check is skipped entirely and never validates or flags anything.</summary>
		public bool Enabled { get; set; }
	}
}
