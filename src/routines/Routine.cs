namespace HydraMenu.routines
{
	public abstract class Routine
	{
		public readonly string name;

		public bool _enabled = false;
		public virtual bool Enabled
		{
			get { return _enabled; }
			set
			{
				if(value == _enabled) return;
				_enabled = value;

				if(value)
				{
					OnEnable();
				}
				else
				{
					OnDisable();
				}
			}
		}

		public Routine(string name)
		{
			this.name = name;
		}

		public abstract void Run();

		protected virtual void OnEnable() { }
		protected virtual void OnDisable() { }
		public virtual void OnDisconnect() { }
	}
}