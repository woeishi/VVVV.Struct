using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.Struct
{
	public class Definition : EventArgs
	{
		private string key;
		public string Key { get { return key; } }
		
		private Dictionary<string,Type> typeDict;
		public Dictionary<string,Type> Types { get { return typeDict; } }
		private Dictionary<string,string> defaultDict;
		public Dictionary<string,string> Defaults { get { return defaultDict; } }
		
		internal Definition(string key)
		{
			this.key = key;
			typeDict = new Dictionary<string, Type>();
			defaultDict = new Dictionary<string, string>();
		}
		
		internal void Define(string name, Type Type, string defaultValue = "")
		{
			typeDict[name] = Type;
			defaultDict[name] = defaultValue;
		}
	}
	
	/// <summary>
	/// class to hold the struct data
	/// </summary>
	public class Struct
	{
		private string key;
		public string Key { get { return key; } }

		private Dictionary<string,object> data;
		public Dictionary<string,object> Data { get { return data; } }
		
		public Struct(string name)
		{
			key = name;
			data = new Dictionary<string,object>();
		}
		
		public Struct DeepCopy()
		{
			if (this == null)
				return null;
			else
			{
				var s = new Struct(this.Key);
				foreach (var entry in this.Data)
					s.Data[entry.Key] = (entry.Value as ISpread).Clone();
				return s;
			}
		}
	}
}
