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

    public static class StructUtils
    {
        public static IOAttribute TrySetDefault(IOAttribute attr, Type type, string defaultString)
        {
            if (!string.IsNullOrEmpty(defaultString))
            {
                double doubleDefault = 0;
                switch (type.ToString())
                {
                    case "System.Boolean":
                        bool boolDefault = false;
                        if (bool.TryParse(defaultString, out boolDefault))
                            attr.DefaultBoolean = boolDefault;
                        break;
                    case "System.Double":
                    case "System.Single":
                    case "System.Int32":
                        if (double.TryParse(defaultString, out doubleDefault))
                            attr.DefaultValue = doubleDefault;
                        break;
                    case "System.String":
                        attr.DefaultString = defaultString;
                        break;
                    case "VVVV.Utils.VMath.Vector2D":
                    case "VVVV.Utils.VMath.Vector3D":
                    case "VVVV.Utils.VMath.Vector4D":
                        var vectorString = defaultString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        attr.DefaultValues = new double[vectorString.Length];
                        for (int i = 0; i < vectorString.Length; i++)
                        {
                            if (double.TryParse(vectorString[i].Trim(), out doubleDefault))
                                attr.DefaultValues[i] = doubleDefault;

                        }
                        break;
                    case "VVVV.Utils.VColor.RGBAColor":
                        var colorString = defaultString.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        attr.DefaultColor = new double[colorString.Length];
                        for (int i = 0; i < colorString.Length; i++)
                        {
                            if (double.TryParse(colorString[i].Trim(), out doubleDefault))
                                attr.DefaultColor[i] = doubleDefault;

                        }
                        break;

                }
            }
            return attr;
        }
    }
}
