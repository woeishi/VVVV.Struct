using System.Collections.Generic;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Struct.Core;

namespace VVVV.Struct.Factories
{
    public class NullContainerRegistry : IContainerRegistry
    {
        public virtual string ContainerType => "Null";

        public bool CreateIO(IIOFactory ioFactory, Dictionary<Field, IOContainer> ioContainers, Field field, bool isInput = true, bool isBinSized = false, int order = 0)
        {
            if (ioContainers.ContainsKey(field))
            {
                ioContainers[field].SetOrder(order);
                return false;
            }
            else
            {
                if (isInput)
                {
                    var inAttr = new InputAttribute(field.Name) { Order = order, AutoValidate = false, Visibility = PinVisibility.Hidden };
                    var ioCont = ioFactory.CreateIOContainer(typeof(INodeIn), inAttr);
                    ioContainers.Add(field,new IOContainer(ioCont, isInput, false, order));
                }
                else
                {
                    var outAttr = new OutputAttribute(field.Name) { Order = order, Visibility = PinVisibility.Hidden }; //, AutoFlush = false
                    var ioCont = ioFactory.CreateIOContainer(typeof(INodeOut), outAttr);
                    ioContainers.Add(field, new IOContainer(ioCont, isInput, false, order));
                    //(ioContainers[field].RawIOObject as ISpread).SliceCount = 0;
                }
            }
            return true;
        }

        public virtual object CloneData(object data) => null;

        public virtual void OnSync(IIOContainer ioContainer) { }

        public int GetLength(IIOContainer ioContainer) => 1;

        public void ReadFromIOBin(Field field, IIOContainer ioContainer, ISpread<Core.Struct> structs) { }

        public virtual void SetLength(Field field, IIOContainer ioContainer, int length) { }

        public void WriteToIO(Field field, IIOContainer ioContainer, ref Core.Struct str) { }

        public virtual void WriteToIOBin(Field field, IIOContainer ioContainer, ref Core.Struct str, int index = 0) { }

        public virtual void NilToIOBin(Field field, IIOContainer ioContainer, int index = 0) { }

        public virtual string ToString(object rawIOObject) => string.Empty;
    }
}
