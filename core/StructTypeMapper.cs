using System;
using System.Collections.Generic;

using VVVV.Utils.VMath;
using VVVV.Utils.VColor;
using SlimDX;
using System.IO;

namespace VVVV.Struct
{
	public static class StructTypeMapper
	{
		public static Type Map(string typeName)
		{
			var key = typeName.ToLower();
			if(Mappings.ContainsKey(key))
				return Mappings[typeName.ToLower()];
			else
			{
				try
				{
					return System.Type.GetType(typeName, true, true);
				}
				catch
				{
					return typeof(double);
				}
			}
		}
		
		public static Dictionary<string, Type> Mappings = new Dictionary<string, Type>();
		
		static StructTypeMapper()
		{
			Mappings.Add("bool", typeof(bool));
			
			Mappings.Add("int", typeof(int));
			Mappings.Add("double", typeof(double));
			Mappings.Add("value", typeof(double));
			Mappings.Add("float", typeof(float));

			Mappings.Add("string", typeof(string));
			Mappings.Add("text", typeof(string));
			
			Mappings.Add("color", typeof(RGBAColor));
			Mappings.Add("rgbacolor", typeof(RGBAColor));
			
			Mappings.Add("vector2", typeof(Vector2D));
			Mappings.Add("vector3", typeof(Vector3D));
			Mappings.Add("vector4", typeof(Vector4D));
			Mappings.Add("vector2d", typeof(Vector2D));
			Mappings.Add("vector3d", typeof(Vector3D));
			Mappings.Add("vector4d", typeof(Vector4D));
			
			Mappings.Add("matrix", typeof(Matrix));
			Mappings.Add("matrix4", typeof(Matrix));
			Mappings.Add("matrix4x4", typeof(Matrix));
			Mappings.Add("transform", typeof(Matrix));
			
			Mappings.Add("raw", typeof(Stream));
			Mappings.Add("stream", typeof(Stream));
			Mappings.Add("struct", typeof(Struct));
		}
	}
}
