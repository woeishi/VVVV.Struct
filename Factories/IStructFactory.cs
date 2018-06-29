using System;
using System.Collections.Generic;

using VVVV.Struct.Core;

namespace VVVV.Struct.Factories
{
    public interface IStructFactory
    {
        IEnumerable<Type> Interfaces(Type type);
        IReadOnlyCollection<IContainerRegistry> ContainerRegistries { get; }
        IReadOnlyCollection<IFieldTypeRegistry> FieldTypeRegistries { get; }
    }
}
