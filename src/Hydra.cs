using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using HydraMenu.features;
using HydraMenu.routines;
using HydraMenu.ui;

namespace HydraMenu;

[BepInPlugin("com.mrd.hydramenu", "Hydra", "1.9.0.0")]
[BepInProcess("Among Us.exe")]
internal class Hydra : BasePlugin
{
	internal static new ManualLogSource Log;

	public static RoutineManager routines;
	public static NotificationManager notifications;

	public override void Load()
	{
		Log = base.Log;

		AddComponent<MainUI>();
		AddComponent<Roles>();

		notifications = AddComponent<NotificationManager>();
		routines = AddComponent<RoutineManager>();

		try
		{
			Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
			harmony.PatchAll();
		}
		catch
		{
			notifications.Send("Fatal Error", "Harmony patches failed to load, you are likely using an unsupported version. Check https://github.com/MrDiamond64/Hydra for more information.", 9999);
			throw;
		}

		Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} has loaded!");
	}

	[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Awake))]
	class OnGameLoad
	{
		public static void Postfix()
		{
			Log.LogInfo("Adding mod stamp");
			ModManager.Instance.ShowModStamp();
		}
	}
}