using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;

using VVVV.Struct.Core;

namespace VVVV.Struct.Hosting
{
    public interface IStructEmit : IStructNode
    {
        [Import]
        IIOFactory IOFactory { get; }
    }

    public interface IStructDeclareEmit : IStructEmit
    {
        [Config("Cache", IsSingle = true)]//, DefaultString = "Template{double DoubleIn; string StringIn;}")]
        IDiffSpread<string> Cache { get; set; }

        IntPtr InputWindowHandle { get; set; }
    }
}
