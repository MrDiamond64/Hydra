using HydraMenu.modules;
using HydraMenu.network;
using UnityEngine;

namespace HydraMenu.routines
{
	internal class SpamInfluencerCues : Routine
	{
		// This is only useful on host-authoritative lobbies
		// The vanilla anticheat has strict anticheat checks such as cooldown and role checks, which we can get around,
		// but they also check if the RPC is being sent globally, which means we would have to send a SpiritGuideMessage for each individual player
		// The SpiritGuideMessage cooldown has a lower bound of three seconds on the server, so with 14 players, each player would only see an influencer cue every 42 seconds
		public SpamInfluencerCues() : base("SpamInfluencerCues") { }

		// The client will throw an error if the SpiritGuideMessage RPC has more than three sprites
		private readonly int MAX_SPRITE_COUNT = 3;
		private readonly float SEND_DELAY = 4.0f;
		private float timeElapsed = 0.0f;

		private System.Random rnd = new System.Random();

		public override void Run()
		{
			timeElapsed += Time.deltaTime;
			if(timeElapsed < SEND_DELAY) return;
			timeElapsed = 0.0f;

			byte[] sprites = new byte[MAX_SPRITE_COUNT];
			int possibleSpriteCount = ShipStatus.Instance.socialMediumFeedSystem.PossibleSpriteList.Count;

			for(int i = 0; i < sprites.Length; i++)
			{
				sprites[i] = (byte)rnd.Next(0, possibleSpriteCount);
			}

			BatchedMessage batch = new BatchedMessage();
			batch.QueueSpiritGuideMessage(sprites);
			batch.FinishBatch();
		}

		private void OnDisconnect()
		{
			Hydra.notifications.Send("Spam Influencer Cues", "Spam Influencer Cues was disabled as you left the game.", 10);
			Enabled = false;
		}

		protected override void OnEnable()
		{
			if(PlayerControl.LocalPlayer == null || ShipStatus.Instance == null)
			{
				Hydra.notifications.Send("Spam Influencer Cues", "Spam Influencer Cues can only be used once the game has started.", 10);
				Enabled = false;
				return;
			}

			EventCoordinator.OnDisconnect += OnDisconnect;
		}

		protected override void OnDisable()
		{
			EventCoordinator.OnDisconnect -= OnDisconnect;
		}
	}
}