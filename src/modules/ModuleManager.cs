using HydraMenu.modules.self;
using HydraMenu.modules.visuals;

namespace HydraMenu.modules
{
	internal class ModuleManager
	{
		// Self
		public static AlwaysShowTaskAnimations alwaysShowTaskAnimations = new AlwaysShowTaskAnimations();
		public static Immortality immortality = new Immortality();
		public static NoLadderCooldown noLadderCooldown = new NoLadderCooldown();
		public static SpeedModifier speedModifier = new SpeedModifier();
		public static UnlimitedMeetings unlimitedMeetings = new UnlimitedMeetings();
		public static UpdateStatsFreeplay updateStatsFreeplay = new UpdateStatsFreeplay();

		// Visual
		public static AlwaysVisibleChat alwaysVisibleChat = new AlwaysVisibleChat();
		public static ShowGhostMessages showGhostMessages = new ShowGhostMessages();
	}
}