using System;

using VVVV.Struct.Core;

namespace VVVV.Struct.Factories
{
    public interface IFieldTypeRegistry
    {
        string ContainerType { get; }
        IContainerRegistry ContainerRegistry { get; set; }
        bool StringToType(string typestring, out Type type);
        bool TypeToString(Type type, out string typestring);
        event EventHandler<string> TypeUpdated;
        bool AddAssembly(System.Reflection.Assembly a);
    }
}
