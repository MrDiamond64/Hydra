using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text.Json;

namespace HydraMenu.routines
{
	public abstract class Routine
	{
		public readonly string name;

		protected bool _enabled = false;
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

		public Dictionary<string, JsonElement> GetConfigData()
		{
			Dictionary<string, JsonElement> configData = new Dictionary<string, JsonElement>();

			Type type = GetType();
			PropertyInfo[] properties = type.GetProperties();

			foreach(PropertyInfo property in properties)
			{
				configData.Add(property.Name, JsonSerializer.SerializeToElement(property.GetValue(this, null)));
			}

			return configData;
		}

		public void LoadConfigData(Dictionary<string, JsonElement> configData)
		{
			Type type = GetType();

			foreach((string propertyName, JsonElement propertyValue) in configData)
			{
				PropertyInfo property = type.GetProperty(propertyName);
				if(property == null)
				{
					Hydra.Log.LogWarning($"Config has setting {propertyName} for routine {name} when this routine has no such setting");
					continue;
				}

				property.SetValue(this, propertyValue.Deserialize(property.PropertyType));
			}
		}
	}
}