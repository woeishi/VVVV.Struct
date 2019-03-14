using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Struct.Core
{
    public interface IContainerRegistry
    {
        string ContainerType { get; }
        object CloneData(object other);
        bool CreateIO(IIOFactory ioFactory, Dictionary<Field, IOContainer> ioContainers, Field field, bool isInput = true, bool isBinSized = false, int order = 0);
        void OnSync(IIOContainer ioContainer);
        int GetLength(IIOContainer ioContainer);
        void SetLength(Field field, IIOContainer ioContainer, int length);
        void ReadFromIOBin(Field field, IIOContainer ioContainer, ISpread<Struct> structs);
        void WriteToIO(Field field, IIOContainer ioContainer, ref Struct str);
        void WriteToIOBin(Field field, IIOContainer ioContainer, ref Struct str, int index = 0);
        void NilToIOBin(Field field, IIOContainer ioContainer, int index = 0);
        string ToString(object rawIOObject);
    }
}