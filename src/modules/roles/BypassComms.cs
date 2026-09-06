using AmongUs.GameOptions;
using HarmonyLib;

namespace LunarMenu.modules.roles
{
    internal class BypassComms : Module
    {
        public BypassComms() : base("BypassComms") { }

        private static BypassComms Instance
        {
            get { return ModuleManager.bypassComms; }
        }

        [HarmonyPatch(typeof(RoleBehaviour), nameof(RoleBehaviour.CommsSabotaged))]
        class BypassCommsPatch
        {
            static bool Prefix(RoleBehaviour __instance, ref bool __result)
            {
                if ((__instance.Role == RoleTypes.Engineer && ModuleManager.ventAsCrewmate.Enabled) || Instance.Enabled)
                {
                    __result = false;
                    return false;
                }

                if (__instance.Role == RoleTypes.Judge && GameState.InMeeting && Instance.Enabled)
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }
    }
}
