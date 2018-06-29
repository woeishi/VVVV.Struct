using VVVV.PluginInterfaces.V2;

namespace VVVV.Struct.Core
{
    public class IOContainer : IIOContainer
    {
        IIOContainer iiocontainer;
        IPin ipin;
        //public bool IsInput { get; }
        public bool IsBinSized { get; }
        public int Order { get; }

        public bool IsSynchronized { get; private set; }

        //IIOContainer
        public object RawIOObject => iiocontainer.RawIOObject;
        public IIOContainer BaseContainer => iiocontainer.BaseContainer;
        public IIOFactory Factory => iiocontainer.Factory;
        public IIOContainer[] AssociatedContainers => iiocontainer.AssociatedContainers;

        //IDisposable
        public void Dispose() => iiocontainer.Dispose();

        public IOContainer(IIOContainer container, bool isInput = true, bool isBinSized = false, int order = 0)
        {
            iiocontainer = container;
            //IsInput = isInput;
            IsBinSized = isBinSized;
            Order = order;
        }

        public bool IsVisible
        {
            get
            { 
                //do this lazy, pin might not be available yet
                ipin = ipin ?? iiocontainer.Factory.PluginHost.GetPin(iiocontainer.GetPluginIO().Name);
                return ipin.Visibility == PinVisibility.True;
            }
        }

        public void SetOrder(int order)
        {
            iiocontainer.GetPluginIO().Order = order;
            if (IsBinSized)
            {
                if (iiocontainer.AssociatedContainers != null)
                    iiocontainer.AssociatedContainers[0].GetPluginIO().Order = order;
                if (iiocontainer.BaseContainer.AssociatedContainers != null)
                    iiocontainer.BaseContainer.AssociatedContainers[0].GetPluginIO().Order = order;
            }
        }

        public void Sync()
        {
            if (!IsSynchronized)
            {
                (iiocontainer.RawIOObject as VVVV.Utils.Streams.ISynchronizable)?.Sync();
                IsSynchronized = true;
            }
        }

        public void ResetCache(bool reset = true)
        {
            IsSynchronized = !reset;
        }

    }
}
