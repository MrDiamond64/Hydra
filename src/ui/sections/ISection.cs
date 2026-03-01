using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal abstract class ISection
	{
		public string name = "";
		public Vector2 scrollVector;

		public virtual void Render() { }
	}
}