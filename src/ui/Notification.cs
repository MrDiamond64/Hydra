namespace HydraMenu.ui
{
	public class Notification
	{
		public readonly string title;
		public readonly string message;
		public readonly float ttl;
		public float lifetime;

		public Notification(string title, string message, float ttl)
		{
			this.title = title;
			this.message = message;
			this.ttl = ttl;
			this.lifetime = 0;
		}

		public bool HasExpired
		{
			get { return this.lifetime > ttl; }
		}
	}
}