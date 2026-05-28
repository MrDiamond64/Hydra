namespace HydraMenu.routines
{
	public abstract class IRoutine
	{
		public string name = "";
		public bool _enabled = false;
		public virtual bool Enabled {
			get { return _enabled; }
			set { _enabled = value; }
		}

		public abstract void Run();
	}
}