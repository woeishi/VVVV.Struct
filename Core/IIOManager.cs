using System;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Struct.Core
{
    public interface IIOManager : IDisposable
    {
        bool IsBinSized { get; set; }

        event EventHandler<string> DeclarationChanged;
        event EventHandler IOsChanged;

        void ResetCache(bool reset = true);
        int CalculateSpreadMax();

        Struct CreateStruct(Declaration declaration);
        Struct CreateStruct(Struct other);
        void ReadFromIOBins(ISpread<Struct> structs);
        void ReadFromIOs(ref Struct str);
        void SetIOs(string fieldString);
        void SetLengthAllIOs(int length);

        void WriteToIOBins(ref Struct str, int index = 0);
        void WriteToIOs(ref Struct str);
    }
}