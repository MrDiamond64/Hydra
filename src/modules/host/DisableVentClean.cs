using HarmonyLib;

namespace LunarMenu.modules.host
{
	internal class DisableVentClean : Module
	{
		public DisableVentClean() : base("DisableVentClean") { }

		private static DisableVentClean Instance
		{
			get { return ModuleManager.disableVentClean; }
		}

		[HarmonyPatch(typeof(VentilationSystem), nameof(VentilationSystem.PerformVentOp))]
		class OnVentUpdate
		{
			static bool Prefix(VentilationSystem.Operation op)
			{
				if(!Instance.Enabled) return true;

				return op != VentilationSystem.Operation.StartCleaning && op != VentilationSystem.Operation.StopCleaning && op != VentilationSystem.Operation.BootImpostors;
			}
		}

		protected override void OnEnable()
		{
			if(ShipStatus.Instance == null || !AmongUsClient.Instance.AmHost) return;

			ISystemType systemType = ShipStatus.Instance.Systems[SystemTypes.Ventilation];
			if(systemType == null) return;

			VentilationSystem ventilationSystem = systemType.Cast<VentilationSystem>();

			if(ventilationSystem.PlayersCleaningVents.Count != 0)
			{
				ventilationSystem.PlayersCleaningVents.Clear();
				ventilationSystem.IsDirty = true;
			}
		}
	}
}