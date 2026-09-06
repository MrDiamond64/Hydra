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

        [HarmonyPatch(typeof(RoleBehaviour), "get_CommsSabotaged")]
        class BypassCommsPatch
        {
            static bool Prefix(RoleBehaviour __instance, ref bool __result)
            {
                if (__instance == null || __instance.Pointer == System.IntPtr.Zero) return true;

                bool isEngineer = __instance.Role.Equals(RoleTypes.Engineer);
                bool isJudge = __instance.Role.Equals(RoleTypes.Judge);

                if ((isEngineer && ModuleManager.ventAsCrewmate.Enabled) || Instance.Enabled)
                {
                    __result = false;
                    return false;
                }

                if (isJudge && GameState.InMeeting && Instance.Enabled)
                {
                    __result = false;
                    return false;
                }

                return true;
            }
        }
    }
}
