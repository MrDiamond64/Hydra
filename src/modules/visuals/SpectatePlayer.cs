using InnerNet;
using UnityEngine;

namespace HydraMenu.modules.visuals
{
	internal class SpectatePlayer : Module
	{
		public SpectatePlayer() : base("SpectatePlayer") { }

		public PlayerControl target;
		private bool wereShadowsEnabled = false;

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

			wereShadowsEnabled = HudManager.Instance.ShadowQuad.gameObject.active;
			HudManager.Instance.ShadowQuad.gameObject.SetActive(false);

			EventCoordinator.OnPlayerDisconnect += OnPlayerDisconnect;
		}

		protected override void OnDisable()
		{
			if(PlayerControl.LocalPlayer != null)
			{
				FollowerCamera camera = Camera.main.GetComponent<FollowerCamera>();
				camera.SetTarget(PlayerControl.LocalPlayer);

				if(wereShadowsEnabled) HudManager.Instance.ShadowQuad.gameObject.SetActive(true);
			}

			EventCoordinator.OnPlayerDisconnect -= OnPlayerDisconnect;
		}
	}
}