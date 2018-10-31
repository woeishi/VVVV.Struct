using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Struct.Core;

using StructType = VVVV.Struct.Core.Struct;

namespace VVVV.Struct.Nodes
{
    [PluginInfo(Name = "Join", Category = "Struct", Author="woei", AutoEvaluate = true)]
    public class JoinStructNode : IPluginEvaluate, IStructFieldSetter, IStructDeclarer, IPartImportsSatisfiedNotification
    {
        [Input("Enabled", Order = 500, IsSingle = true, DefaultBoolean = true, Visibility = PinVisibility.OnlyInspector)]
        public IDiffSpread<bool> FEnabled;

        [Output("Output")]
        ISpread<StructType> FStructOut;

        StructType FStruct;

        [Import]
        public IIOManager IOManager { get; set; }

        public Declaration Declaration { get; set; }

        public void OnImportsSatisfied()
        {
            IOManager.IOsChanged += IOManager_IOsChanged;
            FEnabled.Changed += (s) =>
            {
                if (s.SliceCount > 0 && s[0])
                {
                    FStructOut.SliceCount = 1;
                    FStructOut[0] = FStruct;
                }
                else
                    FStructOut.SliceCount = 0;
                FStructOut.Flush();
            };
        }

        private void IOManager_IOsChanged(object sender, EventArgs e)
        {
            FStruct = IOManager.CreateStruct(Declaration);
            IOManager.ReadFromIOs(ref FStruct);
            if (FEnabled.SliceCount > 0 && FEnabled[0])
            {
                FStructOut[0] = FStruct;
                FStructOut.Flush();
            }
        }

        public void Evaluate(int SpreadMax) { }
    }

    [PluginInfo(Name = "Join", Category = "Struct", Version = "Bin", Author="woei", AutoEvaluate = true)]
    public class JoinBinStructNode : IPluginEvaluate, IStructFieldSetter, IStructDeclarer, IPartImportsSatisfiedNotification
    {
        [Input("Enabled", Order = 500, IsSingle = true, DefaultBoolean = true, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<bool> FEnabled;

        [Output("Output")]
        public ISpread<StructType> FStructOut;

        [Import]
        public IIOManager IOManager { get; set; }

        public Declaration Declaration { get; set; }

        public void OnImportsSatisfied()
        {
            FStructOut.SliceCount = 0;
            IOManager.IsBinSized = true;
            IOManager.IOsChanged += IOManager_IOsChanged;
        }

        private void IOManager_IOsChanged(object sender, EventArgs e)
        {
            FStructOut.SliceCount = 0;
        }

        public void Evaluate(int spreadMax)
        {
            if (FEnabled.SliceCount == 0 || FEnabled[0] == false)
                FStructOut.ResizeAndDismiss(0, () => IOManager.CreateStruct(Declaration));

            else if (FEnabled[0])
            {
                spreadMax = IOManager.CalculateSpreadMax();
                FStructOut.ResizeAndDismiss(spreadMax, () => IOManager.CreateStruct(Declaration));
                
                IOManager.ReadFromIOBins(FStructOut);
            }
        }
    }
}
