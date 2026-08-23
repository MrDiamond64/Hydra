using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal abstract class Section
	{
		public readonly string name;
		public Vector2 scrollVector;

		public Section(string name)
		{
			this.name = name;
		}

		public virtual void HandleSubsectionMove(int offset) { }

		public abstract void Render();
	}
}