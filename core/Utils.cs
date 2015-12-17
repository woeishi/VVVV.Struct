using System;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

namespace VVVV.Struct
{
    public static class StructUtils
    {
        public static Struct DeepCopy(this Struct str)
        {
            if (str == null)
                return null;
            else
            {
                var s = new Struct(str.Key);
                foreach (var entry in str.Data)
                    s.Data[entry.Key] = (entry.Value as ISpread).Clone();
                return s;
            }
        }

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
