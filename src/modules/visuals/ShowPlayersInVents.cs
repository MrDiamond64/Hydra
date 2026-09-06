using HarmonyLib;

namespace LunarMenu.modules.visuals
{
    internal class ShowPlayersInVents : Module
    {
        public ShowPlayersInVents() : base("ShowPlayersInVents") { }

        private static ShowPlayersInVents Instance
        {
            get { return ModuleManager.showPlayersInVents; }
        }

        [HarmonyPatch(typeof(Vent), nameof(Vent.TryMoveToVent))]
        class SetLocalVisibility
        {
            static void Prefix()
            {
                if (PlayerControl.LocalPlayer != null)
                {
                    bool wasVisible = PlayerControl.LocalPlayer.Visible && !PlayerControl.LocalPlayer.walkingToVent && Instance.Enabled && !PlayerControl.LocalPlayer.Data.IsDead;
                    if (wasVisible && PlayerControl.LocalPlayer.inVent)
                        PlayerControl.LocalPlayer.Visible = false;
                }
            }
        }
    }
}
