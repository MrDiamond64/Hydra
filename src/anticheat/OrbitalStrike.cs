using System;
using System.Collections;
using UnityEngine;

namespace HydraMenu.anticheat
{
	/// <summary>
	/// A purely visual "orbital strike" effect: a beam drops from the sky onto a player before they are
	/// removed from the lobby. It is a flourish layered on top of the anticheat's existing ban, and never
	/// changes the outcome — <paramref name="onComplete"/> (the actual ban) always runs, even if the visual
	/// fails to initialise.
	/// </summary>
	internal static class OrbitalStrike
	{
		// How long the beam is visible before the ban fires, and how high above the player it starts.
		private const float Duration = 0.6f;
		private const float BeamHeight = 30.0f;

		public static IEnumerator Strike(PlayerControl target, Action onComplete)
		{
			GameObject beamObject = null;
			LineRenderer beam = null;

			// Sample the position once so the beam stays put even if the target keeps moving.
			Vector3 groundPosition = target.transform.position;
			Vector3 skyPosition = groundPosition + new Vector3(0.0f, BeamHeight, 0.0f);

			try
			{
				beamObject = new GameObject("HydraOrbitalStrike");

				beam = beamObject.AddComponent<LineRenderer>();
				beam.material = new Material(Shader.Find("Sprites/Default"));
				beam.positionCount = 2;
				beam.SetPosition(0, skyPosition);
				beam.SetPosition(1, groundPosition);
				// A high sorting order so the beam draws on top of the map. This may need tuning in-game.
				beam.sortingOrder = 100;
			}
			catch(Exception e)
			{
				// If the visual cannot be created for any reason, log it and fall through to the ban.
				Hydra.Log.LogError($"Orbital strike visual failed to initialise: {e}");
			}

			float elapsed = 0.0f;
			while(beam != null && elapsed < Duration)
			{
				elapsed += Time.deltaTime;
				float progress = elapsed / Duration;

				// Flare bright and thick, then thin out and fade to transparent.
				float width = Mathf.Lerp(1.4f, 0.1f, progress);
				beam.startWidth = width;
				beam.endWidth = width;

				Color color = new Color(1.0f, Mathf.Lerp(0.2f, 0.8f, progress), 0.1f, 1.0f - progress);
				beam.startColor = color;
				beam.endColor = color;

				yield return null;
			}

			if(beamObject != null)
			{
				UnityEngine.Object.Destroy(beamObject);
			}

			onComplete?.Invoke();
		}
	}
}
