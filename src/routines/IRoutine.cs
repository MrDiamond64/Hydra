namespace HydraMenu.routines
{
	public abstract class IRoutine
	{
		public string name = "";

		public bool _enabled = false;
		public virtual bool Enabled {
			get { return _enabled; }
			set {
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

		public abstract void Run();

		public virtual void OnEnable() { }
		public virtual void OnDisable() { }
		public virtual void OnDisconnect() { }
	}
}