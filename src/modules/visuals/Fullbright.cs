namespace LunarMenu.modules.visuals
{
	internal class Fullbright : Module
	{
		public Fullbright() : base("Fullbright") { }

		private void OnGameLoad()
		{
			HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null)
			{
				HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
			}

			EventCoordinator.OnGameLoad += OnGameLoad;
		}

		protected override void OnDisable()
		{
			if(PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null)
			{
				bool shouldBeEnabled = !ModuleManager.spectatePlayer.Enabled && !RoleManager.IsGhostRole(PlayerControl.LocalPlayer.Data.RoleType);
				HudManager.Instance.ShadowQuad.gameObject.SetActive(shouldBeEnabled);
			}

			EventCoordinator.OnGameLoad -= OnGameLoad;
		}
	}
}