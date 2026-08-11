using Hazel;
using HydraMenu.features;
using HydraMenu.network;
using HarmonyLib;
using UnityEngine;

namespace HydraMenu.ui.sections
{
    internal class TrollSection : ISection
    {
        public TrollSection() : base("Troll") { }

        public int selectedVent = 0;
        public System.Random rnd = new System.Random();

        private static float spammerTimer = 0f;
        private static float spammerActionCycleTimer = 0f;
        private static bool spammerIsPaused = false;
        private static float spammerPauseTimer = 0f;

        private const float SPAM_RATE = 0.25f;
        private const float ACTIVE_DURATION = 2.0f;
        private const float PAUSE_DURATION = 1.0f;

        public static bool TaskAndHnSSpammerEnabled { get; set; } = false;

        public static void UpdateSpammer()
        {
            if (!TaskAndHnSSpammerEnabled) return;

            float deltaTime = Time.deltaTime;

            if (spammerIsPaused)
            {
                spammerPauseTimer += deltaTime;
                if (spammerPauseTimer >= PAUSE_DURATION)
                {
                    spammerIsPaused = false;
                    spammerPauseTimer = 0f;
                    spammerActionCycleTimer = 0f;
                }
                return;
            }

            spammerActionCycleTimer += deltaTime;
            if (spammerActionCycleTimer >= ACTIVE_DURATION)
            {
                spammerIsPaused = true;
                spammerPauseTimer = 0f;
                return;
            }

            spammerTimer += deltaTime;
            if (spammerTimer >= SPAM_RATE)
            {
                spammerTimer = 0f;
                if (PlayerControl.LocalPlayer != null && PlayerControl.LocalPlayer.myTasks != null)
                {
                    var tasks = PlayerControl.LocalPlayer.myTasks;
                    for (int i = 0; i < tasks.Count; i++)
                    {
                        try
                        {
                            PlayerControl.LocalPlayer.RpcCompleteTask(tasks[i].Id);
                        }
                        catch { }
                    }
                }
            }
        }

        [HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
        public static class TaskAndHnSSpammerPatch
        {
            static void Postfix()
            {
                UpdateSpammer();
            }
        }

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

            TaskAndHnSSpammerEnabled = GUILayout.Toggle(TaskAndHnSSpammerEnabled, "Deplete HnS timer");

            if (GUILayout.Button("Kick All Players"))
            {
                Hydra.Log.LogInfo($"Sending Enter ventilation system update to all players");

                MessageWriter writer = MessageWriter.Get(SendOption.Reliable);
                writer.Write((ushort)0);
                writer.Write((byte)VentilationSystem.Operation.Enter);
                writer.Write((byte)0);

                BatchedMessage batch = new BatchedMessage();
                batch.QueueUpdateSystem(PlayerControl.LocalPlayer, SystemTypes.Ventilation, writer);
                batch.FinishBatch();

                writer.Recycle();

                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player == PlayerControl.LocalPlayer || player.OwnerId == AmongUsClient.Instance.HostId) continue;

                    Utilities.KickPlayer(player, true);
                }
            }

            Hydra.routines.autoKickAll.Enabled = GUILayout.Toggle(Hydra.routines.autoKickAll.Enabled, "Auto Kick All");

            GUILayout.Label($"Auto Kick Delay: {Hydra.routines.autoKickAll.delay:F1}s");
            Hydra.routines.autoKickAll.delay = GUILayout.HorizontalSlider(Hydra.routines.autoKickAll.delay, 1.0f, 60.0f);

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

            GUILayout.Space(5);
            GUILayout.Label($"Vent TP:");
            Hydra.routines.teleportSpammer.Enabled = GUILayout.Toggle(Hydra.routines.teleportSpammer.Enabled, "Teleport Flooder");

            GUILayout.Label($"Teleport everyone to vent: {selectedVent}");
            selectedVent = (int)GUILayout.HorizontalSlider(selectedVent, 0, ShipStatus.Instance != null ? ShipStatus.Instance.AllVents.Count - 1 : 10);

            if (GUILayout.Button("Teleport to Vent"))
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    Teleporter.TeleportToVent(player, selectedVent);
                }
            }

            if (GUILayout.Button("Teleport to Random Vent"))
            {
                foreach (PlayerControl player in PlayerControl.AllPlayerControls)
                {
                    if (player == PlayerControl.LocalPlayer) continue;

                    int ventId = rnd.Next(0, ShipStatus.Instance.AllVents.Count);

                    Teleporter.TeleportToVent(player, ventId);
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
