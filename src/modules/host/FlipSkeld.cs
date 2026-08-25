using UnityEngine.AddressableAssets;

namespace HydraMenu.modules.host
{
	internal class FlipSkeld : Module
	{
		public FlipSkeld() : base("FlipSkeld") { }

		private static FlipSkeld Instance
		{
			get { return ModuleManager.flipSkeld; }
		}

		private void SwapMapAssets()
		{
			if(AmongUsClient.Instance == null)
			{
				_enabled = false;
				return;
			}

			// ShipPrefabs is a list corresponding map IDs to their map
			// ID 0 is Skeld, 1 is Mira, 2 is Polus, and 3 is Dleks
			// If we want to be able to spawn in Dleks (as this is normally inaccessible) we can swap the two elements
			// so that 0 is Dleks and 3 is Skeld, spawning in Dleks instead of Skeld
			AssetReference temp = AmongUsClient.Instance.ShipPrefabs[3];
			AmongUsClient.Instance.ShipPrefabs[3] = AmongUsClient.Instance.ShipPrefabs[0];
			AmongUsClient.Instance.ShipPrefabs[0] = temp;
		}

		protected override void OnEnable()
		{
			SwapMapAssets();
		}

		protected override void OnDisable()
		{
			SwapMapAssets();
		}
	}
}