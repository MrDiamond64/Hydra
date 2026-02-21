using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu
{
    internal class Teleporter
    {
		// This exists for the same reason as why the UpdateSystemsDirectly option for sabotages exist
		public static bool UseSnapToRPC { get; set; } = true;

        public static Dictionary<string, Vector2> skeldTeleportLocations = new Dictionary<string, Vector2>()
        {
            { "Cafeteria", new Vector2(-0.78f, 2.48f) },
            { "Weapons", new Vector2(8.04f, 1.24f) },
			{ "Medbay", new Vector2(-8.61f, -4.30f) },
			{ "Admin", new Vector2(2.79f, -7.69f) },
            { "Oxygen", new Vector2(6.50f, -3.50f) },
            { "Navigation", new Vector2(16.77f, -4.67f) },
            { "Shields", new Vector2(9.26f, -12.19f) },
            { "Communications", new Vector2(5.10f, -15.24f ) },
            { "Storage", new Vector2(-3.72f, -14.61f ) },
            { "Electrical", new Vector2(-6.91f, -8.60f ) },
            { "Upper Engine", new Vector2(-17.61f, -0.72f) },
            { "Lower Engine", new Vector2(-17.33f, -13.10f ) },
            { "Security", new Vector2(-12.81f, -3.01f) },
            { "Reactor", new Vector2(-20.53f, -5.39f) },
        };

        public static Dictionary<string, Vector2> polusTeleportLocations = new Dictionary<string, Vector2>()
        {
            { "Dropship", new Vector2(16.61f, -1.17f) },
            { "Storage", new Vector2(20.35f, -11.46f) },
            { "Electrical", new Vector2(7.51f, -9.86f) },
            { "Security", new Vector2(2.98f, -12.18f) },
            { "Oxygen", new Vector2(1.55f, -16.81f ) },
            { "Boiler Room", new Vector2(2.14f, -23.55f) },
            { "Communications", new Vector2(11.70f, -15.87f ) },
            { "Weapons", new Vector2(10.71f, -22.90f) },
            { "Office", new Vector2(25.00f, -16.99f) },
            { "Admin", new Vector2(22.76f, -22.32f) },
            { "Laboratory", new Vector2(33.48f, -7.41f) },
            { "Specimen", new Vector2(36.78f, -19.28f) }
        };

        public static Dictionary<string, Vector2> GetTeleportLocations()
        {
            MapNames currentMap = Utilities.GetCurrentMap();

			switch(currentMap)
            {
                case MapNames.Skeld:
                    return skeldTeleportLocations;

                case MapNames.Polus:
                    return polusTeleportLocations;

                // If we don't have any teleport locations for the current map then just default to the Skeld ones
                default:
                    return skeldTeleportLocations;
            }
        }

		public static void TeleportTo(Vector2 position)
		{
			if(UseSnapToRPC)
			{
				PlayerControl.LocalPlayer.NetTransform.RpcSnapTo(position);
			}
			else
			{
				PlayerControl.LocalPlayer.NetTransform.SnapTo(position);
			}
		}
	}
}