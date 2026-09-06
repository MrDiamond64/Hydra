using AmongUs.Data;
using AmongUs.GameOptions;
using HarmonyLib;
using InnerNet;
using UnityEngine;

namespace LunarMenu.modules.visuals
{
    internal class RevealRoles : Module
    {
        public RevealRoles() : base("RevealRoles") { }

        private static RevealRoles Instance
        {
            get { return ModuleManager.revealRoles; }
        }

        public bool ShowKillCD { get; set; } = false;
        public bool ShowPlayerInfo { get; set; } = false;

        [HarmonyPatch(typeof(ChatBubble), nameof(ChatBubble.SetName))]
        class ChatRoles
        {
            static void Prefix(ref string playerName, ref Color color)
            {
                if (Utilities.InGame() || Utilities.InLobby())
                {
                    foreach (var playerData in GameData.Instance.AllPlayers)
                    {
                        var outfit = Utilities.GetPlayerOutfit(playerData);
                        if (outfit == null) continue;
                        if (playerName == playerData.PlayerName)
                        {
                            var localData = PlayerControl.LocalPlayer.Data;
                            color = Instance.Enabled ? Utilities.GetRoleColor(playerData.Role) : (Utilities.IsImpostor(localData) && Utilities.IsImpostor(playerData) ? Palette.ImpostorRed : Palette.White);
                            if (Instance.Enabled && Utilities.InGame())
                                playerName = $"<size=50%>{Utilities.GetRoleName(playerData.Role)}</size> {playerName}";
                        }
                    }
                }
            }
        }

        private static Color GetKillCooldownColor(float killTimer)
        {
            if (killTimer < 2.0)
                return Palette.ImpostorRed;
            else if (killTimer < 5.0)
                return Palette.Orange;
            return Palette.White;
        }

        [HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
        class PlayerRoles
        {
            static void Prefix(PlayerControl __instance)
            {
                try
                {
                    if (Utilities.InGame() || Utilities.InLobby())
                    {
                        if (!__instance || !PlayerControl.LocalPlayer) return;
                        var playerData = __instance.Data;
                        var localData = PlayerControl.LocalPlayer.Data;
                        if (!playerData || !localData) return;

                        if (__instance.cosmetics == null) return;

                        var nameText = __instance.cosmetics.nameText;

                        var outfit = Utilities.GetPlayerOutfit(playerData, true);
                        var client = AmongUsClient.Instance.GetClientFromCharacter(__instance);
                        string playerName = "Unknown";
                        if (outfit != null)
                            playerName = outfit.PlayerName;
                        else if (client != null)
                        {
                            playerName = client.PlayerName;
                            __instance.SetName(playerName);
                        }

                        bool isMushroomMixedUp = false;

                        if (Utilities.GetCurrentMap() == MapNames.Fungle && PlayerControl.LocalPlayer.myTasks != null)
                        {
                            foreach (var task in PlayerControl.LocalPlayer.myTasks)
                            {
                                if (task.TaskType == TaskTypes.MushroomMixupSabotage)
                                {
                                    isMushroomMixedUp = true;
                                    break;
                                }
                            }
                        }

                        var gameOptions = GameOptionsManager.Instance.CurrentGameOptions;
                        bool hideName = isMushroomMixedUp || (gameOptions.GameMode == GameModes.HideNSeek && !gameOptions.GetBool(BoolOptionNames.ShowCrewmateNames));

                        bool shouldSeeName = ((Instance.Enabled || Instance.ShowKillCD) || !hideName) && __instance.Visible;

                        if (Utilities.InLobby() && Instance.Enabled)
                        {
                            Color roleColor = Utilities.GetRoleColor(playerData.Role);
                            playerName = $"<#{ColorUtility.ToHtmlStringRGB(roleColor)}>{playerName}</color>";
                        }

                        if (Utilities.InLobby() && Instance.ShowPlayerInfo)
                        {
                            uint playerLevel = playerData.PlayerLevel + 1;
                            int playerId = Utilities.GetPlayerControlById(playerData.PlayerId).OwnerId;
                            ClientData host = AmongUsClient.Instance.GetHost();

                            string platform = "Unknown";
                            if (client != null)
                                platform = Utilities.GetPlatformString(client.PlatformData.Platform);

                            string friendCode = playerData.FriendCode;
                            bool streamerMode = DataManager.Settings.Gameplay.StreamerMode;

                            if (streamerMode || __instance == PlayerControl.LocalPlayer)
                                friendCode = "[Hidden]";

                            if (client != null && client == host)
                            {
                                if (friendCode == "" && !streamerMode)
                                    playerName = $"<size=1.4><#fb0>[Host]</color> <#0ff>ID {playerData.PlayerId}</color> <#f0f>Level {playerLevel}</color> <#b0f>({platform})</color></size>\n" +
                                        $"{playerName}\n<size=1.4><#0000>0</color><#0bf>No Friend Code</color><#0000>0</color></size>";
                                else
                                    playerName = $"<size=1.4><#fb0>[Host]</color> <#0ff>ID {playerData.PlayerId}</color> <#f0f>Level {playerLevel}</color> <#b0f>({platform})</color></size>\n" +
                                        $"{playerName}\n<size=1.4><#0000>0</color><#0bf>{friendCode}</color><#0000>0</color></size>";
                            }
                            else
                            {
                                if (friendCode == "" && !streamerMode)
                                    playerName = $"<size=1.4><#0ff>ID {playerData.PlayerId}</color> <#f0f>Level {playerLevel}</color> <#b0f>({platform})</color></size>\n" +
                                        $"{playerName}\n<size=1.4><#0000>0</color><#0bf>No Friend Code</color><#0000>0</color></size>";
                                else
                                    playerName = $"<size=1.4><#0ff>ID {playerData.PlayerId}</color> <#f0f>Level {playerLevel}</color> <#b0f>({platform})</color></size>\n" +
                                        $"{playerName}\n<size=1.4><#0000>0</color><#0bf>{friendCode}</color><#0000>0</color></size>";
                            }
                        }

                        if (Utilities.InGame() && Instance.Enabled && shouldSeeName)
                        {
                            string roleName = Utilities.GetRoleName(playerData.Role);

                            int completedTasks = 0;
                            int totalTasks = 0;

                            var tasks = Utilities.GetNormalPlayerTasks(__instance);
                            foreach (var task in tasks)
                            {
                                if (task == null) continue;
                                if (task.taskStep == task.MaxStep)
                                    completedTasks++;
                                totalTasks++;
                            }

                            if (Instance.Enabled)
                            {
                                Color roleColor = Utilities.GetRoleColor(playerData.Role);
                                if (totalTasks == 0 || (Utilities.IsImpostor(playerData) && completedTasks == 0))
                                    playerName = $"<#{ColorUtility.ToHtmlStringRGB(roleColor)}><size=1.4>{roleName}\n</size>{playerName}\n<size=1.4><#0000>0</color></size>";
                                else
                                    playerName = $"<#{ColorUtility.ToHtmlStringRGB(roleColor)}><size=1.4>{roleName} ({completedTasks}/{totalTasks}) \n</size>{playerName}\n<size=1.4><#0000>0</color></size></color>";
                            }
                        }
                        else if (Utilities.IsImpostor(playerData) && Utilities.IsImpostor(localData))
                            playerName = $"<#{ColorUtility.ToHtmlStringRGB(Palette.ImpostorRed)}>{playerName}</color>";

                        if (Utilities.InGame() && playerData.Role && Utilities.IsImpostor(playerData) && !playerData.IsDead)
                            playerData.Role.CanUseKillButton = true;

                        if (Utilities.InGame() && Instance.ShowKillCD && !playerData.IsDead && playerData.Role && playerData.Role.CanUseKillButton && shouldSeeName)
                        {
                            float killTimer = __instance.killTimer;
                            Color color = GetKillCooldownColor(killTimer);
                            if (Instance.Enabled)
                                playerName += $"<size=1.4><#{ColorUtility.ToHtmlStringRGB(color)}>Kill Cooldown: {killTimer:F2}s</color><#0000>0</color></size>";
                            else
                                playerName += $"<size=1.4><#0000>0\n</color></size>{playerName}\n<size=1.4><#{ColorUtility.ToHtmlStringRGB(color)}>Kill Cooldown: {killTimer:F2}s</color></size>";
                        }

                        if (Utilities.InGame() && !shouldSeeName)
                        {
                            playerName = $"<#0000>{Utilities.RemoveHTMLTags(playerName)}</color>";
                        }

                        nameText.text = playerName;

                        if (DataManager.Settings.Accessibility.ColorBlindMode)
                        {
                            var colorBlindText = __instance.cosmetics.colorBlindText;
                            string text = colorBlindText.text;
                            if ((Instance.ShowPlayerInfo && Utilities.InLobby()) || (Instance.ShowKillCD && Utilities.InGame() && playerData.Role && playerData.Role.CanUseKillButton))
                                text = !text.Contains("\n\n") ? "\n+n" + text : text;
                            else
                                text = text.Contains("\n\n") ? text[2..] : text;
                            colorBlindText.text = text;
                        }

                        if (__instance != PlayerControl.LocalPlayer)
                        {
                            var role = playerData.Role;
                            if (role != null && role.CanUseKillButton && !playerData.IsDead)
                            {
                                if (__instance.ForceKillTimerContinue || __instance.IsKillTimerEnabled)
                                    __instance.killTimer = Mathf.Max(__instance.killTimer - Time.fixedDeltaTime, 0f);
                            }
                        }

                        bool shouldSeePhantom = __instance == PlayerControl.LocalPlayer || Utilities.IsImpostor(localData) || localData.IsDead;
                        var roleType = playerData.RoleType;

                        if (roleType == RoleTypes.Phantom && !shouldSeePhantom)
                        {
                            var phantomRole = (PhantomRole)(playerData.Role);
                            bool isFullyVanished = phantomRole.isInvisible;
                            if (isFullyVanished && __instance.invisibilityAlpha < 0.5f && ModuleManager.showPhantoms.Enabled)
                            {
                                __instance.SetInvisibility(false);
                                bool wasDead = false;
                                if (__instance != null && !localData.IsDead)
                                {
                                    localData.IsDead = true;
                                    wasDead = true;
                                }
                                __instance.SetInvisibility(true);
                                if (wasDead) localData.IsDead = false;
                            }
                            if (isFullyVanished && __instance.invisibilityAlpha == 0.5f && !ModuleManager.showPhantoms.Enabled)
                                __instance.SetInvisibility(true);
                        }
                    }
                } catch { }
            }
        }

        [HarmonyPatch(typeof(MeetingHud), nameof(MeetingHud.Update))]
        class MeetingRoles
        {
            static void Prefix(MeetingHud __instance)
            {
                try
                {
                    bool isBeforeResultsState = __instance.state < MeetingHud.MeetingStates.Results;
                    var playerStates = __instance.playerStates;
                    foreach (var playerVoteArea in playerStates)
                    {
                        if (!playerVoteArea) continue;
                        var playerData = GameData.Instance.GetPlayerById(playerVoteArea.PlayerId);
                        var localData = PlayerControl.LocalPlayer?.Data;
                        var playerControl = Utilities.GetPlayerControlById(playerVoteArea.PlayerId);
                        var playerNameText = playerVoteArea.NameText;
                        var outfit = Utilities.GetPlayerOutfit(playerData);

                        string playerName = outfit.PlayerName;

                        if (playerData && localData && outfit != null)
                        {
                            if (Instance.Enabled)
                            {
                                string roleName = Utilities.GetRoleName(playerData.Role);
                                if (!playerData.Disconnected)
                                {
                                    int completedTasks = 0;
                                    int totalTasks = 0;

                                    var tasks = Utilities.GetNormalPlayerTasks(playerControl);
                                    foreach (var task in tasks)
                                    {
                                        if (task == null) continue;
                                        if (task.taskStep == task.MaxStep)
                                            completedTasks++;
                                        totalTasks++;
                                    }

                                    if (totalTasks == 0 || (Utilities.IsImpostor(playerData) && completedTasks == 0))
                                        playerName = $"<size=1.2>{roleName}\n</size>{playerName}\n<size=1.2><#0000>0</color></size>";
                                    else
                                        playerName = $"<size=1.2>{roleName} ({completedTasks}/{totalTasks})\n</size>{playerName}\n<size=1.2><#0000>0</color></size>";
                                }
                                else
                                    playerName = $"size=1.2>{roleName} (D/C)\n</size>{playerName}\n<size=1.2><#0000>0</color></size>";
                                Color roleColor = Utilities.GetRoleColor(playerData.Role);

                                playerName = $"<#{ColorUtility.ToHtmlStringRGB(roleColor)}>{playerName}</color>";
                            }
                            else if (Utilities.IsImpostor(playerData) && Utilities.IsImpostor(localData))
                                playerName = $"<#{ColorUtility.ToHtmlStringRGB(Palette.ImpostorRed)}>{playerName}</color>";

                            playerNameText.text = playerName;

                            playerNameText.color = new Color(1f, 1f, 1f, 1f);
                        }

                        if (playerData)
                        {
                            bool didVote = (playerVoteArea.VotedForId != PlayerVoteArea.HasNotVoted);
                            if (didVote && playerVoteArea.VotedForId != PlayerVoteArea.MissedVote && playerVoteArea.VotedForId != PlayerVoteArea.DeadVote && !GameState.voteMonitor.ContainsKey(playerVoteArea.VotedForId))
                            {
                                GameState.voteMonitor[playerData.PlayerId] = playerVoteArea.VotedForId;

                                if (isBeforeResultsState)
                                {
                                    if (playerVoteArea.VotedForId != PlayerVoteArea.SkippedVote)
                                    {
                                        foreach (var votedForArea in playerStates)
                                        {
                                            if (votedForArea.PlayerId == playerVoteArea.VotedForId)
                                            {
                                                __instance.BloopAVoteIcon(playerData, 0, votedForArea.transform);
                                                break;
                                            }
                                        }
                                    }
                                    else if (__instance.SkippedVoting)
                                        __instance.BloopAVoteIcon(playerData, 0, __instance.SkippedVoting.transform);

                                    var gameOptions = GameOptionsManager.Instance.CurrentGameOptions;
                                    if (!GameState.InMeeting && !__instance || !gameOptions.GetBool(BoolOptionNames.AnonymousVotes)) continue;

                                    foreach (var votedForArea in playerStates)
                                    {
                                        if (!votedForArea) continue;
                                        if (!votedForArea.transform) continue;

                                        var voteSpreader = votedForArea.transform.GetComponent<VoteSpreader>();
                                        if (!voteSpreader) continue;

                                        var votes = voteSpreader.Votes;
                                        if (ModuleManager.revealVotes.RevealAnonymousVotes)
                                        {
                                            int idx = 0;
                                            foreach (var pair in GameState.voteMonitor)
                                            {
                                                if (pair.Value == votedForArea.PlayerId)
                                                {
                                                    if (idx >= votes.Count) break;

                                                    var vOutfit = Utilities.GetPlayerOutfit(GameData.Instance.GetPlayerById(pair.Key));
                                                    if (vOutfit == null) continue;

                                                    PlayerMaterial.SetColors(vOutfit.ColorId, votes[idx++]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (var spriteRenderer in votes)
                                            {
                                                PlayerMaterial.SetColors(Palette.DisabledGrey, spriteRenderer);
                                            }
                                        }
                                    }
                                    if (__instance.SkippedVoting)
                                    {
                                        if (!__instance.SkippedVoting.transform) continue;

                                        var voteSpreader = __instance.SkippedVoting.transform.GetComponent<VoteSpreader>();
                                        if (!voteSpreader) continue;

                                        var votes = voteSpreader.Votes;
                                        if (ModuleManager.revealVotes.RevealAnonymousVotes)
                                        {
                                            int idx = 0;
                                            foreach (var pair in GameState.voteMonitor)
                                            {
                                                if (pair.Value == PlayerVoteArea.SkippedVote)
                                                {
                                                    if (idx >= votes.Count) break;

                                                    var vOutfit = Utilities.GetPlayerOutfit(GameData.Instance.GetPlayerById(pair.Key));
                                                    if (vOutfit == null) continue;

                                                    PlayerMaterial.SetColors(vOutfit.ColorId, votes[idx++]);
                                                }
                                            }
                                        }
                                        else
                                        {
                                            foreach (var spriteRenderer in votes)
                                            {
                                                PlayerMaterial.SetColors(Palette.DisabledGrey, spriteRenderer);
                                            }
                                        }
                                    }
                                }
                            }
                            else if (!didVote && GameState.voteMonitor.ContainsKey(playerData.PlayerId))
                            {
                                var dcPlayer = GameState.voteMonitor[playerData.PlayerId];
                                GameState.voteMonitor.Remove(playerData.PlayerId);

                                foreach (var votedForArea in playerStates)
                                {
                                    if (votedForArea.PlayerId == dcPlayer)
                                    {
                                        var voteSpreader = votedForArea.transform.GetComponent<VoteSpreader>();
                                        if (!voteSpreader) break;

                                        var votes = voteSpreader.Votes;
                                        var length = votes.Count;
                                        if (length == 0) break;

                                        Object.DestroyImmediate(votes[length - 1]);
                                        votes.RemoveAt(length - 1);

                                        break;
                                    }
                                }
                            }
                        }
                    }

                    if (isBeforeResultsState)
                    {
                        foreach (var votedForArea in playerStates)
                        {
                            if (!votedForArea) continue;

                            var voteSpreader = votedForArea.transform.GetComponent<VoteSpreader>();
                            if (!voteSpreader) continue;

                            foreach (var spriteRenderer in voteSpreader.Votes)
                            {
                                spriteRenderer.gameObject.SetActive(ModuleManager.revealVotes.Enabled);
                            }
                        }

                        if (__instance.SkippedVoting)
                            __instance.SkippedVoting.SetActive(GameState.voteMonitor.ContainsValue(PlayerVoteArea.SkippedVote) && ModuleManager.revealVotes.Enabled);
                    }
                } catch { }
            }
        }

        [HarmonyPatch(typeof(PlayerIdentifierButton), nameof(PlayerIdentifierButton.Populate))]
        class MatchInfoRoles
        {
            static void Postfix(PlayerIdentifierButton __instance, NetworkedPlayerInfo player)
            {
                var outfit = Utilities.GetPlayerOutfit(player);
                if (outfit == null) return;
                string playerName = outfit.PlayerName;

                if (Instance.Enabled)
                {
                    string roleName = Utilities.GetRoleName(player.Role);
                    if (!player.Disconnected)
                    {
                        int completedTasks = 0;
                        int totalTasks = 0;
                        if (player._object)
                        {
                            var tasks = Utilities.GetNormalPlayerTasks(player._object);
                            foreach (var task in tasks)
                            {
                                if (task == null) continue;
                                if (task.taskStep == task.MaxStep)
                                    completedTasks++;
                                totalTasks++;
                            }
                        }
                        if (totalTasks == 0 || (Utilities.IsImpostor(player) && completedTasks == 0))
                            playerName = $"<size=1.2>{roleName}\n</size>{playerName}\n<size=1.2><#0000>0</color></size>";
                        else
                            playerName = $"<size=1.2>{roleName} ({completedTasks}/{totalTasks})\n</size>{playerName}\n<size=1.2><#0000>0</color></size>";
                    }
                    else
                        playerName = $"<size=1.2>{roleName} (D/C)\n</size>{playerName}\n<size=1.2><#0000>0</color></size>";
                    Color roleColor = Utilities.GetRoleColor(player.Role);

                    playerName = $"<#{ColorUtility.ToHtmlStringRGB(roleColor)}>{playerName}</color>";
                }

                __instance.NameText.text = playerName;
            }
        }

        [HarmonyPatch(typeof(ExileController), nameof(ExileController.ReEnableGameplay))]
        class ResetKillCD0
        {
            static void Postfix()
            {
                try
                {
                    foreach (var pc in PlayerControl.AllPlayerControls)
                    {
                        if (pc && pc.Data && pc != PlayerControl.LocalPlayer && !pc.Data.Disconnected)
                        {
                            var role = pc.Data.Role;
                            if (role != null && role.CanUseKillButton && !pc.Data.IsDead)
                            {
                                var gameOptions = GameOptionsManager.Instance.CurrentGameOptions;
                                pc.killTimer = Mathf.Max(gameOptions.GetFloat(FloatOptionNames.KillCooldown), 0f);
                            }
                        }
                    }
                } catch { }
            }
        }

        [HarmonyPatch(typeof(SabotageSystemType), nameof(SabotageSystemType.SetInitialSabotageCooldown))]
        class ResetKillCD1
        {
            static void Postfix()
            {
                foreach (var pc in PlayerControl.AllPlayerControls)
                {
                    if (pc && pc.Data && pc != PlayerControl.LocalPlayer && !pc.Data.Disconnected)
                    {
                        var role = pc.Data.Role;
                        if (role != null && role.CanUseKillButton && !pc.Data.IsDead)
                            pc.killTimer = 10f;
                    }
                }
            }
        }

        [HarmonyPatch(typeof(InnerNetClient), nameof(InnerNetClient.Update))]
        class ResetKillCD2
        {
            static void Prefix()
            {
                try
                {
                    var gameOptions = GameOptionsManager.Instance.CurrentGameOptions;
                    if ((Utilities.InGame() || Utilities.InLobby()) && gameOptions.GameMode == GameModes.Normal && PlayerControl.LocalPlayer.Data != null && AmongUsClient.Instance.AmHost && ModuleManager.noKillChecks.NoKillCooldown)
                    {
                        var logicGameOptions = GameManager.Instance.LogicOptions;
                        if (logicGameOptions.GetKillCooldown() > 0)
                            PlayerControl.LocalPlayer.killTimer = 0f;
                    }
                } catch { }
            }
        }
    }
}
