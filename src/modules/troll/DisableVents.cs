namespace HydraMenu.modules.troll
{
	internal class DisableVents : Module
	{
		public DisableVents() : base("DisableVents") { }

		private void KickPlayerFromVent(PlayerControl player, byte ventId)
		{
			if(!Utilities.IsAnticheatPresent() || AmongUsClient.Instance.AmHost)
			{
				player.MyPhysics.RpcBootFromVent(ventId);
			}
			else
			{
				VentilationSystem.Update(VentilationSystem.Operation.BootImpostors, ventId);
			}
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnPlayerEnterVent += KickPlayerFromVent;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnPlayerEnterVent -= KickPlayerFromVent;
		}
	}
}