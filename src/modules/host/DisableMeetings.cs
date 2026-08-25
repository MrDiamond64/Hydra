using HarmonyLib;

namespace HydraMenu.modules.host
{
	internal class DisableMeetings : Module
	{
		public DisableMeetings() : base("DisableMeetings") { }

		private static DisableMeetings Instance
		{
			get { return ModuleManager.disableMeetings; }
		}

		// When a player reports a body, their client sends a ReportDeadBody RPC to the host. The host then should validate the RPC and start a meeting
		// To block meetings, we can simply ignore any received ReportDeadBody RPCs
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.ReportDeadBody))]
		class OnReportBody
		{
			static bool Prefix()
			{
				return !Instance.Enabled;
			}
		}
	}
}