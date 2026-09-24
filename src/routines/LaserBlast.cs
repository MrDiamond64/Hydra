using BepInEx.Unity.IL2CPP.Utils.Collections;
using HydraMenu.network;
using System;
using System.Collections;
using UnityEngine;

namespace HydraMenu.routines
{
	/// <summary>
	/// A custom, keybind-driven ability: press the ability key in-game to fire a laser beam from your player
	/// toward the nearest player (or straight ahead if the lobby is empty around you). The beam is purely
	/// cosmetic — it is spectacle for a private lobby and does not damage, kill, or otherwise affect anyone.
	///
	/// This routine also serves as a template for other custom abilities: a routine gets a per-frame Run() tick
	/// plus config persistence for free, so binding a key to an effect is just the pattern below.
	/// </summary>
	public class LaserBlastRoutine : Routine
	{
		public LaserBlastRoutine() : base("LaserBlast") { }

		// Persisted automatically by the routine config system.
		public KeyCode AbilityKey { get; set; } = KeyCode.L;
		public float Cooldown { get; set; } = 3.0f;

		// When true AND we are the host, the laser kills the player it hits. This reuses the same host murder
		// authority as the existing "Kill Everyone" host tool, gated to the host of the lobby only. It is off by
		// default so the ability stays a harmless visual unless the host of a private lobby opts in.
		public bool LethalWhenHosting { get; set; } = false;

		private float cooldownRemaining = 0.0f;
		private bool isFiring = false;

		// How long the beam stays on screen, and how far it reaches when there is nobody to aim at.
		private const float Duration = 0.35f;
		private const float MaxRange = 40.0f;

		public override void Run()
		{
			if(cooldownRemaining > 0.0f)
			{
				cooldownRemaining -= Time.deltaTime;
			}

			if(PlayerControl.LocalPlayer == null) return;

			// Never fire while the player is interacting with the menu or chat, so the key press is not "stolen".
			if(Hydra.mainUI.visible || Hydra.mainUI.pendingKeybind != null) return;
			if(HudManager.Instance != null && HudManager.Instance.Chat != null && HudManager.Instance.Chat.IsOpenOrOpening) return;

			if(!Input.GetKeyDown(AbilityKey) || cooldownRemaining > 0.0f || isFiring) return;

			cooldownRemaining = Cooldown;
			AmongUsClient.Instance.StartCoroutine(FireLaser().WrapToIl2Cpp());
		}

		private IEnumerator FireLaser()
		{
			isFiring = true;

			PlayerControl shooter = PlayerControl.LocalPlayer;
			Vector3 origin = shooter.transform.position;

			// Aim at the nearest living player; if there is nobody nearby, just fire off to the side.
			PlayerControl target = GetNearestPlayer(shooter);
			Vector3 endPoint = target != null
				? target.transform.position
				: origin + new Vector3(MaxRange, 0.0f, 0.0f);

			// Optionally make the hit lethal, but only for the host of the lobby (see TryKill).
			if(LethalWhenHosting && target != null)
			{
				TryKill(target);
			}

			GameObject beamObject = null;
			LineRenderer beam = null;

			try
			{
				beamObject = new GameObject("HydraLaserBlast");

				beam = beamObject.AddComponent<LineRenderer>();
				beam.material = new Material(Shader.Find("Sprites/Default"));
				beam.positionCount = 2;
				beam.SetPosition(0, origin);
				beam.SetPosition(1, endPoint);
				// A high sorting order so the beam draws on top of the map. May need tuning in-game.
				beam.sortingOrder = 100;
			}
			catch(Exception e)
			{
				Hydra.Log.LogError($"Laser blast visual failed to initialise: {e}");
			}

			float elapsed = 0.0f;
			while(beam != null && elapsed < Duration)
			{
				elapsed += Time.deltaTime;
				float progress = elapsed / Duration;

				// A bright cyan bolt that thins and fades out.
				float width = Mathf.Lerp(0.9f, 0.1f, progress);
				beam.startWidth = width;
				beam.endWidth = width;

				Color color = new Color(0.2f, Mathf.Lerp(0.8f, 1.0f, progress), 1.0f, 1.0f - progress);
				beam.startColor = color;
				beam.endColor = color;

				// Keep the beam anchored to the shooter (and target) so it tracks movement while firing.
				if(shooter != null) beam.SetPosition(0, shooter.transform.position);
				if(target != null) beam.SetPosition(1, target.transform.position);

				yield return null;
			}

			if(beamObject != null)
			{
				UnityEngine.Object.Destroy(beamObject);
			}

			isFiring = false;
		}

		// Kills the target, but only when we are the host, using the same authority and checks as the other host
		// kill tools. Outside of a lobby we host, this deliberately does nothing but notify.
		private static void TryKill(PlayerControl target)
		{
			bool hasAnticheat = Utilities.IsAnticheatPresent();

			if(hasAnticheat && !AmongUsClient.Instance.AmHost)
			{
				Hydra.notifications.Send("Laser Blast", "The lethal laser only works when you are the host of the lobby.");
				return;
			}

			if(hasAnticheat && AmongUsClient.Instance.GameState != InnerNet.InnerNetClient.GameStates.Started)
			{
				Hydra.notifications.Send("Laser Blast", "The lethal laser can only be used once the game has started.");
				return;
			}

			BatchedMessage batch = new BatchedMessage();
			batch.QueueMurderPlayer(PlayerControl.LocalPlayer, target, MurderResultFlags.Succeeded);
			batch.FinishBatch();
		}

		private static PlayerControl GetNearestPlayer(PlayerControl self)
		{
			PlayerControl nearest = null;
			float nearestDistance = float.MaxValue;

			foreach(PlayerControl player in PlayerControl.AllPlayerControls)
			{
				if(player == self || player.Data == null || player.Data.IsDead) continue;

				float distance = Vector2.Distance(self.transform.position, player.transform.position);
				if(distance < nearestDistance)
				{
					nearestDistance = distance;
					nearest = player;
				}
			}

			return nearest;
		}

		protected override void OnEnable()
		{
			// Only greet the player when actually in a game; at startup (config load) there is no local player.
			if(PlayerControl.LocalPlayer != null)
			{
				Hydra.notifications.Send("Laser Blast", $"Laser Blast ready. Press {AbilityKey} to fire.", 5);
			}
		}
	}
}
