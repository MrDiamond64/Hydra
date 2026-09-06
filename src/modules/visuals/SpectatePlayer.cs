using InnerNet;
using UnityEngine;

namespace HydraMenu.modules.visuals
{
	internal class SpectatePlayer : Module
	{
		public SpectatePlayer() : base("SpectatePlayer") { }

		public PlayerControl target;

		private void OnPlayerDisconnect(ClientData client, DisconnectReasons reason)
		{
			if(client.Character != target) return;

			Hydra.notifications.Send("Spectate Player", "Spectate Player was disabled as the player you were spectating left the game.");
			Enabled = false;
		}

		protected override void OnEnable()
		{
			// In case this module was enabled as part of the config
			if(target == null)
			{
				_enabled = false;
				return;
			}

			FollowerCamera camera = Camera.main.GetComponent<FollowerCamera>();
			camera.SetTarget(target);

			HudManager.Instance.ShadowQuad.gameObject.SetActive(false);

			EventCoordinator.OnPlayerDisconnect += OnPlayerDisconnect;
		}

		protected override void OnDisable()
		{
			target = null;

			if(PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.Data != null)
			{
				FollowerCamera camera = Camera.main.GetComponent<FollowerCamera>();
				camera.SetTarget(PlayerControl.LocalPlayer);

				bool shouldBeEnabled = !ModuleManager.fullbright.Enabled && !RoleManager.IsGhostRole(PlayerControl.LocalPlayer.Data.RoleType);
				HudManager.Instance.ShadowQuad.gameObject.SetActive(shouldBeEnabled);
			}

			EventCoordinator.OnPlayerDisconnect -= OnPlayerDisconnect;
		}
	}
}