using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json;

namespace HydraMenu.modules
{
	internal abstract class Module
	{
		public readonly string name;
		public readonly Type[] incompatibilities;

		protected bool _enabled = false;
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

		protected Module(string name, Type[] incompatibilities = null)
		{
			this.name = name;
			this.incompatibilities = incompatibilities ?? Array.Empty<Type>();
		}

		protected virtual void OnEnable()
		{
			// Handle module incompatibilities
			if(Hydra.modules != null)
			{
				foreach(Module module in Hydra.modules.moduleList)
				{
					if(!incompatibilities.Contains(module.GetType()) || !module.Enabled) continue;

					Hydra.notifications.Send(name, $"The module {module.name} is incompatible with this module, so it has been disabled.", 5);
					module.Enabled = false;
				}
			}
		}

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
					Hydra.Log.LogWarning($"Config has setting {propertyName} for module {name} when this module has no such setting");
					continue;
				}

				property.SetValue(this, propertyValue.Deserialize(property.PropertyType));
			}
		}
	}
}