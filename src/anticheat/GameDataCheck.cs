using AmongUs.InnerNet.GameDataMessages;
using Hazel;

namespace HydraMenu.anticheat
{
	/// <summary>
	/// Base class for a check that validates an incoming game data message (as opposed to an RPC). Register
	/// concrete subclasses in <see cref="Anticheat.GameDataHandlers"/>, keyed by the <see cref="GameDataTypes"/>
	/// value they handle.
	///
	/// Unlike <see cref="RpcCheck"/>, game data messages are not tied to a specific player, so a check that
	/// wants to flag a player must resolve them from the payload itself (see <c>ClientReady</c> for an example).
	/// </summary>
	internal abstract class GameDataCheck : ICheck
	{
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// Validates the game data payload. The reader is positioned at the start of the payload; the caller
		/// restores the read position afterwards.
		/// </summary>
		/// <returns><c>true</c> if the message is legitimate; <c>false</c> if it should be treated as a violation.</returns>
		public virtual bool Validate(MessageReader reader)
		{
			return true;
		}

		/// <summary>The game data message type this check is responsible for validating.</summary>
		public abstract GameDataTypes GetId();
	}
}
