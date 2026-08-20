using HarmonyLib;
using HydraMenu.features;

namespace HydraMenu.modules.troll
{
	internal class BlockSabotages : Module
	{
		// When the host receives a Sabotage system update, it first ensures that there is no active meeting, and that the sabotage cooldown has ended
		// If all checks pass, the host sets the sabotage cooldown to 30.0s and then handles which system to update based off of the sabotage type
		// The only problem is that the host updates the sabotage cooldown without first confirming that the attempted sabotage actually succeeded
		// Meaning that if we were to sabotage a system that does not have an associated sabotage, the host would just reset the sabotage cooldown
		// We can use flaw to create an anti-sabotage by sabotaging an invalid system every time the sabotage cooldown ends
		// which gives the impostors practically no time to be able to do any sabotages themselves
		public BlockSabotages() : base("BlockSabotages") { }

		public readonly float MINIMUM_TIMER_DURATION = 0.1f;
		// Can be any value that is not assigned to any SystemTypes
		public readonly byte INVALID_SYSTEM_TYPE = 255;

		public static BlockSabotages Instance
		{
			get { return ModuleManager.blockSabotages; }
		}

		[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.Deserialize))]
		public static class SabotageSerialize
		{
			static void Postfix(SabotageSystemType __instance)
			{
				if(!Instance.Enabled || __instance.Timer > Instance.MINIMUM_TIMER_DURATION) return;

				Hydra.Log.LogMessage($"Sabotage cooldown has depleted to {__instance.Timer}, sending Sabotage system update");
				ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, Instance.INVALID_SYSTEM_TYPE);
			}
		}

		protected override void OnEnable()
		{
			if(AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("Block Sabotages", "This option should be used when you are not the host of the lobby. Use Disable Sabotages in the Host section instead.");
				Host.DisableSabotages.Enabled = true;
				Enabled = false;
				return;
			}
		}
	}
}