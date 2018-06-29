using System.IO;
using VVVV.Utils.VColor;
using VVVV.Utils.VMath;
using VVVV.Utils.IO;

using System.Xml.Linq;

namespace VVVV.Struct.Factories
{
    public class PrimitivesFieldTypeRegistry : BaseFieldTypeRegistry
    {
        public override string ContainerType => "Stream";

        public PrimitivesFieldTypeRegistry() : base()
        {
            FMappings.Add("bool", typeof(bool));

            FMappings.Add("int", typeof(int));
            FMappings.Add("double", typeof(double));
            FMappings.Add("value", typeof(double));
            FMappings.Add("float", typeof(float));

            FMappings.Add("string", typeof(string));
            FMappings.Add("text", typeof(string));

            FMappings.Add("color", typeof(RGBAColor));
            FMappings.Add("rgbacolor", typeof(RGBAColor));

            FMappings.Add("vector2d", typeof(Vector2D));
            FMappings.Add("vector3d", typeof(Vector3D));
            FMappings.Add("vector4d", typeof(Vector4D));
            FMappings.Add("vector2", typeof(Vector2D));
            FMappings.Add("vector3", typeof(Vector3D));
            FMappings.Add("vector4", typeof(Vector4D));

            FMappings.Add("matrix", typeof(Matrix4x4));
            FMappings.Add("matrix4", typeof(Matrix4x4));
            FMappings.Add("matrix4x4", typeof(Matrix4x4));
            FMappings.Add("transform", typeof(Matrix4x4));

            FMappings.Add("stream", typeof(Stream));
            FMappings.Add("raw", typeof(Stream));
            FMappings.Add("struct", typeof(Core.Struct));

            FMappings.Add("xnode", typeof(XNode));
            FMappings.Add("xelement", typeof(XElement));
            FMappings.Add("xattribute", typeof(XAttribute));

            FMappings.Add("mouse", typeof(Mouse));
            FMappings.Add("keyboard", typeof(Keyboard));
            FMappings.Add("touch", typeof(TouchDevice));
            FMappings.Add("gesture", typeof(GestureDevice));

            //FMappings.Add("enum", typeof(EnumEntry));
        }
    }
}
