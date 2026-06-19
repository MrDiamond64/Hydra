using AmongUs.GameOptions;
using AmongUs.InnerNet.GameDataMessages;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Hazel;
using HydraMenu.features;
using InnerNet;
using System.Collections;
using UnityEngine;

namespace HydraMenu.ui.sections
{
    internal class TrollSection : ISection
    {
        public TrollSection() : base("Troll") { }

        RoleTypes selectedRole = RoleTypes.Phantom;

        public override void Render()
        {
            if (PlayerControl.LocalPlayer == null)
            {
                GUILayout.Label("You are not currently in a game, these options will not work.");
            }

            Troll.AutoReportBodies.Enabled = Controls.PlayerSpecificToggle("Auto Report Bodies", PlayerControl.LocalPlayer, ref Troll.AutoReportBodies.source);
            Hydra.routines.autoTriggerSpores.Enabled = GUILayout.Toggle(Hydra.routines.autoTriggerSpores.Enabled, "Auto Trigger Spores");
            Troll.BlockSabotages.Enabled = GUILayout.Toggle(Troll.BlockSabotages.Enabled, "Block Sabotages");
            Troll.BlockVenting.Enabled = GUILayout.Toggle(Troll.BlockVenting.Enabled, "Disable Vents");

            ForceHost.Enabled = GUILayout.Toggle(ForceHost.Enabled, "Force Host");

            if (GUILayout.Button("Copy Random Player"))
            {
                PlayerControl randomPl = Utilities.GetRandomPlayer();
                Utilities.CopyPlayer(randomPl);
            }

            if (GUILayout.Button("Trigger All Spores"))
            {
                if (Utilities.GetCurrentMap() != MapNames.Fungle)
                {
                    Hydra.notifications.Send("Trigger Spores", "This option only works on the Fungle map.");
                }
                else
                {
                    FungleShipStatus shipStatus = ShipStatus.Instance.Cast<FungleShipStatus>();

                    foreach (Mushroom mushroom in shipStatus.sporeMushrooms.Values)
                    {
                        PlayerControl.LocalPlayer.RpcTriggerSpores(mushroom);
                    }

                    Hydra.notifications.Send("Trigger Spores", "All spores have been triggered.", 5);
                }
            }

            GUILayout.Label($"Change role to: {selectedRole}");
            selectedRole = Controls.HorizontalRoleSlider(selectedRole);

            if (GUILayout.Button("Change My Role"))
            {
                Network.BatchedMessage batch = new Network.BatchedMessage();
                batch.UseAnticheatBypass();
                batch.QueueSetRole(PlayerControl.LocalPlayer, selectedRole, true);
                batch.FinishBatch();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Change All Roles"))
            {
                ChangeAllRole(selectedRole, false);
            }

            if (GUILayout.Button("Change All Roles but Host"))
            {
                ChangeAllRole(selectedRole, true);
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(5);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Start Medbay Scan For All"))
            {
                ScanForAll(true);
            }

            if (GUILayout.Button("End Medbay Scan For All"))
            {
                ScanForAll(false);
            }
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("TP All to Host"))
            {
                TPTo(AmongUsClient.Instance.GetHost().Character);
            }
            if (GUILayout.Button("TP All to Me"))
            {
                TPTo(PlayerControl.LocalPlayer);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Randomize All Colors"))
            {
                RandomizeColors();
            }

            if (GUILayout.Button("Make Everyone Shapeshift"))
            {
                ShapeshiftAll();
            }

            GUILayout.Space(5);

            if (GUILayout.Button("Set Levels To 2^32"))
            {
                CrazyLevels();
            }

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Turn all to Me"))
            {
                TurnAllToHost1(PlayerControl.LocalPlayer);
                TurnAllToHost2(PlayerControl.LocalPlayer);
            }

            if (GUILayout.Button("Turn all to Host"))
            {
                TurnAllToHost1(AmongUsClient.Instance.GetHost().Character);
                TurnAllToHost2(AmongUsClient.Instance.GetHost().Character);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("Make Host Kill All"))
            {
                AmongUsClient.Instance.StartCoroutine(KillAllHost().WrapToIl2Cpp());
            }

            if (GUILayout.Button("Change Scene Tutorial"))
            {
                SceneChange("Tutorial");
            }

            if (GUILayout.Button("Make Everyone vote for Host"))
            {
                MakeAllVote(AmongUsClient.Instance.GetHost().Character);
            }

            if (GUILayout.Button("Crazy Votes"))
            {
                MakeAllVote(AmongUsClient.Instance.GetHost().Character, true);
            }

            if (GUILayout.Button("Funny Lobby Timer"))
            {
                FunnyLobbyTimer();
            }
            
            if (GUILayout.Button("Nuke Lobby"))
            {
                RenameAll("<color=yellow>Your lobby has been ㏊cked by Hydra Menu</color>\nGet the best ㏊cks for Among Us at:\ndiscord.gg/ADKj3GM2Wb\n\ngithub.com/MrDiamond64/Hydra");
                Nuker2();
            }

            if (GUILayout.Button("Memory Allocation Overload"))
            {
                Network.BatchedMessage batch = new Network.BatchedMessage();
                batch.UseAnticheatBypass();

                if (MeetingHud.Instance == null)
                {
                    MeetingHud.Instance = Object.Instantiate<MeetingHud>(HudManager.Instance.MeetingPrefab);

                    SpawnGameDataMessage spawn = AmongUsClient.Instance.CreateSpawnMessage(MeetingHud.Instance, -2, SpawnFlags.None);

                    spawn.Serialize(batch.writer);
                }

                for (byte i = 0; i < 50; i++)
                {
                    batch.writer.StartMessage((byte)GameDataTypes.RpcFlag);
                    batch.writer.WritePacked(MeetingHud.Instance.NetId);
                    batch.writer.Write((byte)RpcCalls.VotingComplete);
                    batch.writer.WritePacked(1 << 30);
                    batch.writer.EndMessage();
                }

                batch.FinishBatch();

                AmongUsClient.Instance.RemoveNetObject(MeetingHud.Instance);
                Object.Destroy(MeetingHud.Instance.gameObject);
            }

            if (GUILayout.Button("Fuck Start Timer"))
            {
                System.Random rnd = new System.Random();

                Network.BatchedMessage batch = new Network.BatchedMessage();
                batch.UseAnticheatBypass();
                batch.QueueSetStartCounter(PlayerControl.LocalPlayer, (sbyte)rnd.Next(-128, 127), ++PlayerControl.LocalPlayer.LastStartCounter);
                batch.FinishBatch();
            }

            if (GUILayout.Button("Disable New Lobby Creation"))
            {
                int index = GameManager.Instance.IsHideAndSeek() ? 5 : 4;

                IGameOptions options = GameManager.Instance.LogicOptions.currentGameOptions;
                options.SetByte(ByteOptionNames.MapId, 8);

                MessageWriter writer = MessageWriter.Get(SendOption.None);
                writer.StartMessage((byte)index);
                writer.WriteBytesAndSize(GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(options, AprilFoolsMode.IsAprilFoolsModeToggledOn));
                writer.EndMessage();

                Network.BatchedMessage batch = new Network.BatchedMessage(AmongUsClient.Instance.HostId);
                batch.UseAnticheatBypass();
                batch.QueueDataFlag(GameManager.Instance.NetId, writer);
                batch.FinishBatch();
            }

            GUILayout.Space(5);
            GUILayout.Label("Despawn Net Objects:");

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("LobbyBehavior"))
            {
                Despawn(LobbyBehaviour.Instance);
            }

            if (GUILayout.Button("ShipStatus"))
            {
                Despawn(ShipStatus.Instance);
            }
            GUILayout.EndHorizontal();

            if (GUILayout.Button("PlayerControl"))
            {
                Network.BatchedMessage batch = new Network.BatchedMessage();
                batch.UseAnticheatBypass();
                batch.QueueDespawn(PlayerControl.LocalPlayer.NetId);
                batch.FinishBatch();
            }

            GUILayout.Space(5);
            GUILayout.Label("Custom Chat:");
            Self.CustomChat.Enabled = GUILayout.Toggle(Self.CustomChat.Enabled, "Enabled");

            GUILayout.Label($"Size: {Self.CustomChat.size}");
            Self.CustomChat.size = (byte)GUILayout.HorizontalSlider(Self.CustomChat.size, 0, 10);

            GUILayout.Label($"Color: {Self.CustomChat.colors[Self.CustomChat.colorIndex]}");
            Self.CustomChat.colorIndex = (byte)GUILayout.HorizontalSlider(Self.CustomChat.colorIndex, 0, Self.CustomChat.colors.Count - 1);

            // Automatically close and open all doors at a set interval
            GUILayout.Label("Door Troller:");
            Hydra.routines.doorTroller.Enabled = GUILayout.Toggle(Hydra.routines.doorTroller.Enabled, "Enabled");

            GUILayout.Label($"Lock and Unlock Delay: {Hydra.routines.doorTroller.lockAndUnlockDelay:F2}s");
            Hydra.routines.doorTroller.lockAndUnlockDelay = GUILayout.HorizontalSlider(Hydra.routines.doorTroller.lockAndUnlockDelay, 0.1f, 2.0f);
        }

        private static void ScanForAll(bool scanning)
        {
            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                batch.QueueSetScanner(player, scanning, ++player.scannerCount);
            }

            batch.FinishBatch();
        }

        private static void RandomizeColors()
        {
            System.Random rnd = new System.Random();

            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                int color = rnd.Next(0, 17);

                batch.QueueSetColor(player, (byte)color);
            }

            batch.FinishBatch();
        }

        private static void ShapeshiftAll()
        {
            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                batch.QueueShapeshift(player, player, true);
            }

            batch.FinishBatch();
        }

        private static void TPTo(PlayerControl target)
        {
            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                ushort seqId = (ushort)(player.NetTransform.lastSequenceId + 2);

                batch.QueueSnapTo(player, seqId, target.transform.position);
            }

            batch.FinishBatch();
        }

        private static void FunnyLobbyTimer()
        {
            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();
            batch.QueueLobbyTimeExpiring(69420);
            batch.FinishBatch();
        }

        private static void RenameAll(string message)
        {
            Network.BatchedMessage batch = null;

            int currentCount = 0;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (currentCount == 0)
                {
                    batch = new Network.BatchedMessage();
                    batch.UseAnticheatBypass();
                }

                currentCount++;
                Hydra.Log.LogInfo($"Changing name of {player.Data.PlayerName}");
                batch.QueueSetName(player, message);

                if (currentCount == 5)
                {
                    Hydra.Log.LogInfo($"Finishing batch, ended up with {batch.writer.Length} bytes.");
                    batch.FinishBatch();
                    currentCount = 0;
                }
            }

            if (currentCount != 0)
            {
                Hydra.Log.LogInfo($"Finishing batch, ended up with {batch.writer.Length} bytes.");
                batch.FinishBatch();
            }
        }

        private static void Nuker2()
        {
            int index = GameManager.Instance.IsHideAndSeek() ? 5 : 4;

            IGameOptions options = GameManager.Instance.LogicOptions.currentGameOptions;
            options.SetInt(Int32OptionNames.DiscussionTime, 99999);
            options.SetByte(ByteOptionNames.MapId, 8);

            MessageWriter writer = MessageWriter.Get(SendOption.None);
            writer.StartMessage((byte)index);
            writer.WriteBytesAndSize(GameManager.Instance.LogicOptions.gameOptionsFactory.ToBytes(options, AprilFoolsMode.IsAprilFoolsModeToggledOn));
            writer.EndMessage();

            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();
            batch.QueueLobbyTimeExpiring(69420);

            byte i = 0;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                i++;

                // TP Away from Spawn
                Vector2 pos = new Vector2(2 * i, 2 * i);
                batch.QueueSnapTo(player, 32767, pos);
                batch.QueueSetScanner(player, true, 255);
                batch.QueueShapeshift(player, player, true);
            }

            batch.QueueDataFlag(GameManager.Instance.NetId, writer);
            batch.QueueSceneChange(AmongUsClient.Instance.ClientId, "Tutorial");
            batch.QueueSetStartCounter(PlayerControl.LocalPlayer, -69, ++PlayerControl.LocalPlayer.LastStartCounter);

            batch.FinishBatch();
        }

        private static void CrazyLevels()
        {
            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                batch.QueueSetLevel(player, uint.MaxValue - 1);
            }

            batch.FinishBatch();
        }

        private static void TurnAllToHost1(PlayerControl target)
        {
            NetworkedPlayerInfo.PlayerOutfit host = target.Data.DefaultOutfit;

            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                batch.QueueSetName(player, host.PlayerName);
                batch.QueueSetColor(player, (byte)host.ColorId);
                batch.QueueSetLevel(player, 465);
                batch.QueueSetHatStr(player, host.HatId, (byte)(player.Data.DefaultOutfit.HatSequenceId + 2));
            }

            batch.FinishBatch();
        }

        private static void TurnAllToHost2(PlayerControl target)
        {
            NetworkedPlayerInfo.PlayerOutfit host = target.Data.DefaultOutfit;

            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                batch.QueueSetNameplateStr(player, host.NamePlateId, (byte)(player.Data.DefaultOutfit.NamePlateSequenceId + 2));
                batch.QueueSetSkinStr(player, host.SkinId, (byte)(player.Data.DefaultOutfit.SkinSequenceId + 2));
                batch.QueueSetVisorStr(player, host.VisorId, (byte)(player.Data.DefaultOutfit.VisorSequenceId + 2));
                batch.QueueSetPetStr(player, host.PetId, (byte)(player.Data.DefaultOutfit.PetSequenceId + 2));
            }

            batch.FinishBatch();
        }

        public static void TurnAllTo(PlayerControl target)
        {
            NetworkedPlayerInfo.PlayerOutfit outfit = target.Data.DefaultOutfit;

            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();

            int currentCount = 0;
            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (currentCount == 0)
                {
                    batch = new Network.BatchedMessage();
                    batch.UseAnticheatBypass();
                }

                currentCount++;
                Hydra.Log.LogInfo($"Changing name of {player.Data.PlayerName}");

                batch.QueueSetName(player, outfit.PlayerName);
                batch.QueueSetColor(player, (byte)outfit.ColorId);
                batch.QueueSetLevel(player, target.Data.PlayerLevel);
                batch.QueueSetNameplateStr(player, outfit.NamePlateId, (byte)(player.Data.DefaultOutfit.NamePlateSequenceId + 2));
                batch.QueueSetHatStr(player, outfit.HatId, (byte)(player.Data.DefaultOutfit.HatSequenceId + 2));
                batch.QueueSetSkinStr(player, outfit.SkinId, (byte)(player.Data.DefaultOutfit.SkinSequenceId + 2));
                batch.QueueSetVisorStr(player, outfit.VisorId, (byte)(player.Data.DefaultOutfit.VisorSequenceId + 2));
                batch.QueueSetPetStr(player, outfit.PetId, (byte)(player.Data.DefaultOutfit.PetSequenceId + 2));

                if (currentCount == 4)
                {
                    Hydra.Log.LogInfo($"Finishing batch, ended up with {batch.writer.Length} bytes.");
                    batch.FinishBatch();
                    currentCount = 0;
                }
            }

            if (currentCount != 0)
            {
                Hydra.Log.LogInfo($"Finishing batch, ended up with {batch.writer.Length} bytes.");
                batch.FinishBatch();
            }

            batch.FinishBatch();
        }


        private static IEnumerator KillAllHost()
        {
            PlayerControl host = AmongUsClient.Instance.GetHost().Character;

            PlayerControl[] players = PlayerControl.AllPlayerControls.ToArray();

            int numOfImps = 0;
            foreach (PlayerControl player in players)
            {
                if (RoleManager.IsImpostorRole(player.Data.RoleType)) numOfImps++;
            }

            foreach (PlayerControl player in players)
            {
                if (!RoleManager.IsImpostorRole(player.Data.RoleType)) continue;

                numOfImps--;
                if (numOfImps == 0) break;

                Network.BatchedMessage batch = new Network.BatchedMessage();
                batch.UseAnticheatBypass();
                batch.QueueMurderPlayer(host, player, MurderResultFlags.Succeeded);
                batch.FinishBatch();
            }

            foreach (PlayerControl player in players)
            {
                if (player == host || RoleManager.IsImpostorRole(player.Data.RoleType)) continue;

                Network.BatchedMessage batch = new Network.BatchedMessage();
                batch.UseAnticheatBypass();
                batch.QueueMurderPlayer(host, player, MurderResultFlags.Succeeded);
                batch.FinishBatch();

                yield return Effects.Wait(0.3f);
            }

            yield break;
        }

        private static void SceneChange(string scene)
        {
            Network.BatchedMessage batch = new Network.BatchedMessage(AmongUsClient.Instance.HostId);
            batch.UseAnticheatBypass();
            batch.QueueSceneChange(AmongUsClient.Instance.HostId, scene);
            batch.FinishBatch();
        }

        private static void MakeAllVote(PlayerControl player, bool fuckTonVotes = false)
        {
            int voteCount = MeetingHud.Instance.playerStates.Length;
            if (fuckTonVotes) voteCount *= 3;

            MeetingHud.VoterState[] array = new MeetingHud.VoterState[voteCount];

            for (int i = 0; i < array.Length; i++)
            {
                MeetingHud.VoterState state = array[i];

                state.VoterId = (byte)(PlayerControl.AllPlayerControls.Count % 15);
                state.VotedForId = player.PlayerId;
            }

            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();
            batch.QueueVotingComplete(array, player.Data, false);
            batch.FinishBatch();
        }

        private static void ChangeAllRole(RoleTypes role, bool excludeHost)
        {
            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();

            foreach (PlayerControl player in PlayerControl.AllPlayerControls)
            {
                if (excludeHost && player.OwnerId == AmongUsClient.Instance.HostId)
                {
                    batch.QueueSetRole(player, RoleTypes.Crewmate, false);
                }
                else
                {
                    batch.QueueSetRole(player, role, false);
                }
            }

            batch.FinishBatch();
        }

        private static void Despawn(InnerNetObject netObject)
        {
            Network.BatchedMessage batch = new Network.BatchedMessage();
            batch.UseAnticheatBypass();
            batch.QueueDespawn(netObject.NetId);
            batch.FinishBatch();
        }
    }
}
