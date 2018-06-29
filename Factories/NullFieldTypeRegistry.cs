using System;
using VVVV.Struct.Core;

namespace VVVV.Struct.Factories
{
    public class NullFieldTypeRegistry : IFieldTypeRegistry
    {
        public virtual string ContainerType => "Null";
        public IContainerRegistry ContainerRegistry { get; set; }
        public event EventHandler<string> TypeUpdated;

        public NullFieldTypeRegistry() {}

        public bool AddAssembly(System.Reflection.Assembly a) => true;

        public virtual bool StringToType(string typestring, out Type type)
        {
            type = null;
            return true;
        }

        public virtual bool TypeToString(Type type, out string typestring)
        {
            typestring = type.Name.ToLower();
            return true;
        }
    }
}