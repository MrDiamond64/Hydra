using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

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

		public Dictionary<string, object> GetConfigData()
		{
			Dictionary<string, object> configData = new Dictionary<string, object>();

			Type type = GetType();
			IEnumerable<PropertyInfo> properties = type.GetProperties();

			foreach(PropertyInfo property in properties)
			{
				configData.Add(property.Name, property.GetValue(this, null));
			}

			return configData;
		}
	}
}