using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;

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

		private Dictionary<string,IIOContainer> data;
		public Dictionary<string,IIOContainer> Data { get { return data; } }
		
		public Struct(string name)
		{
			key = name;
			data = new Dictionary<string,IIOContainer>();
		}
	}
}
