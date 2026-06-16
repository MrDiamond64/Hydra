using HydraMenu.features;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal class TrollSection : ISection
	{
		public TrollSection() : base("Troll") { }

		public override void Render()
		{
			if(PlayerControl.LocalPlayer == null)
			{
				GUILayout.Label("You are not currently in a game, these options will not work.");
			}

			Troll.AutoReportBodies.Enabled = Controls.PlayerSpecificToggle("Auto Report Bodies", PlayerControl.LocalPlayer, ref Troll.AutoReportBodies.source);
			Hydra.routines.autoTriggerSpores.Enabled = GUILayout.Toggle(Hydra.routines.autoTriggerSpores.Enabled, "Auto Trigger Spores");
			Troll.BlockSabotages.Enabled = GUILayout.Toggle(Troll.BlockSabotages.Enabled, "Block Sabotages");
			Troll.BlockVenting.Enabled = GUILayout.Toggle(Troll.BlockVenting.Enabled, "Disable Vents");

			if(GUILayout.Button("Copy Random Player"))
			{
				PlayerControl randomPl = Utilities.GetRandomPlayer();
				Utilities.CopyPlayer(randomPl);
			}

			if(GUILayout.Button("Trigger All Spores"))
			{
				if(Utilities.GetCurrentMap() != MapNames.Fungle)
				{
					Hydra.notifications.Send("Trigger Spores", "This option only works on the Fungle map.");
				}
				else
				{
					FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();

					foreach(Mushroom mushroom in shipStatus.sporeMushrooms.Values)
					{
						PlayerControl.LocalPlayer.RpcTriggerSpores(mushroom);
					}

					Hydra.notifications.Send("Trigger Spores", "All spores have been triggered.", 5);
				}
			}

			GUILayout.Space(5);

			// Automatically close and open all doors at a set interval
			GUILayout.Label("Door Troller:");
			Hydra.routines.doorTroller.Enabled = GUILayout.Toggle(Hydra.routines.doorTroller.Enabled, "Enabled");

			GUILayout.Label($"Lock and Unlock Delay: {Hydra.routines.doorTroller.lockAndUnlockDelay:F2}s");
			Hydra.routines.doorTroller.lockAndUnlockDelay = GUILayout.HorizontalSlider(Hydra.routines.doorTroller.lockAndUnlockDelay, 0.1f, 2.0f);
		}
	}
}