using HarmonyLib;

namespace LunarMenu.modules.general
{
    internal class UnlockCosmetics : Module
    {
        public UnlockCosmetics() : base("UnlockCosmetics") { }

        private static UnlockCosmetics Instance
        {
            get { return ModuleManager.unlockCosmetics; }
        }

        [HarmonyPatch(typeof(PlayerPurchasesData), nameof(PlayerPurchasesData.GetPurchase))]
        class UnlockAllCosmetics
        {
            static void Prefix(ref bool __result)
            {
                if (Instance.Enabled) __result = true;
            }
        }
    }
}
