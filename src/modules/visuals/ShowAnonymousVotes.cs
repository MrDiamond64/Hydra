using HarmonyLib;

namespace HydraMenu.modules.visuals
{
	internal class ShowAnonymousVotes : Module
	{
		public ShowAnonymousVotes() : base("ShowAnonymousVotes")
		{
			base.Enabled = true;
		}

		private static ShowAnonymousVotes Instance
		{
			get { return ModuleManager.showAnonymousVotes; }
		}

		[HarmonyPatch(typeof(LogicOptionsNormal), nameof(LogicOptionsNormal.GetAnonymousVotes))]
		class SetAnonymousVotesDisabled
		{
			static bool Prefix(ref bool __result)
			{
				if(!Instance.Enabled) return true;

				__result = false;
				return false;
			}
		}
	}
}