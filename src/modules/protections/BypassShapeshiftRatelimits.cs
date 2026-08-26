using AmongUs.GameOptions;

namespace HydraMenu.modules.protections
{
	internal class BypassShapeshiftRatelimits : Module
	{
		// Shapeshifting and reverting shapeshifts have strict ratelimits for the host, which can impact the Mass Shapeshift feature in Host options
		// We can bypass these ratelimits by sending a game options update and setting the shapeshift cooldown to zero seconds
		public BypassShapeshiftRatelimits() : base("BypassShapeshiftRatelimits")
		{
			base.Enabled = true;
		}

		private void OnGameStart()
		{
			if(!AmongUsClient.Instance.AmHost) return;

			PlayerControl player = Utilities.GetRandomPlayer();
			if(player == null) return;

			IGameOptions options = GameOptions.CreateCloneOptions(GameManager.Instance.LogicOptions.currentGameOptions);
			options.SetFloat(FloatOptionNames.ShapeshifterCooldown, 0.0f);

			// Send the settings update to a random player, we don't want to mess up our saved lobby settings
			GameOptions.SendGameOptionsToClient(options, player.OwnerId);
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnGameStart += OnGameStart;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnGameStart -= OnGameStart;
		}
	}
}