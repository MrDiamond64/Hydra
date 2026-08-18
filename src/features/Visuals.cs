using HarmonyLib;
using UnityEngine;

namespace HydraMenu.features
{
    internal class Visuals
    {
	    public static class SpectatePlayer
		{
			private static bool _enabled = false;
			private static bool wasShadowsEnabled = false;

			public static PlayerControl target;

			public static bool Enabled
			{
				get { return _enabled; }
				set
				{
					if(_enabled == value) return;
					_enabled = value;

					FollowerCamera camera = Camera.main.GetComponent<FollowerCamera>();

					if(value)
					{
						camera.SetTarget(target);
						wasShadowsEnabled = HudManager._instance.ShadowQuad.gameObject.active;
						HudManager.Instance.ShadowQuad.gameObject.SetActive(false);
					}
					else
					{
						camera.SetTarget(PlayerControl.LocalPlayer);

						if(wasShadowsEnabled) HudManager.Instance.ShadowQuad.gameObject.SetActive(true);
					}
				}
			}
		}
	}
}