using HydraMenu.features;
using InnerNet;
using UnityEngine;

namespace HydraMenu.ui.sections
{
    internal class HostSection : ISection
    {
        public HostSection()
        {
            sectionName = "Host";
        }

        public override void Render()
        {
            if(PlayerControl.LocalPlayer == null)
            {
                GUILayout.Label("You are not currently in a game, these options will not work.");
            } else if(!AmongUsClient.Instance.AmHost)
            {
                GUILayout.Label("You are not the host of the current lobby. Using these options will either do nothing or get you banned by the anticheat");
            }

            Host.BanMidGame.Enabled = GUILayout.Toggle(Host.BanMidGame.Enabled, "Be able to ban players mid-game");

            Host.FlippedSkeld.Enabled = GUILayout.Toggle(Host.FlippedSkeld.Enabled, "Use Flipped Skeld Map");

            Host.DisableMeetings.Enabled = GUILayout.Toggle(Host.DisableMeetings.Enabled, "Disable Meetings");
			Host.DisableSabotages.Enabled = GUILayout.Toggle(Host.DisableSabotages.Enabled, "Disable Sabotages");
			Host.DisableGameEnd.Enabled = GUILayout.Toggle(Host.DisableGameEnd.Enabled, "Disable Game End");

			GUILayout.BeginHorizontal();
            Host.BlockLowLevels.Enabled = GUILayout.Toggle(Host.BlockLowLevels.Enabled, $"Kick players whose level is less than {Host.BlockLowLevels.MinLevel}");
            Host.BlockLowLevels.MinLevel = (uint)GUILayout.HorizontalSlider(Host.BlockLowLevels.MinLevel, 0, 100);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Force Crewmate Victory") && PlayerControl.LocalPlayer)
            {
				// Just incase the user has this enabled
				Host.DisableGameEnd.Enabled = false;

				GameManager.Instance.RpcEndGame(GameOverReason.CrewmatesByTask, false);
                Hydra.notifications.Send("Game Finished", "You ended the game with a crewmate victory.", 5);
            }

            if(GUILayout.Button("Force Imposter Victory") && PlayerControl.LocalPlayer)
            {
                // Just incase the user has this enabled
                Host.DisableGameEnd.Enabled = false;

                GameManager.Instance.RpcEndGame(GameOverReason.ImpostorsByKill, false);
                Hydra.notifications.Send("Game Finished", "You ended the game with an imposter victory.", 5);
            }
            GUILayout.EndHorizontal();

            if(GUILayout.Button("Kill Everyone") && PlayerControl.LocalPlayer)
            {
                foreach(PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    PlayerControl.LocalPlayer.RpcMurderPlayer(player, true);
                }
            }

            /*
            if(GUILayout.Button("Teleport Everyone to Me") && PlayerControl.LocalPlayer)
            {
                Vector2 pos = PlayerControl.LocalPlayer.transform.position;
                foreach(PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if(player.PlayerId == PlayerControl.LocalPlayer.NetId) continue;

                    Network.SendSnapTo(player.NetTransform, pos);
                }

                Hydra.notifications.Send("Teleporter", "Everyone has been teleported to you!", 5);
            }
            */

            GUILayout.Label("Map Spawner/Despawner:");
            GUILayout.BeginHorizontal();
            if(GUILayout.Button("Spawn Lobby"))
            {
                // From GameStartManager::Start
                LobbyBehaviour.Instance = Object.Instantiate<LobbyBehaviour>(GameStartManager.Instance.LobbyPrefab);
                AmongUsClient.Instance.Spawn(LobbyBehaviour.Instance, -2, SpawnFlags.None);

                Hydra.notifications.Send("Lobby Map", "A new instance of the lobby map has been spawned", 5);
            }

            if(GUILayout.Button("Despawn Lobby"))
            {
                if(LobbyBehaviour.Instance != null)
                {
                    LobbyBehaviour.Instance.Despawn();
                    Hydra.notifications.Send("Lobby Map", "The lobby map has been despawned.", 5);
                }
                else
                {
                    Hydra.notifications.Send("Lobby Map", "The lobby map has already been despawned.", 5);
                }
            }
            GUILayout.EndHorizontal();

            // AmongUsClient::CoStartGameHost has code for spawning in The Skeld, Mira, or Polus maps, but I'm not too sure how to get around replicating its behavior here
            if(GUILayout.Button("Despawn Map"))
            {
                if(ShipStatus.Instance != null)
                {
					ShipStatus.Instance.Despawn();
					Hydra.notifications.Send("Game Map", "The current map has been despawned.", 5);
				} else
                {
                    Hydra.notifications.Send("Game Map", "The game map has already been despawned.", 5);
                }
            }

			if(GUILayout.Button("Start"))
			{
				AmongUsClient.Instance.StartGame();
			}

            GUILayout.Label("Disco Party");
            Hydra.routines.discoHost.Enabled = GUILayout.Toggle(Hydra.routines.discoHost.Enabled, "Enabled");
            GUILayout.Label($"Color randomization delay: {Hydra.routines.discoHost.randomizationDelay:F2}");
            Hydra.routines.discoHost.randomizationDelay = GUILayout.HorizontalSlider(Hydra.routines.discoHost.randomizationDelay, 0.1f, 2.0f);
		}
	}
}