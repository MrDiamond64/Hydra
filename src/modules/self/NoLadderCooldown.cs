namespace HydraMenu.modules.self
{
	internal class NoLadderCooldown : Module
	{
		public NoLadderCooldown() : base("NoLadderCooldown")
		{
			base.Enabled = true;
		}

		private void OnLadderUse(Ladder ladder)
		{
			ladder.CoolDown = 0.0f;
			ladder.Destination.CoolDown = 0.0f;
		}

		protected override void OnEnable()
		{
			EventCoordinator.OnUseLadder += OnLadderUse;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnUseLadder -= OnLadderUse;
		}
	}
}