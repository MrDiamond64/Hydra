namespace HydraMenu.modules
{
	public abstract class Module
	{
		public readonly string name;

		private bool _enabled = false;
		public virtual bool Enabled
		{
			get { return _enabled; }
			set
			{
				if(_enabled == value) return;
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

		protected Module(string name)
		{
			this.name = name;
		}

		protected virtual void OnEnable() { }
		protected virtual void OnDisable() { }
	}
}