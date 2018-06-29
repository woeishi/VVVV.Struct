using System;
using System.Globalization;
using VVVV.PluginInterfaces.V2;

using VVVV.Struct.Core;

namespace VVVV.Struct.Factories
{
    public static class ContainerRegistyUtils
    {
        public static void SetDefaultAttribute(this InputAttribute attr, Field field)
        {
            if (!string.IsNullOrEmpty(field.Default))
            {
                double doubleDefault = 0;
                switch (field.FieldType.ToString())
                {
                    case "System.Boolean":
                        bool boolDefault = false;
                        if (bool.TryParse(field.Default, out boolDefault))
                            attr.DefaultBoolean = boolDefault;
                        break;
                    case "System.Double":
                    case "System.Single":
                    case "System.Int16":
                    case "System.Int32":
                    case "System.Int64":
                    case "System.UInt16":
                    case "System.UInt32":
                    case "System.UInt64":
                        if (double.TryParse(field.Default, NumberStyles.Any, CultureInfo.InvariantCulture, out doubleDefault))
                            attr.DefaultValue = doubleDefault;
                        break;
                    case "System.String":
                        attr.DefaultString = field.Default;
                        break;
                    case "VVVV.Utils.VMath.Vector2D":
                    case "VVVV.Utils.VMath.Vector3D":
                    case "VVVV.Utils.VMath.Vector4D":
                        var vectorString = field.Default.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        attr.DefaultValues = new double[vectorString.Length];
                        for (int i = 0; i < vectorString.Length; i++)
                        {
                            if (double.TryParse(vectorString[i].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out doubleDefault))
                                attr.DefaultValues[i] = doubleDefault;

                        }
                        break;
                    case "VVVV.Utils.VColor.RGBAColor":
                        var colorString = field.Default.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                        attr.DefaultColor = new double[colorString.Length];
                        for (int i = 0; i < colorString.Length; i++)
                        {
                            if (double.TryParse(colorString[i].Trim(), NumberStyles.Any, CultureInfo.InvariantCulture, out doubleDefault))
                                attr.DefaultColor[i] = doubleDefault;

                        }
                        break;
                }
            }
        }
    }
}
