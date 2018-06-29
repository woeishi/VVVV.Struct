using System;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

using VVVV.Struct.Core;

namespace VVVV.Struct.Factories
{
    public class DX11ResourceContainerRegistry : SpreadContainerRegistry
    {
        public DX11ResourceContainerRegistry() : base() {}

        public override string ContainerType => "Spread.DX11Resource";

        public override void OnSync(IIOContainer ioContainer)
        {
            dynamic self = (ioContainer.Factory.PluginHost.Plugin as IPluginContainer).PluginBase;
            if (self.RenderRequest != null)
            {
                self.RenderRequest(self, ioContainer.Factory.PluginHost);
            }
        }

        public override void SetLength(Field field, IIOContainer ioContainer, int length)
        {
            var s = ioContainer.RawIOObject as ISpread;
            int diff = length - s.SliceCount;
            if (diff < 0)
            {
                for (int r = s.SliceCount-1; r>=length; r--)
                {
                    dynamic item = s[r];
                    item.Dispose();
                }
            }
            s.SliceCount = length;
            if (diff > 0)
            {
                for (int a = s.SliceCount; a<length; a++)
                {
                    s[a] = Activator.CreateInstance(field.FieldType);
                }
            }
        }

        protected override void WriteToIO<T>(Field field, ISpread<T> source, ISpread<T> destination)
        {
            destination.AssignFrom(source);
            dynamic src = source;
            dynamic dst = destination;

            for (int i = 0; i < src.SliceCount; i++)
            {
                dst[i].Assign(src[i]);
            }
        }
    }
}
