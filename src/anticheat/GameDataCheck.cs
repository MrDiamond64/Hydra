using AmongUs.InnerNet.GameDataMessages;
using Hazel;

namespace HydraMenu.anticheat
{
	internal abstract class GameDataCheck : ICheck
	{
		public bool Enabled { get; set; } = true;

		public virtual bool Validate(MessageReader reader)
		{
			return true;
		}

		public abstract GameDataTypes GetId();
	}
}