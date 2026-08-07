using HarmonyLib;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace HydraMenu.features
{
	// client-side minimap overlay, draws players/bodies/tracers on the in-game map
	//
	// no network traffic at all - just reads positions the game already replicated to us
	// and drops local sprites on top, so it's invisible to server-side anticheat
	//
	// points are parented under HerePoint so they inherit its coord space + show/hide.
	// we copy HerePoint's layer and a de-tinted material copy so our stuff isn't clipped
	// behind the map, but sorting comes from ResolveSorting - HerePoint sits below the
	// sabotage buttons, so matching its order would put us under them too
	internal class Minimap
	{
		public static bool Enabled { get; set; } = false;
		// body 'X' markers, their kill timers, and the kill burst rings all ride on this one toggle -
		// they're the same idea (where someone died) drawn three ways, so splitting them just made
		// for a half-lit map
		public static bool ShowDeadBodies { get; set; } = true;
		public static bool ShowTracers { get; set; } = true;
		// off = raw sampled points, no snapping/lanes/rounding
		public static bool BundleTracers { get; set; } = false;

		// seconds of movement history a tracer shows
		public static float TrailSeconds { get; set; } = 30f;

		// sized relative to the game's own HerePoint marker
		private const float PointScale = 0.5f;
		private const float LocalPointScale = 0.62f;
		// team-colored ring behind each point, relative to the point
		private const float OutlineScale = 1.5f;
		// body markers use a generated 'X' sprite
		private const float BodyMarkerScale = 0.85f;

		// kill ring animation
		private const float BurstDuration = 5.4f;
		// slow creep before the exponential blowout kicks in
		private const float BurstSlowDuration = 3f;
		// how much of the full span the slow phase covers
		private const float BurstSlowShare = 0.12f;
		// final ring size - big enough to sweep past any map's edges
		private const float BurstMaxWorldSpan = 90f;
		private const float BurstOpacity = 0.5f; // peak alpha, kept subtle
		private const float BurstAcceleration = 2.4f; // expansion speed ramp
		private const int MaxBursts = 8;

		// sort order relative to the top of the map's renderer stack (see ResolveSorting).
		// tracers sit below everything so markers always read on top
		private const int TracerSortOrder = 20;
		private const int OutlineSortOrder = 30;
		private const int PointSortOrder = 31;
		private const int BodyOutlineSortOrder = 32;
		private const int BodySortOrder = 33;
		private const int BurstSortOrder = 34;
		private const int TimerSortOrder = 35;

		// kill timer under each body marker
		private const float TimerFontSize = 4f;
		private const float TimerScaleFactor = 0.5f; // text height vs marker
		private const float TimerOffsetFactor = 0.78f; // how far below the 'X'
		private const float TimerOutlineWidth = 0.22f; // for legibility over map art
		// tracer width relative to a player point
		private const float TracerWidthFactor = 0.3f;
		private const float TracerOpacity = 0.95f;

		// movement sampling - kept coarse since every vertex crosses IL2CPP
		private const float SampleInterval = 0.2f;
		// absolute ceiling on stored samples, guards against a runaway TrailSeconds
		private const int MaxSamplesCeiling = 1024;
		// bigger than this between samples = teleport (vent, ladder, etc)
		private const float VentJumpDistance = 2f;

		// derived from the trail length so the whole slider range actually works - a fixed
		// cap silently truncated long trails. vent hops add off-interval samples, hence the headroom
		private static int MaxSamples => Mathf.Min(MaxSamplesCeiling, Mathf.CeilToInt(TrailSeconds / SampleInterval) + 8);

		// subway-style lane bundling
		private const float LaneSpacingFactor = 0.5f; // lane gap vs a player point
		private const float GroupRadiusFactor = 4f; // lattice cell size (corridor width)
		private const int SmoothWindow = 1; // vertices smoothed either side
		private const int SmoothIterations = 1; // corner rounding passes
		private const float BundleInterval = 0.2f; // seconds between rebuilds
		// body scanning is pricey and bodies don't move, so throttle it
		private const float BodyScanInterval = 0.25f;

		// crewmates white, impostors red
		private static readonly Color CrewOutline = Color.white;
		private static readonly Color ImpostorOutline = new Color(0.86f, 0.12f, 0.12f);
		private static readonly Color BurstColor = Color.white;

		// marks where a kill happened, expanding ring
		private struct KillBurst
		{
			public Vector2 Position;
			public float Start;
			// pooled ring slot this burst owns for its whole lifetime. bursts expire from the
			// front, so indexing rings by list position made survivors inherit a neighbour's
			// ring mid-animation and visibly snap
			public int Ring;
		}

		private static readonly List<KillBurst> bursts = new List<KillBurst>();
		private static readonly List<SpriteRenderer> burstRings = new List<SpriteRenderer>();

		// per-player dot, keyed by player id
		private static readonly Dictionary<byte, SpriteRenderer> playerDots = new Dictionary<byte, SpriteRenderer>();
		// team-colored ring behind each non-local dot
		private static readonly Dictionary<byte, SpriteRenderer> playerOutlines = new Dictionary<byte, SpriteRenderer>();
		// 'X' markers for unreported bodies, keyed by dead player id
		private static readonly Dictionary<byte, SpriteRenderer> bodyDots = new Dictionary<byte, SpriteRenderer>();
		// kill timer text under each body
		private static readonly Dictionary<byte, TextMeshPro> bodyTimers = new Dictionary<byte, TextMeshPro>();
		// death time on the trail clock, so timers freeze during meetings
		private static readonly Dictionary<byte, float> killTimes = new Dictionary<byte, float>();
		// tracer line per player
		private static readonly Dictionary<byte, LineRenderer> tracers = new Dictionary<byte, LineRenderer>();

		// ids actually drawn this frame. the render loop only walks players the game still lists, so
		// anything it skips (dead, disconnected, or gone from AllPlayerControls entirely) would keep
		// whatever it last drew - this is what hides the leftovers. tracked separately from dots
		// because a dead player keeps their trail but loses their dot
		private static readonly HashSet<byte> renderedIds = new HashSet<byte>();
		private static readonly HashSet<byte> renderedTracers = new HashSet<byte>();

		// movement history, sampled even with the map closed so it's ready on open
		private static readonly Dictionary<byte, List<TrailSample>> trails = new Dictionary<byte, List<TrailSample>>();

		// last known position before a meeting teleport, so we can freeze there
		private static readonly Dictionary<byte, Vector2> lastPositions = new Dictionary<byte, Vector2>();

		private struct TrailSample
		{
			public Vector2 Position;
			public float Time;
			// true if this was a teleport (vent) rather than walking
			public bool Jump;
		}

		// lane-offset tracer paths, rebuilt on a timer not every frame
		private static readonly Dictionary<byte, Vector3[]> bundledPaths = new Dictionary<byte, Vector3[]>();
		private static readonly Dictionary<byte, int> tracerVersion = new Dictionary<byte, int>();
		private static int bundleVersion = 0;
		private static float lastBundleTime = -999f;
		private static float lastBodyScan = -999f;

		// reused across rebuilds - the per-player path arrays below still allocate each pass
		private static readonly List<byte> activeIds = new List<byte>();
		private static readonly Dictionary<long, int> cellOccupancy = new Dictionary<long, int>();
		// each player's route as lattice cells
		private static readonly Dictionary<byte, List<long>> routeCells = new Dictionary<byte, List<long>>();
		// lane bit per waypoint - doubling back gets a second bit for its own lane
		private static readonly Dictionary<byte, List<int>> routeBits = new Dictionary<byte, List<int>>();
		// waypoints locked out of corner rounding (vent hop endpoints)
		private static readonly Dictionary<byte, List<bool>> routeLocks = new Dictionary<byte, List<bool>>();
		// visits per cell for the player currently being processed
		private static readonly Dictionary<long, int> visitCounts = new Dictionary<long, int>();

		// paused during meetings/ejections so that time doesn't clear trails
		private static float trailClock = 0f;
		private static ShipStatus lastShip;
		// detects meeting/ejection end to clear tracers
		private static bool wasPaused = false;

		// the table teleport happens inside PlayerControl.StartMeeting, several frames before the
		// MeetingHud object exists - freezing on the hud alone let those teleported positions leak
		// onto the map. set the moment the meeting is announced instead, dropped again once the hud
		// takes over the freeze
		private static bool meetingStarting = false;
		private static float meetingStartTime = -999f;
		// safety valve: if the meeting never materializes (rejected/desynced RPC) don't freeze forever
		private const float MeetingStartTimeout = 5f;

		// even that isn't airtight - remote players are moved by their own net transforms, so a table
		// position can land in our samples before anything tells us a meeting is happening. so on
		// freeze we also rewind each player past any movement they couldn't have walked, within this
		// many seconds of trail time. a vent hop right before a meeting gets rewound too, which just
		// means the frozen map shows where they vented from
		private const float MeetingTeleportWindow = 1f;
		// per-sample distance a player can't cover on foot (max speed is ~3.5 units/s)
		private const float MeetingTeleportStep = 1.5f;

		// HerePoint we parented to - stale after map recreation, triggers a rebuild
		private static Transform dotParent;

		// de-tinted copy of HerePoint's material, shared by everything we draw
		private static Material pointMaterial;
		// generated sprites, built once and tinted per-renderer
		private static Sprite circleSprite;
		private static Sprite xSprite;
		private static Sprite xOutlineSprite;
		private static Sprite ringSprite;

		// sorting layer + order our bumps are applied on top of. HerePoint is NOT the top of the
		// map's stack - the sabotage buttons (InfectedOverlay.allButtons) and the room door/special
		// sprites are authored above it, so anchoring to HerePoint's own order drew us underneath.
		// the real orders live in the map prefab, so this is resolved at runtime
		private static int sortingLayer;
		private static int sortingBase;

		// Unity clamps sortingOrder to short, leave room for our bumps
		private static readonly int SortingCeiling = 32767 - 128;

		// the overlays populate after the map first opens, so a one-shot resolve misses them.
		// cheap enough to redo on a timer while the map is on screen
		private const float SortingScanInterval = 0.5f;
		private static float lastSortingScan = -999f;
		private static bool dumpPending = false;
		// set true to dump the full map/marker render inventory once per map open
		private static readonly bool Diagnostics = false;

		// z we draw at, in dotParent's local space. the map's Unlit/*Shader materials write and
		// test depth, so draw order never mattered - the sabotage buttons simply sit nearer the
		// camera and the depth test was discarding our fragments. resolved per map, since the
		// prefab z values differ between maps
		private static float markerDepth = -1f;
		// how far in front of the frontmost map-surface element to sit
		private const float DepthMargin = 1f;

		// every renderer we own, with the bump it was created at, so a later re-resolve can
		// push new sorting onto objects that already exist
		private static readonly List<KeyValuePair<Renderer, int>> ownedRenderers = new List<KeyValuePair<Renderer, int>>();

		private static void ResolveSorting(MapBehaviour map, SpriteRenderer template)
		{
			int previousLayer = sortingLayer;
			int previousBase = sortingBase;

			sortingLayer = template.sortingLayerID;
			sortingBase = template.sortingOrder;

			int topLayer = SortingLayer.GetLayerValueFromID(sortingLayer);
			string winner = "HerePoint";
			int seen = 0;

			// the generic Renderer sweep came back empty - abstract base types don't resolve through
			// Il2CppInterop's GetComponentsInChildren<T>, so it silently found nothing and left us at
			// HerePoint's own order. walk MapBehaviour's concrete references instead, which is exactly
			// where the interactive sabotage elements live
			InfectedOverlay infected = map.infectedOverlay;
			if(infected != null)
			{
				if(infected.allButtons != null)
				{
					foreach(ButtonBehavior button in infected.allButtons)
					{
						if(button != null) Consider(button.spriteRenderer, ref topLayer, ref winner, ref seen);
					}
				}

				if(infected.rooms != null)
				{
					foreach(MapRoom room in infected.rooms)
					{
						if(room == null) continue;

						Consider(room.door, ref topLayer, ref winner, ref seen);
						Consider(room.special, ref topLayer, ref winner, ref seen);
					}
				}
			}

			if(map.countOverlay != null && map.countOverlay.SabotageText != null)
			{
				Consider(map.countOverlay.SabotageText.GetComponent<MeshRenderer>(), ref topLayer, ref winner, ref seen);
			}

			Consider(map.TrackedHerePoint, ref topLayer, ref winner, ref seen);

			// SpriteRenderer is concrete, so unlike the Renderer sweep this one returns results
			ScanRoot(map.transform, ref topLayer, ref winner, ref seen);

			sortingBase = Mathf.Min(sortingBase, SortingCeiling) + 1;

			float previousDepth = markerDepth;
			ResolveDepth(map, template);

			if(sortingLayer == previousLayer && sortingBase == previousBase && Mathf.Approximately(markerDepth, previousDepth)) return;

			Hydra.Log.LogInfo($"Minimap sorting resolved above '{winner}' -> layer '{SortingLayer.IDToName(sortingLayer)}' order {sortingBase} depth {markerDepth:F3} (saw {seen} renderers)");
			ApplySorting();
		}

		// sit just in front of the nearest thing drawn on the map surface. deliberately scoped to
		// the overlays and HerePoint - the close button sits far nearer the camera and we must
		// stay behind it rather than covering it
		private static void ResolveDepth(MapBehaviour map, SpriteRenderer template)
		{
			Transform parent = template.transform.parent;
			if(parent == null) return;

			float frontZ = template.transform.position.z;

			InfectedOverlay infected = map.infectedOverlay;
			if(infected != null)
			{
				if(infected.allButtons != null)
				{
					foreach(ButtonBehavior button in infected.allButtons)
					{
						if(button != null) ConsiderDepth(button.spriteRenderer, ref frontZ);
					}
				}

				if(infected.rooms != null)
				{
					foreach(MapRoom room in infected.rooms)
					{
						if(room == null) continue;

						ConsiderDepth(room.door, ref frontZ);
						ConsiderDepth(room.special, ref frontZ);
					}
				}
			}

			if(map.taskOverlay != null)
			{
				foreach(SpriteRenderer icon in map.taskOverlay.transform.GetComponentsInChildren<SpriteRenderer>(true))
				{
					ConsiderDepth(icon, ref frontZ);
				}
			}

			// world -> dotParent local, so a scaled parent still lands us at the right depth
			markerDepth = parent.InverseTransformPoint(new Vector3(0f, 0f, frontZ - DepthMargin)).z;
		}

		private static void ConsiderDepth(Renderer renderer, ref float frontZ)
		{
			if(renderer == null || renderer.gameObject.name.StartsWith("HydraMinimap")) return;

			float z = renderer.transform.position.z;
			if(z < frontZ) frontZ = z;
		}

		private static void ScanRoot(Transform root, ref int topLayer, ref string winner, ref int seen)
		{
			if(root == null) return;

			foreach(SpriteRenderer renderer in root.GetComponentsInChildren<SpriteRenderer>(true))
			{
				Consider(renderer, ref topLayer, ref winner, ref seen);
			}
		}

		private static void Consider(Renderer renderer, ref int topLayer, ref string winner, ref int seen)
		{
			// skip our own markers or the base would ratchet upward on every scan
			if(renderer == null || renderer.gameObject.name.StartsWith("HydraMinimap")) return;
			seen++;

			int layer = SortingLayer.GetLayerValueFromID(renderer.sortingLayerID);
			if(layer < topLayer) return;

			// a higher layer always wins regardless of order, so restart the order search there
			if(layer > topLayer)
			{
				topLayer = layer;
				sortingLayer = renderer.sortingLayerID;
				sortingBase = renderer.sortingOrder;
				winner = renderer.gameObject.name;
				return;
			}

			if(renderer.sortingOrder > sortingBase)
			{
				sortingBase = renderer.sortingOrder;
				winner = renderer.gameObject.name;
			}
		}

		// one-shot inventory of everything the map draws, next to everything we draw. sorting
		// order alone did not explain the draw order, so this reports the other things that can:
		// unity layer (ie. a different camera), z depth, shader and render queue
		private static void DumpSortingDiagnostics(MapBehaviour map)
		{
			Hydra.Log.LogInfo("=== Minimap sorting diagnostics ===");

			Camera main = Camera.main;
			if(main != null)
			{
				Hydra.Log.LogInfo($"main camera '{main.name}' depth={main.depth} cullingMask={main.cullingMask} ortho={main.orthographic} sortAxis={main.transparencySortAxis} sortMode={main.transparencySortMode}");
			}

			foreach(SpriteRenderer renderer in map.transform.GetComponentsInChildren<SpriteRenderer>(true))
			{
				LogRenderer("map", renderer);
			}

			InfectedOverlay infected = map.infectedOverlay;
			if(infected != null && infected.allButtons != null)
			{
				foreach(ButtonBehavior button in infected.allButtons)
				{
					if(button != null) LogRenderer("sabButton", button.spriteRenderer);
				}
			}

			for(int i = 0; i < ownedRenderers.Count; i++)
			{
				LogRenderer("ours", ownedRenderers[i].Key);
			}

			Hydra.Log.LogInfo("=== end Minimap diagnostics ===");
		}

		private static void LogRenderer(string tag, Renderer renderer)
		{
			if(renderer == null) return;

			Material material = renderer.sharedMaterial;
			string shader = material != null && material.shader != null ? material.shader.name : "<none>";
			int queue = material != null ? material.renderQueue : -1;
			GameObject go = renderer.gameObject;

			Hydra.Log.LogInfo($"[{tag}] {HierarchyPath(renderer.transform)} | unityLayer={LayerMask.LayerToName(go.layer)}({go.layer}) | sortLayer={SortingLayer.IDToName(renderer.sortingLayerID)} order={renderer.sortingOrder} | z={renderer.transform.position.z:F3} | active={go.activeInHierarchy} enabled={renderer.enabled} | shader={shader} queue={queue}");
		}

		private static string HierarchyPath(Transform transform)
		{
			string path = transform.name;

			for(Transform current = transform.parent; current != null; current = current.parent)
			{
				path = current.name + "/" + path;
			}

			return path;
		}

		// re-stamps everything we've already created, since the base can move after they exist
		private static void ApplySorting()
		{
			for(int i = ownedRenderers.Count - 1; i >= 0; i--)
			{
				Renderer renderer = ownedRenderers[i].Key;

				if(renderer == null)
				{
					ownedRenderers.RemoveAt(i);
					continue;
				}

				renderer.sortingLayerID = sortingLayer;
				renderer.sortingOrder = sortingBase + ownedRenderers[i].Value;
			}
		}

		private static void Own(Renderer renderer, int bump)
		{
			ownedRenderers.Add(new KeyValuePair<Renderer, int>(renderer, bump));
		}

		// true during a meeting/ejection - positions are meaningless then. covers the gap between the
		// meeting being announced (players teleport to the table right there) and the hud spawning
		private static bool IsPaused()
		{
			return IsMeetingStarting() || MeetingHud.Instance != null || ExileController.Instance != null;
		}

		// the announcement only has to hold the freeze until the hud/exile screen exists to take over
		private static bool IsMeetingStarting()
		{
			if(!meetingStarting) return false;

			// ...or until it's clear the meeting never arrived, so a rejected RPC can't freeze us forever
			if(MeetingHud.Instance != null || ExileController.Instance != null || Time.time - meetingStartTime > MeetingStartTimeout)
			{
				meetingStarting = false;
			}

			return meetingStarting;
		}

		// called from both the hud tick and the map render, so the freeze snapshot is taken the first
		// moment either of them notices - the render runs in FixedUpdate and can beat HudManager.Update
		// to the frame the teleport lands on
		private static void UpdatePauseState()
		{
			bool paused = IsPaused();

			// meeting just ended, everyone got teleported back - wipe old history
			if(wasPaused && !paused) ClearHistory();
			else if(paused && !wasPaused) FreezeForMeeting();

			wasPaused = paused;
		}

		// drops everything the players did between the meeting teleport and us noticing the meeting.
		// scans back over the recent tail of each trail for a step no one could have walked and cuts
		// there, so both an instant snap to the table and a net transform sliding into it are removed
		private static void FreezeForMeeting()
		{
			foreach(var entry in trails)
			{
				List<TrailSample> history = entry.Value;

				int cut = -1;
				for(int i = history.Count - 1; i > 0; i--)
				{
					if(trailClock - history[i].Time > MeetingTeleportWindow) break;

					if((history[i].Position - history[i - 1].Position).sqrMagnitude > MeetingTeleportStep * MeetingTeleportStep) cut = i;
				}

				// the offending sample is the teleport destination, so drop it along with the rest
				if(cut >= 0) history.RemoveRange(cut, history.Count - cut);

				// freeze on what's left rather than on whatever the sampler last saw
				if(history.Count > 0) lastPositions[entry.Key] = history[history.Count - 1].Position;
				else lastPositions.Remove(entry.Key);
			}

			// tracers have to be rebuilt off the truncated history or the leg to the table stays drawn
			bundledPaths.Clear();
			lastBundleTime = -999f;
		}

		// earliest client-side signal of a meeting - runs before the teleport to the table, on both
		// the host and everyone receiving the StartMeeting RPC
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.StartMeeting))]
		private static class MeetingStartPatch
		{
			static void Prefix()
			{
				meetingStarting = true;
				meetingStartTime = Time.time;
			}
		}

		// ticks the clock each frame outside meetings, resets history on new game / meeting end
		[HarmonyPatch(typeof(HudManager), nameof(HudManager.Update))]
		private static class TrailClockPatch
		{
			static void Postfix()
			{
				if(ShipStatus.Instance != lastShip)
				{
					lastShip = ShipStatus.Instance;
					ClearHistory();
					trailClock = 0f;
					meetingStarting = false;
				}

				UpdatePauseState();

				if(ShipStatus.Instance != null && !wasPaused)
				{
					trailClock += Time.deltaTime;
				}
			}
		}

		// records movement continuously so tracers are ready the moment the map opens
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.FixedUpdate))]
		private static class TrailSamplerPatch
		{
			static void Postfix(PlayerControl __instance)
			{
				if(!Enabled || ShipStatus.Instance == null || IsPaused()) return;

				NetworkedPlayerInfo data = __instance.Data;
				if(data == null) return;

				// a player who left takes their trail with them, nobody's going to look for it
				if(data.Disconnected)
				{
					ForgetTrail(data.PlayerId);
					return;
				}

				// death freezes the trail exactly where it is - no new samples, and deliberately no
				// pruning either, so it survives the meeting whole instead of quietly ageing out from
				// under the clock. ClearHistory wipes it at the end along with everyone else's
				if(data.IsDead) return;

				byte id = data.PlayerId;

				// keep last position fresh even with tracers off, for meeting freeze
				lastPositions[id] = __instance.transform.position;

				// history is recorded even with tracers off - the meeting freeze rewinds through it
				if(!trails.TryGetValue(id, out List<TrailSample> history))
				{
					history = new List<TrailSample>();
					trails[id] = history;
				}

				Vector2 position = __instance.transform.position;

				// catch vent jumps immediately so the line lands exactly on the vent mouths
				bool jump = false;
				if(history.Count > 0)
				{
					Vector2 last = history[history.Count - 1].Position;
					jump = (position - last).sqrMagnitude > VentJumpDistance * VentJumpDistance;
				}

				// throttle so we don't store a sample every physics step
				if(history.Count == 0 || jump || trailClock - history[history.Count - 1].Time >= SampleInterval)
				{
					history.Add(new TrailSample { Position = position, Time = trailClock, Jump = jump });
				}

				PruneHistory(history);
			}
		}

		// where to draw a player - frozen at last real position during meetings, purely visual.
		// false during a meeting for anyone we have no frozen position for: their live position is
		// the meeting table, so drawing it is exactly what we're trying to avoid - drop the marker
		private static bool TryGetRenderPosition(PlayerControl player, byte playerId, out Vector2 position)
		{
			if(IsPaused())
			{
				bool known = lastPositions.TryGetValue(playerId, out position);
				return known;
			}

			position = player.transform.position;
			return true;
		}

		private static void ClearHistory()
		{
			trails.Clear();
			lastPositions.Clear();
			bundledPaths.Clear();
			tracerVersion.Clear();
			routeCells.Clear();
			routeBits.Clear();
			routeLocks.Clear();
			bursts.Clear();
			killTimes.Clear();

			// bodies are gone once the meeting ends, drop the frozen markers too
			HideEach(bodyDots);
			HideTimers();
			lastBundleTime = -999f;
		}

		// drops every per-player cache derived from a trail, so nothing can redraw it later. the
		// trails removal doubles as the early out - this runs each physics step per departed player
		private static void ForgetTrail(byte playerId)
		{
			if(!trails.Remove(playerId)) return;

			lastPositions.Remove(playerId);
			bundledPaths.Remove(playerId);
			tracerVersion.Remove(playerId);
			routeCells.Remove(playerId);
			routeBits.Remove(playerId);
			routeLocks.Remove(playerId);

			// lane assignments are relative to who else is out there, so the survivors shift
			lastBundleTime = -999f;
		}

		private static void PruneHistory(List<TrailSample> history)
		{
			float cutoff = trailClock - TrailSeconds;

			int drop = 0;
			while(drop < history.Count && history[drop].Time < cutoff) drop++;

			// hard cap too, so a huge TrailSeconds can't blow up vertex count
			if(history.Count - drop > MaxSamples) drop = history.Count - MaxSamples;

			if(drop > 0) history.RemoveRange(0, drop);
		}

		// runs locally for every kill, so we see where kills happen for free
		[HarmonyPatch(typeof(PlayerControl), nameof(PlayerControl.MurderPlayer))]
		private static class KillBurstPatch
		{
			static void Postfix(PlayerControl target, MurderResultFlags resultFlags)
			{
				if(!Enabled) return;
				// also fires for declined kills. checking the flag rather than IsDead matters for
				// a repeat murder against an already-dead target, which would otherwise reset
				// that body's timer and drop a second burst
				if(!resultFlags.HasFlag(MurderResultFlags.Succeeded)) return;
				if(target == null || target.Data == null) return;

				// timed off the trail clock so it stops counting during meetings
				killTimes[target.Data.PlayerId] = trailClock;

				if(!ShowDeadBodies) return;

				if(bursts.Count >= MaxBursts) bursts.RemoveAt(0);
				bursts.Add(new KillBurst { Position = target.transform.position, Start = Time.time, Ring = ClaimRingSlot() });
			}
		}

		// only runs while the map is actually open - perfect hook point for drawing
		[HarmonyPatch(typeof(MapBehaviour), nameof(MapBehaviour.FixedUpdate))]
		private static class MapUpdatePatch
		{
			static void Postfix(MapBehaviour __instance)
			{
				Render(__instance);
			}
		}

		private static void Render(MapBehaviour map)
		{
			// before anything reads a position this frame
			if(Enabled && ShipStatus.Instance != null) UpdatePauseState();

			if(!Enabled || map == null || !map.IsOpen || map.HerePoint == null || ShipStatus.Instance == null)
			{
				HideAll();
				return;
			}

			SpriteRenderer template = map.HerePoint;
			Transform parent = template.transform.parent;

			// map gets destroyed/recreated between games - stale parent means rebuild
			if(dotParent != parent)
			{
				playerDots.Clear();
				playerOutlines.Clear();
				bodyDots.Clear();
				bodyTimers.Clear();
				tracers.Clear();
				tracerVersion.Clear();
				burstRings.Clear();
				// force lanes to recalc against the new map
				lastBundleTime = -999f;
				ownedRenderers.Clear();
				dotParent = parent;
				lastSortingScan = -999f;
				dumpPending = true;
			}

			if(Time.time - lastSortingScan >= SortingScanInterval)
			{
				lastSortingScan = Time.time;
				ResolveSorting(map, template);
			}

			float scale = ShipStatus.Instance.MapScale;
			// matches our circle sprite to HerePoint's on-screen size
			float pointBaseScale = MatchedScale(template, GetCircleSprite());
			byte localId = PlayerControl.LocalPlayer != null ? PlayerControl.LocalPlayer.PlayerId : byte.MaxValue;

			// point diameter in map-local units, everything else scales off this
			float pointSize = Mathf.Abs(template.transform.localScale.x) * pointBaseScale * PointScale;
			float laneSpacing = pointSize * LaneSpacingFactor;
			// LineRenderer width ignores transform scale, so convert out manually
			float tracerWidth = pointSize * TracerWidthFactor * Mathf.Abs(parent.lossyScale.x);

			if(ShowTracers) RebuildBundles(scale, laneSpacing);

			renderedIds.Clear();
			renderedTracers.Clear();

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				NetworkedPlayerInfo data = player.Data;

				// anything we skip below stays out of the rendered sets, so the sweep after the
				// loop hides it - no need to hide per-case here

				// skip players who never joined or already left
				if(data == null || data.Disconnected) continue;

				Color color = GetPlayerColor(data);

				// the trail outlives its owner - it's drawn before the dead check so a corpse still
				// shows where they walked, right up until the meeting clears everyone's
				if(ShowTracers && UpdateTracer(data.PlayerId, template, parent, color, tracerWidth))
				{
					renderedTracers.Add(data.PlayerId);
				}

				// dead players get an 'X' body marker instead of a dot, drawn separately
				if(data.IsDead) continue;

				// nothing safe to draw them at (see TryGetRenderPosition)
				if(!TryGetRenderPosition(player, data.PlayerId, out Vector2 position)) continue;

				renderedIds.Add(data.PlayerId);

				bool isLocal = data.PlayerId == localId;

				SpriteRenderer dot = GetOrCreatePlayerPoint(data.PlayerId, template, parent, isLocal);

				// same transform the game uses to place HerePoint
				dot.transform.localPosition = new Vector3(position.x / scale, position.y / scale, markerDepth);

				dot.color = color;
				dot.transform.localScale = template.transform.localScale * pointBaseScale * (isLocal ? LocalPointScale : PointScale);
				Show(dot.gameObject);

				// non-local dots get a team-colored ring (white crew, red imp)
				if(!isLocal && playerOutlines.TryGetValue(data.PlayerId, out SpriteRenderer outline) && outline != null)
				{
					outline.color = RoleManager.IsImpostorRole(data.RoleType) ? ImpostorOutline : CrewOutline;
					Show(outline.gameObject);
				}
			}

			HideUnrendered();

			RenderBodies(map, template, parent, scale);
			RenderBursts(template, parent, scale);

			// deferred until our own objects exist so the dump can compare them side by side
			if(Diagnostics && dumpPending && ownedRenderers.Count > 0)
			{
				dumpPending = false;
				DumpSortingDiagnostics(map);
			}
		}

		// expanding kill rings - starts moving, accelerates outward while fading
		private static void RenderBursts(SpriteRenderer template, Transform parent, float scale)
		{
			// toggled off mid-animation - drop the live rings too, not just future ones
			if(!ShowDeadBodies)
			{
				if(bursts.Count > 0) bursts.Clear();
				HideBurstRings();
				return;
			}

			for(int i = bursts.Count - 1; i >= 0; i--)
			{
				if(Time.time - bursts[i].Start >= BurstDuration) bursts.RemoveAt(i);
			}

			for(int i = 0; i < bursts.Count; i++)
			{
				SpriteRenderer ring = GetOrCreateBurstRing(bursts[i].Ring, template, parent);

				float elapsed = Time.time - bursts[i].Start;

				float growth;
				float fade;

				if(elapsed < BurstSlowDuration)
				{
					// slow phase: steady creep, full opacity so it stays readable
					growth = BurstSlowShare * (elapsed / BurstSlowDuration);
					fade = 1f;
				}
				else
				{
					// fast phase: exponential blowout while fading
					float progress = Mathf.Clamp01((elapsed - BurstSlowDuration) / (BurstDuration - BurstSlowDuration));
					float curve = (Mathf.Exp(BurstAcceleration * progress) - 1f) / (Mathf.Exp(BurstAcceleration) - 1f);

					growth = BurstSlowShare + (1f - BurstSlowShare) * curve;
					fade = 1f - progress;
				}

				Vector2 position = bursts[i].Position;
				ring.transform.localPosition = new Vector3(position.x / scale, position.y / scale, markerDepth);
				// ring sprite is one unit across, so scale = diameter directly
				ring.transform.localScale = Vector3.one * (BurstMaxWorldSpan / scale) * growth;
				ring.color = new Color(BurstColor.r, BurstColor.g, BurstColor.b, BurstOpacity * fade);

				Show(ring.gameObject);
			}

			// hide any pooled ring no live burst is holding
			for(int slot = 0; slot < burstRings.Count; slot++)
			{
				if(burstRings[slot] != null && !IsRingSlotTaken(slot)) Hide(burstRings[slot].gameObject);
			}
		}

		// first pool slot no live burst owns. MaxBursts slots for at most MaxBursts bursts,
		// and the oldest is evicted before we get here, so one is always free
		private static int ClaimRingSlot()
		{
			for(int slot = 0; slot < MaxBursts; slot++)
			{
				if(!IsRingSlotTaken(slot)) return slot;
			}

			return 0;
		}

		private static bool IsRingSlotTaken(int slot)
		{
			for(int i = 0; i < bursts.Count; i++)
			{
				if(bursts[i].Ring == slot) return true;
			}

			return false;
		}

		private static SpriteRenderer GetOrCreateBurstRing(int index, SpriteRenderer template, Transform parent)
		{
			while(burstRings.Count <= index) burstRings.Add(null);

			if(burstRings[index] == null)
			{
				burstRings[index] = CreatePoint($"HydraMinimapBurst_{index}", GetRingSprite(), template, parent, BurstSortOrder);
			}

			return burstRings[index];
		}

		private static void RenderBodies(MapBehaviour map, SpriteRenderer template, Transform parent, float scale)
		{
			if(!ShowDeadBodies)
			{
				HideEach(bodyDots);
				HideTimers();
				return;
			}

			// bodies get destroyed on meeting start, so skip scanning and just freeze markers
			if(IsPaused()) return;

			// FindObjectsOfType is expensive and bodies don't move, so throttle it
			if(Time.time - lastBodyScan < BodyScanInterval) return;
			lastBodyScan = Time.time;

			// track which bodies still exist to hide ones that got reported
			HashSet<byte> seen = new HashSet<byte>();

			foreach(DeadBody body in Object.FindObjectsOfType<DeadBody>())
			{
				seen.Add(body.ParentId);

				SpriteRenderer marker = GetOrCreateBodyMarker(body.ParentId, template, parent);

				Vector2 position = body.TruePosition;
				marker.transform.localPosition = new Vector3(position.x / scale, position.y / scale, markerDepth);

				// tint the 'X' with the dead player's color
				NetworkedPlayerInfo owner = GameData.Instance != null ? GameData.Instance.GetPlayerById(body.ParentId) : null;
				Color color = owner != null ? GetPlayerColor(owner) : Color.white;
				marker.color = color;
				Show(marker.gameObject);

				UpdateBodyTimer(body.ParentId, map, template, parent, marker, color);
			}

			foreach(var pair in bodyDots)
			{
				if(!seen.Contains(pair.Key) && pair.Value != null) Hide(pair.Value.gameObject);
			}

			foreach(var pair in bodyTimers)
			{
				if(!seen.Contains(pair.Key) && pair.Value != null) Hide(pair.Value.gameObject);
			}
		}

		// seconds-since-death counter under a body, frozen with the trail clock during meetings.
		// shown/hidden with the body marker itself - no separate toggle
		private static void UpdateBodyTimer(byte playerId, MapBehaviour map, SpriteRenderer template, Transform parent, SpriteRenderer marker, Color color)
		{
			TextMeshPro timer = GetOrCreateBodyTimer(playerId, map, template, parent);
			if(timer == null) return;

			// if we never saw the kill (feature toggled on mid-round), time from now
			if(!killTimes.TryGetValue(playerId, out float killed))
			{
				killed = trailClock;
				killTimes[playerId] = killed;
			}

			timer.text = $"{Mathf.Max(0f, trailClock - killed):F0}s";
			timer.color = color;

			// pin under the 'X', scaled off the marker size
			float markerSize = Mathf.Abs(marker.transform.localScale.x);
			Vector3 markerPosition = marker.transform.localPosition;

			timer.transform.localPosition = new Vector3(markerPosition.x, markerPosition.y - markerSize * TimerOffsetFactor, markerDepth);
			timer.transform.localScale = Vector3.one * markerSize * TimerScaleFactor;

			Show(timer.gameObject);
		}

		private static TextMeshPro GetOrCreateBodyTimer(byte playerId, MapBehaviour map, SpriteRenderer template, Transform parent)
		{
			if(bodyTimers.TryGetValue(playerId, out TextMeshPro existing) && existing != null)
			{
				return existing;
			}

			// clone the map's own text so we inherit a working TMP font + material
			TextMeshPro source = map.countOverlay != null ? map.countOverlay.SabotageText : null;
			if(source == null) return null;

			GameObject go = Object.Instantiate(source.gameObject, parent);
			go.name = $"HydraMinimapTimer_{playerId}";
			go.layer = template.gameObject.layer;

			TextMeshPro text = go.GetComponent<TextMeshPro>();
			if(text == null)
			{
				Object.Destroy(go);
				return null;
			}

			text.fontSize = TimerFontSize;
			text.alignment = TextAlignmentOptions.Center;
			text.richText = false;
			// white outline, so it reads over dark map art (a white player's text will be low
			// contrast over light art - swap to black here if that turns out to matter)
			text.outlineWidth = TimerOutlineWidth;
			text.outlineColor = Color.white;

			MeshRenderer renderer = go.GetComponent<MeshRenderer>();
			if(renderer != null)
			{
				renderer.sortingLayerID = sortingLayer;
				renderer.sortingOrder = sortingBase + TimerSortOrder;
				Own(renderer, TimerSortOrder);
			}

			bodyTimers[playerId] = text;
			return text;
		}

		private static void HideTimers()
		{
			foreach(TextMeshPro timer in bodyTimers.Values)
			{
				if(timer != null) Hide(timer.gameObject);
			}
		}

		// returns whether a line actually got drawn - the caller records that so the sweep at the end
		// of the frame doesn't hide it again
		private static bool UpdateTracer(byte playerId, SpriteRenderer template, Transform parent, Color color, float width)
		{
			if(!bundledPaths.TryGetValue(playerId, out Vector3[] path) || path.Length < 2) return false;

			LineRenderer line = GetOrCreateTracer(playerId, template, parent);

			// vertex upload is the expensive part. bundleVersion only moves on a rebuild, so this
			// throttles uploads to BundleInterval instead of every frame
			tracerVersion.TryGetValue(playerId, out int applied);
			if(applied != bundleVersion || line.positionCount != path.Length)
			{
				line.positionCount = path.Length;
				// one interop call for the whole path, not one per vertex
				line.SetPositions(path);

				tracerVersion[playerId] = bundleVersion;
			}

			line.widthMultiplier = width;

			// solid the whole way, like a printed transit map
			Color lineColor = new Color(color.r, color.g, color.b, TracerOpacity);
			line.startColor = lineColor;
			line.endColor = lineColor;

			Show(line.gameObject);
			return true;
		}

		// bundles overlapping tracers into parallel "subway lines" so shared corridors
		// stay readable instead of smearing into one line
		//
		// each trail vertex marks its player's bit in the cell it lands in, then reads
		// back who else shares that cell - both players read the same mask so they always
		// agree on lane count/rank. O(vertices), no neighbour search, stays cheap
		//
		// a cell with one line through it gets zero offset, so an unshared trail draws exactly
		// where walked - note a player doubling back claims a second bit and so does offset.
		// offsets are perpendicular to travel and centred on the real route
		private static void RebuildBundles(float scale, float laneSpacing)
		{
			// history only changes every SampleInterval, no need to rebundle every frame
			if(Time.time - lastBundleTime < BundleInterval && bundledPaths.Count > 0) return;
			lastBundleTime = Time.time;
			bundleVersion++;

			bundledPaths.Clear();

			// raw mode: draw sampled positions exactly as recorded
			if(!BundleTracers)
			{
				BuildRawPaths(scale);
				return;
			}

			// stable index per player so each owns one bit in the cell masks
			activeIds.Clear();
			foreach(var entry in trails)
			{
				if(entry.Value.Count < 2) continue;

				// dead players stay in - their frozen trail keeps its lane until ClearHistory
				NetworkedPlayerInfo info = GameData.Instance != null ? GameData.Instance.GetPlayerById(entry.Key) : null;
				if(info == null || info.Disconnected) continue;

				activeIds.Add(entry.Key);
			}

			if(activeIds.Count == 0) return;
			// sorting keeps lane order consistent across players
			activeIds.Sort();

			float cell = laneSpacing * GroupRadiusFactor;
			if(cell <= 0f) cell = 0.01f;

			// pass 1: snap routes onto a shared lattice, record who passes through each cell.
			// this is what makes shared corridors collapse onto the same cell sequence
			cellOccupancy.Clear();
			// two lane bits per player: first pass through a cell, and any later pass
			for(int p = 0; p < activeIds.Count && p < 16; p++)
			{
				byte id = activeIds[p];
				List<TrailSample> history = trails[id];

				if(!routeCells.TryGetValue(id, out List<long> route))
				{
					route = new List<long>();
					routeCells[id] = route;
				}
				route.Clear();

				if(!routeBits.TryGetValue(id, out List<int> bits))
				{
					bits = new List<int>();
					routeBits[id] = bits;
				}
				bits.Clear();

				if(!routeLocks.TryGetValue(id, out List<bool> locks))
				{
					locks = new List<bool>();
					routeLocks[id] = locks;
				}
				locks.Clear();

				visitCounts.Clear();

				long previous = long.MinValue;
				for(int i = 0; i < history.Count; i++)
				{
					Vector2 local = history[i].Position / scale;
					long key = CellKey(local, cell);

					// a vent hop always starts a new waypoint, even in the same cell
					bool jump = history[i].Jump && route.Count > 0;

					// collapse wandering inside one cell to a single waypoint - straightens the line
					if(key == previous && !jump) continue;
					previous = key;

					// pin both ends of the hop so corner rounding can't eat into it
					if(jump) locks[route.Count - 1] = true;

					// a revisit gets its own lane bit, so doubling back runs alongside the outbound line
					visitCounts.TryGetValue(key, out int visits);
					visitCounts[key] = visits + 1;

					int bit = 1 << (p * 2 + Mathf.Min(visits, 1));

					cellOccupancy.TryGetValue(key, out int mask);
					cellOccupancy[key] = mask | bit;

					route.Add(key);
					bits.Add(bit);
					locks.Add(jump);
				}
			}

			// pass 2: lay each route along cell centres, then slide it into its own lane
			for(int p = 0; p < activeIds.Count && p < 16; p++)
			{
				byte id = activeIds[p];
				List<long> route = routeCells[id];
				List<int> bits = routeBits[id];
				List<bool> locks = routeLocks[id];
				if(route.Count < 2) continue;

				int count = route.Count;

				Vector2[] points = new Vector2[count];
				float[] offsets = new float[count];

				for(int i = 0; i < count; i++)
				{
					Unpack(route[i], out int cx, out int cy);
					points[i] = new Vector2((cx + 0.5f) * cell, (cy + 0.5f) * cell);

					cellOccupancy.TryGetValue(route[i], out int mask);
					int bit = bits[i];

					int lanes = CountBits(mask);
					// rank among lines through this cell, counting only bits below ours
					int lane = CountBits(mask & (bit - 1));

					offsets[i] = lanes > 1 ? (lane - (lanes - 1) * 0.5f) * laneSpacing : 0f;
				}

				// round the lattice corners so it reads as flowing lines, not stairsteps
				bundledPaths[id] = RoundCorners(BuildOffsetPath(points, Smooth(offsets)), locks.ToArray(), SmoothIterations);
			}
		}

		// every recorded position, verbatim - no snapping, lanes, or smoothing
		private static void BuildRawPaths(float scale)
		{
			foreach(var entry in trails)
			{
				List<TrailSample> history = entry.Value;
				if(history.Count < 2) continue;

				NetworkedPlayerInfo info = GameData.Instance != null ? GameData.Instance.GetPlayerById(entry.Key) : null;
				if(info == null || info.Disconnected) continue;

				Vector3[] path = new Vector3[history.Count];
				for(int i = 0; i < history.Count; i++)
				{
					Vector2 point = history[i].Position / scale;
					path[i] = new Vector3(point.x, point.y, markerDepth);
				}

				bundledPaths[entry.Key] = path;
			}
		}

		private static long CellKey(Vector2 point, float cell)
		{
			int cx = Mathf.FloorToInt(point.x / cell);
			int cy = Mathf.FloorToInt(point.y / cell);

			// disjoint bit ranges, packs losslessly
			return ((long)cx << 32) ^ (uint)cy;
		}

		private static void Unpack(long key, out int cx, out int cy)
		{
			cx = (int)(key >> 32);
			cy = (int)(uint)key;
		}

		private static int CountBits(int value)
		{
			int count = 0;

			while(value != 0)
			{
				value &= value - 1;
				count++;
			}

			return count;
		}

		// eases lane offsets in/out so a line slides into a shared corridor instead of snapping
		private static float[] Smooth(float[] offsets)
		{
			float[] result = new float[offsets.Length];

			for(int i = 0; i < offsets.Length; i++)
			{
				float sum = 0f;
				int samples = 0;

				for(int j = i - SmoothWindow; j <= i + SmoothWindow; j++)
				{
					if(j < 0 || j >= offsets.Length) continue;

					sum += offsets[j];
					samples++;
				}

				result[i] = samples > 0 ? sum / samples : offsets[i];
			}

			return result;
		}

		private static Vector2[] BuildOffsetPath(Vector2[] points, float[] offsets)
		{
			Vector2[] result = new Vector2[points.Length];

			for(int i = 0; i < points.Length; i++)
			{
				// central difference for a stable heading, shift perpendicular to keep shape.
				// shared corridors get identical points here, so perpendiculars match exactly
				Vector2 delta = points[Mathf.Min(points.Length - 1, i + 1)] - points[Mathf.Max(0, i - 1)];
				Vector2 direction = delta.sqrMagnitude > 1e-8f ? delta.normalized : Vector2.right;

				result[i] = points[i] + CanonicalPerpendicular(direction) * offsets[i];
			}

			return result;
		}

		// picks a consistent side for the lane offset - a naive normal flips depending on
		// travel direction, which would stack opposing traffic on the same side. folding it
		// into a fixed half-plane keeps opposite directions on opposite sides
		private static Vector2 CanonicalPerpendicular(Vector2 direction)
		{
			Vector2 perpendicular = new Vector2(-direction.y, direction.x);

			if(perpendicular.y < -1e-6f || (Mathf.Abs(perpendicular.y) <= 1e-6f && perpendicular.x < 0f))
			{
				perpendicular = -perpendicular;
			}

			return perpendicular;
		}

		// chaikin corner cutting - straight runs stay straight, corners round off, endpoints preserved.
		// locked vertices get emitted verbatim so a vent hop still looks like an instant teleport
		private static Vector3[] RoundCorners(Vector2[] points, bool[] locked, int iterations)
		{
			Vector2[] current = points;
			bool[] currentLocked = locked;

			for(int pass = 0; pass < iterations && current.Length >= 3; pass++)
			{
				Vector2[] next = new Vector2[2 * (current.Length - 1) + 2];
				bool[] nextLocked = new bool[next.Length];
				int write = 0;

				next[write] = current[0];
				nextLocked[write++] = currentLocked[0];

				for(int i = 0; i < current.Length - 1; i++)
				{
					Vector2 a = current[i];
					Vector2 b = current[i + 1];
					bool lockedA = currentLocked[i];
					bool lockedB = currentLocked[i + 1];

					next[write] = lockedA ? a : a * 0.75f + b * 0.25f;
					nextLocked[write++] = lockedA;

					next[write] = lockedB ? b : a * 0.25f + b * 0.75f;
					nextLocked[write++] = lockedB;
				}

				next[write] = current[current.Length - 1];
				nextLocked[write] = currentLocked[current.Length - 1];

				current = next;
				currentLocked = nextLocked;
			}

			// locked vertices can repeat a point, which upsets line cap geometry
			List<Vector3> result = new List<Vector3>(current.Length);
			for(int i = 0; i < current.Length; i++)
			{
				if(i > 0 && (current[i] - current[i - 1]).sqrMagnitude < 1e-10f) continue;

				result.Add(new Vector3(current[i].x, current[i].y, markerDepth));
			}

			return result.ToArray();
		}

		private static SpriteRenderer GetOrCreatePlayerPoint(byte playerId, SpriteRenderer template, Transform parent, bool isLocal)
		{
			if(playerDots.TryGetValue(playerId, out SpriteRenderer existing) && existing != null)
			{
				return existing;
			}

			SpriteRenderer dot = CreatePoint($"HydraMinimapPoint_{playerId}", GetCircleSprite(), template, parent, PointSortOrder);

			// everyone but the local player gets a team-colored ring
			if(!isLocal)
			{
				SpriteRenderer outline = CreatePoint($"HydraMinimapOutline_{playerId}", GetCircleSprite(), template, dot.transform, OutlineSortOrder);
				outline.transform.localPosition = Vector3.zero;
				// child of the point, so this scale is relative to the already-scaled point
				outline.transform.localScale = Vector3.one * OutlineScale;
				playerOutlines[playerId] = outline;
			}

			playerDots[playerId] = dot;
			return dot;
		}

		private static SpriteRenderer GetOrCreateBodyMarker(byte playerId, SpriteRenderer template, Transform parent)
		{
			if(bodyDots.TryGetValue(playerId, out SpriteRenderer existing) && existing != null)
			{
				return existing;
			}

			SpriteRenderer marker = CreatePoint($"HydraMinimapBody_{playerId}", GetXSprite(), template, parent, BodySortOrder);
			marker.transform.localScale = template.transform.localScale * MatchedScale(template, GetXSprite()) * BodyMarkerScale;

			// thicker white 'X' behind it, same extent, so it frames all four arms evenly
			SpriteRenderer outline = CreatePoint($"HydraMinimapBodyOutline_{playerId}", GetXOutlineSprite(), template, marker.transform, BodyOutlineSortOrder);
			outline.color = Color.white;
			outline.transform.localPosition = Vector3.zero;
			outline.transform.localScale = Vector3.one;

			bodyDots[playerId] = marker;
			return marker;
		}

		private static LineRenderer GetOrCreateTracer(byte playerId, SpriteRenderer template, Transform parent)
		{
			if(tracers.TryGetValue(playerId, out LineRenderer existing) && existing != null)
			{
				return existing;
			}

			GameObject go = new GameObject($"HydraMinimapTracer_{playerId}");
			go.transform.SetParent(parent, false);
			go.layer = template.gameObject.layer;

			LineRenderer line = go.AddComponent<LineRenderer>();
			line.useWorldSpace = false;
			line.sharedMaterial = GetPointMaterial(template);
			line.sortingLayerID = sortingLayer;
			// above the whole map stack, below our own outlines and points
			line.sortingOrder = sortingBase + TracerSortOrder;
			Own(line, TracerSortOrder);
			line.numCapVertices = 2;
			line.numCornerVertices = 2;
			line.alignment = LineAlignment.View;
			line.textureMode = LineTextureMode.Stretch;

			tracers[playerId] = line;
			return line;
		}

		// copies HerePoint's layer/sorting/material (de-tinted) so it draws on top of the map,
		// but uses SpriteRenderer.color for tint instead of the local player's baked color
		private static SpriteRenderer CreatePoint(string name, Sprite sprite, SpriteRenderer template, Transform parent, int sortingBump)
		{
			GameObject go = new GameObject(name);
			go.transform.SetParent(parent, false);
			go.layer = template.gameObject.layer;

			SpriteRenderer renderer = go.AddComponent<SpriteRenderer>();
			renderer.sprite = sprite;
			renderer.sharedMaterial = GetPointMaterial(template);
			renderer.maskInteraction = template.maskInteraction;
			renderer.sortingLayerID = sortingLayer;
			renderer.sortingOrder = sortingBase + sortingBump;
			Own(renderer, sortingBump);

			return renderer;
		}

		// SetActive is an interop call, skip it if already in the right state
		private static void Show(GameObject go)
		{
			if(!go.activeSelf) go.SetActive(true);
		}

		private static void Hide(GameObject go)
		{
			if(go.activeSelf) go.SetActive(false);
		}

		// hides every marker we own that wasn't drawn this frame. keyed off what we rendered rather
		// than off the player list, so a player who died, left, or vanished from AllPlayerControls
		// can't leave their last dot and tracer sitting on the map
		private static void HideUnrendered()
		{
			foreach(var pair in playerDots)
			{
				if(!renderedIds.Contains(pair.Key) && pair.Value != null) Hide(pair.Value.gameObject);
			}

			foreach(var pair in playerOutlines)
			{
				if(!renderedIds.Contains(pair.Key) && pair.Value != null) Hide(pair.Value.gameObject);
			}

			foreach(var pair in tracers)
			{
				if(!renderedTracers.Contains(pair.Key) && pair.Value != null) Hide(pair.Value.gameObject);
			}
		}

		private static void HideAll()
		{
			HideEach(playerDots);
			HideEach(playerOutlines);
			HideEach(bodyDots);

			foreach(LineRenderer line in tracers.Values)
			{
				if(line != null) Hide(line.gameObject);
			}

			HideBurstRings();
			HideTimers();
		}

		private static void HideBurstRings()
		{
			foreach(SpriteRenderer ring in burstRings)
			{
				if(ring != null) Hide(ring.gameObject);
			}
		}

		private static void HideEach(Dictionary<byte, SpriteRenderer> dots)
		{
			foreach(SpriteRenderer dot in dots.Values)
			{
				if(dot != null) Hide(dot.gameObject);
			}
		}

		private static Color GetPlayerColor(NetworkedPlayerInfo player)
		{
			int colorId = player.DefaultOutfit.ColorId;

			if(colorId < 0 || colorId >= Palette.PlayerColors.Length)
			{
				return Color.white;
			}

			return Palette.PlayerColors[colorId];
		}

		// copies HerePoint's material (satisfies the map's stencil) but resets color to white -
		// per-object tint comes from SpriteRenderer/LineRenderer color instead
		private static Material GetPointMaterial(SpriteRenderer template)
		{
			if(pointMaterial != null) return pointMaterial;

			Material source = template.sharedMaterial != null ? template.sharedMaterial : template.material;
			pointMaterial = new Material(source)
			{
				color = Color.white
			};

			return pointMaterial;
		}

		// scales one of our sprites (1 unit across) to match HerePoint's on-screen size
		private static float MatchedScale(SpriteRenderer template, Sprite mySprite)
		{
			float herePointSize = template.sprite != null ? template.sprite.bounds.size.x : 0.2f;
			float mySize = mySprite != null ? mySprite.bounds.size.x : 1f;
			if(mySize <= 0f) mySize = 1f;

			return herePointSize / mySize;
		}

		// filled white circle, built once, tinted per-point via SpriteRenderer.color
		private static Sprite GetCircleSprite()
		{
			if(circleSprite != null) return circleSprite;

			const int size = 64;
			Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
			{
				filterMode = FilterMode.Bilinear
			};

			float center = (size - 1) / 2f;
			float radius = size / 2f - 1f;

			// filled into a managed buffer and uploaded in one call - per-pixel SetPixel
			// crosses IL2CPP every time and stalls the frame the map first opens on
			Color32[] pixels = new Color32[size * size];

			for(int x = 0; x < size; x++)
			{
				for(int y = 0; y < size; y++)
				{
					float dx = x - center;
					float dy = y - center;
					float distance = Mathf.Sqrt(dx * dx + dy * dy);

					// one-pixel soft edge so it isn't harshly aliased
					float alpha = Mathf.Clamp01(radius - distance + 0.5f);
					pixels[y * size + x] = ToPixel(alpha);
				}
			}

			texture.SetPixels32(pixels);
			texture.Apply();

			// pixelsPerUnit == size makes it exactly one unit across, MatchedScale adjusts from there
			circleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
			return circleSprite;
		}

		// white 'X' for body markers, built once, tinted per-body via SpriteRenderer.color
		private static Sprite GetXSprite()
		{
			// inset from corners so the outline behind still peeks past the tips
			if(xSprite == null) xSprite = BuildXSprite(4, 4);

			return xSprite;
		}

		// thicker, longer 'X' as the white outline behind a body marker - full length while
		// the colored X is inset, so white shows on the arm tips too
		private static Sprite GetXOutlineSprite()
		{
			if(xOutlineSprite == null) xOutlineSprite = BuildXSprite(10, 0);

			return xOutlineSprite;
		}

		private static Sprite BuildXSprite(int thickness, int inset)
		{
			const int size = 48;

			Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
			{
				filterMode = FilterMode.Bilinear
			};

			// default Color32 is already fully transparent, so no clearing pass is needed
			Color32[] pixels = new Color32[size * size];

			// draw both diagonals for the 'X' - inset shortens arms symmetrically so the
			// outline variant (thicker, full length) frames the colored one on every side
			int half = thickness / 2;
			for(int i = inset; i < size - inset; i++)
			{
				for(int t = -half; t <= half; t++)
				{
					PaintPixel(pixels, i + t, i, size);
					PaintPixel(pixels, i + t, size - 1 - i, size);
				}
			}

			texture.SetPixels32(pixels);
			texture.Apply();

			return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
		}

		// hollow ring, built once, used for the kill burst animation
		private static Sprite GetRingSprite()
		{
			if(ringSprite != null) return ringSprite;

			const int size = 128;

			Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false)
			{
				filterMode = FilterMode.Bilinear
			};

			float center = (size - 1) / 2f;
			float outer = size / 2f - 1f;
			// thin band - the ring scales up to map size, so a thick one would blob out
			float inner = outer * 0.93f;

			Color32[] pixels = new Color32[size * size];

			for(int x = 0; x < size; x++)
			{
				for(int y = 0; y < size; y++)
				{
					float dx = x - center;
					float dy = y - center;
					float distance = Mathf.Sqrt(dx * dx + dy * dy);

					// soft on both edges so it stays smooth as it scales up
					float alpha = Mathf.Clamp01(outer - distance + 0.5f) * Mathf.Clamp01(distance - inner + 0.5f);
					pixels[y * size + x] = ToPixel(alpha);
				}
			}

			texture.SetPixels32(pixels);
			texture.Apply();

			ringSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
			return ringSprite;
		}

		private static void PaintPixel(Color32[] pixels, int x, int y, int size)
		{
			if(x >= 0 && x < size && y >= 0 && y < size)
			{
				pixels[y * size + x] = new Color32(255, 255, 255, 255);
			}
		}

		// white texel at the given coverage, fully transparent black below zero so the
		// generated sprites keep the exact edge they had with per-pixel Color writes
		private static Color32 ToPixel(float alpha)
		{
			if(alpha <= 0f) return new Color32(0, 0, 0, 0);

			return new Color32(255, 255, 255, (byte)Mathf.RoundToInt(Mathf.Clamp01(alpha) * 255f));
		}
	}
}
