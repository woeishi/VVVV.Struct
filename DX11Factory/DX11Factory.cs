using System;
using System.Collections.Generic;

using VVVV.Struct.Core;
using VVVV.DX11;

namespace VVVV.Struct.Factories
{
    public class DX11GraphPartFactory : IStructFactory
    {
        readonly DX11GraphPartFieldtypeRegistry fieldtypeReg;
        readonly IReadOnlyCollection<IFieldTypeRegistry> ftRegistries;
        readonly DX11ResourceContainerRegistry containerReg;
        readonly IReadOnlyCollection<IContainerRegistry> cRegistries;

        public DX11GraphPartFactory()
        {
            fieldtypeReg = new DX11GraphPartFieldtypeRegistry();
            ftRegistries = new List<IFieldTypeRegistry>() { fieldtypeReg };
            containerReg = new DX11ResourceContainerRegistry();
            cRegistries = new List<IContainerRegistry>() { containerReg };
        }

        public IReadOnlyCollection<IFieldTypeRegistry> FieldTypeRegistries => ftRegistries;
        public IReadOnlyCollection<IContainerRegistry> ContainerRegistries => cRegistries;

        public IEnumerable<Type> Interfaces(Type type)
        {
            if (type.GetInterface(typeof(IStructFieldGetter).FullName) != null)
                yield return typeof(IDX11RenderGraphPart);
            if (type.GetInterface(typeof(IStructFieldSetter).FullName) != null)
                yield return typeof(IDX11ResourceDataRetriever);
        }
    }

    public class DX11Factory : IStructFactory
    {
        readonly DX11FieldtypeRegistry fieldtypeReg;
        readonly IReadOnlyCollection<IFieldTypeRegistry> ftRegistries;
        readonly IReadOnlyCollection<IContainerRegistry> cRegistries;

        public DX11Factory()
        {
            fieldtypeReg = new DX11FieldtypeRegistry();
            ftRegistries = new List<IFieldTypeRegistry>() { fieldtypeReg };
            cRegistries = new List<IContainerRegistry>();
        }

        public IReadOnlyCollection<IFieldTypeRegistry> FieldTypeRegistries => ftRegistries;
        public IReadOnlyCollection<IContainerRegistry> ContainerRegistries => cRegistries;

        public IEnumerable<Type> Interfaces(Type type)
        {
            yield break;
        }
    }
}
