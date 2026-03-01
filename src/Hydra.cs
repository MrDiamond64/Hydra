using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using HydraMenu.features;
using HydraMenu.routines;
using HydraMenu.ui;

namespace HydraMenu;

[BepInPlugin("com.mrd.hydramenu", "Hydra", "1.0.0.0")]
[BepInProcess("Among Us.exe")]
public class Hydra : BasePlugin
{
	internal static new ManualLogSource Log;

	public static RoutineManager routines;
	public static NotificationManager notifications;

	public override void Load()
	{
		var harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
		harmony.PatchAll();

		AddComponent<MainUI>();
		AddComponent<Roles>();

		notifications = AddComponent<NotificationManager>();
		routines = AddComponent<RoutineManager>();

		Log = base.Log;
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