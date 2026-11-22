using BepInEx;
using BepInEx.Logging;
using BepInEx.Unity.IL2CPP;
using HarmonyLib;
using HydraMenu.assets;
using HydraMenu.modules;
using HydraMenu.routines;
using HydraMenu.ui;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu;

[BepInPlugin("com.mrd.hydramenu", "Hydra", "2.2.0.0")]
[BepInProcess("Among Us.exe")]
internal class Hydra : BasePlugin
{
	internal static new ManualLogSource Log;
	private static readonly Harmony harmony = new Harmony(MyPluginInfo.PLUGIN_GUID);
	public static readonly ConfigManager config = new ConfigManager();

	public static MainUI mainUI;
	public static NotificationManager notifications;
	public static ModuleManager modules;
	public static RoutineManager routines;

	public override void Load()
	{
		Log = base.Log;

		mainUI = AddComponent<MainUI>();
		notifications = AddComponent<NotificationManager>();
		modules = AddComponent<ModuleManager>();
		routines = AddComponent<RoutineManager>();

		try
		{
			harmony.PatchAll();
		}
		catch
		{
			notifications.Send("Fatal Error", "Harmony patches failed to load, you are likely using an unsupported version. Check https://github.com/MrDiamond64/Hydra for more information.", 9999);
			throw;
		}

		config.Initialize();

		Log.LogInfo($"Plugin {MyPluginInfo.PLUGIN_GUID} has loaded!");
	}

	public static void Eject()
	{
		harmony.UnpatchSelf();

		Object.Destroy(mainUI);
		Object.Destroy(notifications);
		Object.Destroy(modules);
		Object.Destroy(routines);

		ModManager.Instance.ModStamp.enabled = false;
		ModManager.Instance.gameObject.SetActive(false);
	}

	[HarmonyPatch(typeof(MainMenuManager), nameof(MainMenuManager.Awake))]
	class OnGameLoad
	{
		public static void Postfix()
		{
			int broadcastVersion = Constants.GetBroadcastVersion();

			if(!GameAssets.BroadcastVersions.ContainsKey(broadcastVersion))
			{
				Log.LogWarning($"BroadcastVersion list is missing broadcast version {broadcastVersion}");

				GameAssets.BroadcastVersions.Add(broadcastVersion, ReferenceDataManager.Instance.Refdata.userFacingVersion);
			}

			Log.LogInfo("Adding mod stamp");
			ModManager.Instance.ShowModStamp();
		}
	}
}