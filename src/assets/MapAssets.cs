using System.Collections.Generic;

namespace LunarMenu.assets
{
	internal class MapAssets
	{
		public static readonly Dictionary<string, TaskTypes> skeldAnimations = new Dictionary<string, TaskTypes>()
		{
			{ "Clear Asteroids", TaskTypes.ClearAsteroids },
			{ "Empty Garbage", TaskTypes.EmptyGarbage },
			{ "Prime Shields", TaskTypes.PrimeShields }
		};

		public static readonly Dictionary<string, TaskTypes> polusAnimations = new Dictionary<string, TaskTypes>()
		{
			{ "Clear Asteroids", TaskTypes.ClearAsteroids }
		};

		public static readonly SortedDictionary<int, string> skeldVents = new SortedDictionary<int, string>()
		{
			{ 0, "Admin" },
			{ 1, "Hallway" },
			{ 2, "Cafeteria" },
			{ 3, "Electrical" },
			{ 4, "Upper Engine" },
			{ 5, "Security" },
			{ 6, "Medbay" },
			{ 7, "Weapons" },
			{ 8, "Lower Reactor" },
			{ 9, "Lower Engine" },
			{ 10, "Shields" },
			{ 11, "Upper Reactor" },
			{ 12, "Upper Navigation"},
			{ 13, "Lower Navigation" }
		};

		public static readonly SortedDictionary<int, string> miraVents = new SortedDictionary<int, string>()
		{
			// Mira HQ has no vent with the ID of 0
			// Fortebass' explanation on why this is the case can be found at https://github.com/roobscoob/among-us-protocol/blob/master/images/mirahq_vents_quote.png
			// Very informative as you can see... At least we know why the game uses 255 as a default value frequently in the code
			{ 1, "Balcony" },
			{ 2, "Above Cafeteria" },
			{ 3, "Reactor" },
			{ 4, "Laboratory" },
			{ 5, "Office" },
			{ 6, "Admin" },
			{ 7, "Greenhouse" },
			{ 8, "Medbay" },
			{ 9, "Decontamination" },
			{ 10, "Locker Room" },
			{ 11, "Launchpad" }
		};

		public static readonly SortedDictionary<int, string> polusVents = new SortedDictionary<int, string>()
		{
			{ 0, "Electrical" },
			{ 1, "Outside Electrical" },
			{ 2, "Oxygen" },
			{ 3, "Outside Comms" },
			{ 4, "Office" },
			{ 5, "Comms" },
			{ 6, "Laboratory" },
			{ 7, "Outside Laboratory" },
			{ 8, "Storage" },
			{ 9, "Outside Rocket" },
			{ 10, "Above Electrical" },
			{ 11, "Outside Office" }
		};

		public static readonly SortedDictionary<int, string> airshipVents = new SortedDictionary<int, string>()
		{
			{ 0, "Vault" },
			{ 1, "Cockpit" },
			{ 2, "Viewing Deck" },
			{ 3, "Engine Room" },
			{ 4, "Kitchen" },
			{ 5, "Bedroom 1" },
			{ 6, "Janitor Roomw" },
			{ 7, "Gap Room Right" },
			{ 8, "Gap Room Left" },
			{ 9, "Showers" },
			{ 10, "Records" },
			{ 11, "Cargo Bay" }
		};

		public static readonly SortedDictionary<int, string> fungleVents = new SortedDictionary<int, string>()
		{
			{ 0, "Comms" },
			{ 1, "Kitchen" },
			{ 2, "Lookout" },
			{ 3, "Above Meeting Room" },
			{ 4, "Laboratory" },
			{ 5, "Reactor" },
			{ 6, "Jungle" },
			{ 7, "Jungle Bottom" },
			{ 8, "Splash Zone" },
			{ 9, "Outside Dropship" }
		};

		public static Dictionary<string, TaskTypes> GetAnimations()
		{
			MapNames currentMap = Utilities.GetCurrentMap();

			return currentMap switch
			{
				MapNames.Skeld or MapNames.Dleks => skeldAnimations,
				MapNames.Polus => polusAnimations,
				// These maps do not have any task animations, other than medbay scan
				MapNames.MiraHQ or MapNames.Airship or MapNames.Fungle => [],
				// If we do not any known animations for the current map then just default to the Skeld ones
				_ => skeldAnimations
			};
		}

		public static SortedDictionary<int, string> GetVents()
		{
			MapNames currentMap = Utilities.GetCurrentMap();

			return currentMap switch
			{
				MapNames.Skeld or MapNames.Dleks => skeldVents,
				MapNames.MiraHQ => miraVents,
				MapNames.Polus => polusVents,
				MapNames.Airship => airshipVents,
				MapNames.Fungle => fungleVents,
				// If we do not any known vents for the current map then just default to the Skeld ones
				_ => skeldVents
			};
		}
	}
}