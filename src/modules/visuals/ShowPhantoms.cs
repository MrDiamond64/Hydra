using AmongUs.GameOptions;
using HarmonyLib;

namespace LunarMenu.modules.visuals
{
    internal class ShowPhantoms : Module
    {
        public ShowPhantoms() : base("ShowPhantoms") { }

        private static ShowPhantoms Instance
        {
            get { return ModuleManager.showPhantoms; }
        }

        [HarmonyPatch(typeof(PlayerPhysics), nameof(PlayerPhysics.FixedUpdate))]
        class ShowPhantomed
        {
            static void Prefix(PlayerPhysics __instance)
            {
                try
                {
                    var player = __instance.myPlayer;
                    var playerData = player?.Data;
                    var localData = PlayerControl.LocalPlayer?.Data;

                    if (player == null || playerData == null || localData == null) return;
                    if (player.cosmetics == null) return;

                    bool shouldSeePhantom = __instance.myPlayer == PlayerControl.LocalPlayer || Utilities.IsImpostor(localData) || localData.IsDead || Instance.Enabled;
                    var roleType = playerData.RoleType;

                    bool isFullyVanished = GameState.vanishedPlayers.Contains(playerData.PlayerId);

                    if (player.transform != null)
                    {
                        var vanishEffect = player.transform.FindChild("VanishChargeEffect(Clone)");
                        if (roleType == RoleTypes.Phantom && vanishEffect != null)
                            isFullyVanished = !isFullyVanished;
                    }

                    bool isDead = playerData.IsDead;
                    var nameText = player.cosmetics.nameText.gameObject;
                    bool isSeekerBody = player.cosmetics.bodyType == PlayerBodyTypes.Seeker || player.cosmetics.bodyType == PlayerBodyTypes.LongSeeker;
                    if (player.inVent)
                    {
                        if (!player.Visible && ModuleManager.showPlayersInVents.Enabled && (!isFullyVanished || shouldSeePhantom))
                        {
                            player.Visible = true;
                            player.invisibilityAlpha = 0.5f;
                            player.cosmetics.SetPhantomRoleAlpha(player.invisibilityAlpha);
                            if (isSeekerBody)
                                player.cosmetics.skin.layer.color = Palette.ClearWhite;
                        }
                        else if (player.invisibilityAlpha == 0.5f && (!(ModuleManager.showPlayersInVents.Enabled && (!isFullyVanished || shouldSeePhantom))))
                        {
                            player.Visible = false;
                            player.invisibilityAlpha = 0f;
                            player.cosmetics.SetPhantomRoleAlpha(player.invisibilityAlpha);
                            if (isSeekerBody)
                                player.cosmetics.skin.layer.color = Palette.ClearWhite;
                        }
                        nameText.SetActive(player.invisibilityAlpha > 0f);
                    }
                    else if (!isDead)
                    {
                        player.invisibilityAlpha = isFullyVanished ? (shouldSeePhantom ? 0.5f : 0f) : 1f;
                        player.cosmetics.SetPhantomRoleAlpha(player.invisibilityAlpha);
                        player.Visible = player.invisibilityAlpha > 0f;
                        if (isSeekerBody)
                            player.cosmetics.skin.layer.color = Palette.ClearWhite;
                        nameText.SetActive(player.invisibilityAlpha > 0f);
                    }
                    else if (isDead)
                        player.Visible = localData.IsDead || ModuleManager.showGhosts.Enabled;
                } catch { }
            }
        }
    }
}
