using System;
using System.Collections.Generic;
using UnityEngine;

namespace HydraMenu.ui.sections
{
	internal abstract class ISection
	{
		public readonly string name = "";
		public Vector2 scrollVector;
		public List<Feature> Features = new List<Feature>();

		public ISection(string name)
		{
			this.name = name;
		}

		public void AddFeature(string featureName, Action renderAction)
		{
			Features.Add(new Feature { Name = featureName, RenderAction = renderAction });
		}

		public virtual void HandleSubsectionMove(int offset) { }

		public abstract void Render();
	}

	public class Feature
	{
		public string Name { get; set; }
		public Action RenderAction { get; set; }
	}
}