using Hazel;
using System;

namespace HydraMenu.anticheat
{
	/// <summary>
	/// Base class for a check that validates a single kind of incoming RPC. Register concrete subclasses in
	/// <see cref="Anticheat.RpcHandlers"/>, keyed by the <see cref="RpcCalls"/> value they handle.
	///
	/// To add a new RPC check:
	/// <list type="number">
	/// <item>Create a subclass in <c>src/anticheat/rpc/</c> and override <see cref="GetId"/> to return the RPC it handles.</item>
	/// <item>Override <see cref="Validate"/> to read the RPC payload and call <see cref="Anticheat.Flag(PlayerControl, string, bool)"/> when something is wrong, returning <c>false</c> for an invalid RPC.</item>
	/// <item>If the RPC belongs to a net object other than <see cref="PlayerControl"/>, override <see cref="GetExpectedNetObject"/>.</item>
	/// <item>If only the host should ever send this RPC, override <see cref="IsHostOnly"/> to return <c>true</c>.</item>
	/// <item>Add an entry to <see cref="Anticheat.RpcHandlers"/> (kept sorted by RPC id).</item>
	/// </list>
	/// </summary>
	internal abstract class RpcCheck : ICheck
	{
		public bool Enabled { get; set; } = true;

		/// <summary>
		/// Validates the RPC payload. The reader is positioned at the start of the payload; the caller restores
		/// the read position afterwards, so implementations are free to consume it.
		/// </summary>
		/// <param name="player">The player the RPC was sent on behalf of, or null for net objects without an owner.</param>
		/// <param name="reader">Reader positioned at the start of the RPC payload.</param>
		/// <returns><c>true</c> if the RPC is legitimate; <c>false</c> if it should be treated as a violation.</returns>
		public virtual bool Validate(PlayerControl player, MessageReader reader)
		{
			return true;
		}

		/// <summary>The RPC this check is responsible for validating.</summary>
		public abstract RpcCalls GetId();

		/// <summary>
		/// Whether this RPC should only ever originate from the host. When true, the anticheat automatically
		/// flags any non-host player that sends it, so subclasses do not need to check for that themselves.
		/// </summary>
		public virtual bool IsHostOnly()
		{
			return false;
		}

		/// <summary>
		/// The net object this RPC is expected to arrive on. RPCs received on a different net object are treated
		/// as an exploit attempt and discarded before <see cref="Validate"/> is even called.
		/// </summary>
		public virtual Type GetExpectedNetObject()
		{
			// There are more RPCs for the PlayerControl net object than for any other net object
			// To make it easier for us, each instance of RpcCheck will be for the PlayerControl net object unless stated otherwise
			return typeof(PlayerControl);
		}
	}
}
