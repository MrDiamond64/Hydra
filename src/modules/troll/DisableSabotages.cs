using HarmonyLib;

namespace HydraMenu.modules.troll
{
	internal class DisableSabotages : Module
	{
		public DisableSabotages() : base("DisableSabotages") { }

		public readonly float MINIMUM_TIMER_DURATION = 0.1f;
		// Can be any value that is not assigned to any SystemTypes
		public readonly byte INVALID_SYSTEM_TYPE = 255;

		private static DisableSabotages Instance
		{
			get { return ModuleManager.disableSabotages; }
		}

		[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.UpdateSystem))]
		class BlockSabotagesHost
		{
			static bool Prefix()
			{
				return !Instance.Enabled;
			}
		}

		// When the host receives a Sabotage system update, it first ensures that there is no active meeting, and that the sabotage cooldown has ended
		// If all checks pass, the host sets the sabotage cooldown to 30.0s and then handles which system to update based off of the sabotage type
		// The only problem is that the host updates the sabotage cooldown without first confirming that the attempted sabotage actually succeeded
		// Meaning that if we were to sabotage a system that does not have an associated sabotage, the host would just reset the sabotage cooldown
		// We can use flaw to create an anti-sabotage by sabotaging an invalid system every time the sabotage cooldown ends
		// which gives the impostors practically no time to be able to do any sabotages themselves
		[HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.Deserialize))]
		class BlockSabotagesNonHost
		{
			static void Postfix(SabotageSystemType __instance)
			{
				if(!Instance.Enabled || AmongUsClient.Instance.AmHost || __instance.Timer > Instance.MINIMUM_TIMER_DURATION || MeetingHud.Instance) return;

				Hydra.Log.LogMessage($"Sabotage cooldown has depleted to {__instance.Timer}, sending Sabotage system update");
				ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, Instance.INVALID_SYSTEM_TYPE);
			}
		}

		private void OnMeetingEnd()
		{
			Hydra.Log.LogMessage($"Meeting has ended, sending Sabotage system update");
			ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, Instance.INVALID_SYSTEM_TYPE);
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnMeetingEnd += OnMeetingEnd;

			if(ShipStatus.Instance != null)
			{
				ISystemType system = ShipStatus.Instance.Systems[SystemTypes.Sabotage];
				SabotageSystemType sabotageSystem = system.Cast<SabotageSystemType>();

				if(sabotageSystem.Timer > MINIMUM_TIMER_DURATION)
				{
					ShipStatus.Instance.RpcUpdateSystem(SystemTypes.Sabotage, INVALID_SYSTEM_TYPE);
				}
			}
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnMeetingEnd -= OnMeetingEnd;
		}
	}
}