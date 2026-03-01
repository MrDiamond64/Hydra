namespace HydraMenu.routines
{
	public abstract class IRoutine
	{
		public virtual string routineName { get; set; } = "";
		public virtual bool Enabled { get; set; } = false;

		public virtual void Run() { }
	}
}