# Hydra Anticheat

Hydra Anticheat inspects the network traffic Among Us receives and flags players who send packets a legitimate
client never would — venting without a role that can vent, reporting a body that does not exist, sending
host-only RPCs while not the host, and so on. It is designed to *extend* the vanilla Among Us anticheat rather
than replace it, and it works whether or not you are the host of the lobby.

- **As the host**, Hydra can automatically punish detected cheaters (notify, kick, silent-kick, or ban).
- **As a non-host**, Hydra cannot punish anyone, but it will still notify you so you know who to keep an eye on.

## Baseline requirement

Hydra Anticheat assumes the server prevents **player impersonation** — that is, a client cannot send RPCs on
another player's behalf. If a server allows impersonation, a cheater could frame an innocent player (or even
frame *you* as the host, causing Hydra to act against your own lobby). The official Among Us servers enforce
impersonation checks, so this is only a concern on custom servers.

## How it works

Every incoming RPC and game data message is intercepted through Harmony patches on the relevant
`HandleRpc` / deserialize methods (see [`Anticheat.cs`](../src/anticheat/Anticheat.cs)). The flow is:

1. The message is looked up in `RpcHandlers` / `GameDataHandlers`. If there is no handler, or the anticheat or
   that specific check is disabled, the message passes through untouched.
2. For RPCs, the anticheat verifies the message arrived on the **expected net object** and that **host-only**
   RPCs really did come from the host. Either mismatch is treated as an exploit.
3. The message is handed to the check's `Validate` method. A check returns `false` and calls `Anticheat.Flag`
   when it detects a violation.
4. If a check fails and *Discard the invalid packet* is enabled, the packet is dropped so the cheat has no
   effect locally.

### Detection vs. enforcement

Detection (a check calling `Flag`) is deliberately separate from enforcement (kicking/banning). This matters
because it lets Hydra warn on low-confidence signals without acting on them, and it keeps the punishment policy
in one place.

Enforcement only ever happens when **you are the host**, and it is gated behind a **strike threshold** (see
below), so a single ambiguous detection does not immediately punish a player.

## Strike threshold (avoiding false positives)

*Strikes before punishment* controls how many times a single player must be flagged during a game before Hydra
punishes them:

- **1** — punish on the first flag (the original behaviour).
- **2+** — the player must trip checks multiple times before any punishment lands.

Because real cheats tend to trip many checks in quick succession while an innocent player might only produce a
single borderline detection (lag, an unusual-but-legal action, an edge case a check hasn't accounted for yet),
a threshold of 2–3 is a good way to keep catching cheaters while making it very unlikely that an honest player
is kicked or banned by mistake. When a threshold above 1 is set, notifications show each player's running count
(e.g. `strike 2/3`) so you can watch an escalating cheater before the punishment triggers.

Strikes are tracked per player for the current game only. They are automatically cleared when a new game starts
and when you leave a lobby, so detections never carry over between rounds.

## Punishment types

| Punishment | Effect |
| --- | --- |
| Notify only | No punishment — you are only notified. Works as a non-host too. |
| Kick | Kicks the player from the lobby. |
| Silent kick (fake timeout) | Removes the player disguised as a connection timeout, so other players see them "leave" due to an error rather than a kick. Falls back to a normal kick before the game has started. |
| Ban from lobby | Kicks and bans the player so they cannot rejoin. |

When the punishment is set to *Ban*, an optional **orbital strike** can play on the cheater just before the ban
lands — a beam drops from the sky onto them for a fraction of a second. It is purely cosmetic: the strike's
completion is what triggers the actual ban, and if the visual ever fails to initialise the ban still happens, so
the outcome is identical whether the effect is on or off. Toggle it in the Anticheat tab (only shown while the
punishment is set to Ban). See [`OrbitalStrike.cs`](../src/anticheat/OrbitalStrike.cs) — it is a good template
for hanging other cosmetic effects off anticheat events.

## Adding a new check

Checks are small, self-contained classes. See [`RpcCheck`](../src/anticheat/RpcCheck.cs) and
[`GameDataCheck`](../src/anticheat/GameDataCheck.cs) for the full contract. In short:

### An RPC check

1. Create a class in `src/anticheat/rpc/` that extends `RpcCheck`.
2. Override `GetId()` to return the `RpcCalls` value it handles.
3. Override `Validate(PlayerControl player, MessageReader reader)`. Read the payload, and when something is
   wrong call `Anticheat.Flag(player, "...reason...")` and return `false`.
4. Override `GetExpectedNetObject()` if the RPC is not on `PlayerControl` (e.g. `PlayerPhysics` for venting).
5. Override `IsHostOnly()` to return `true` if only the host should ever send this RPC — the anticheat then
   flags any non-host sender for you automatically.
6. Register it in `Anticheat.RpcHandlers`, keeping the dictionary sorted by RPC id.

```csharp
internal class ExampleCheck : RpcCheck
{
    public override bool Validate(PlayerControl player, MessageReader reader)
    {
        if (/* something illegitimate */)
        {
            Anticheat.Flag(player, $"{player.Data.PlayerName} did something they shouldn't have.");
            return false;
        }

        return true;
    }

    public override RpcCalls GetId() => RpcCalls.Example;
}
```

### A game data check

Same idea, but extend `GameDataCheck`, override `GetId()` to return a `GameDataTypes` value, override
`Validate(MessageReader reader)`, and register it in `Anticheat.GameDataHandlers`. Game data messages are not
tied to a player, so resolve the player from the payload yourself (see
[`ClientReady`](../src/anticheat/gamedata/ClientReady.cs) for an example).

### Guidance for writing checks

- **Prefer false negatives over false positives.** A missed cheat is annoying; a wrongly banned player is
  worse. When a legitimate edge case could trip your check, let it pass.
- **Watch for self-flagging.** `Anticheat.Flag` already ignores the local player, but keep it in mind on
  servers without impersonation protection.
- **Restore nothing yourself.** The anticheat saves and restores the reader position around `Validate`, so you
  can read freely.
- **Pass `shouldPunish: false`** to `Flag` for low-confidence detections that should only ever notify.
