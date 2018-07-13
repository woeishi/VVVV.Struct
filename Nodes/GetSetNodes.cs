using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Struct.Core;

using StructType = VVVV.Struct.Core.Struct;

namespace VVVV.Struct.Nodes
{
    [PluginInfo(Name = "GetField", Category = "Struct", Author = "woei")]
    public class GetFieldStructNode : IPluginEvaluate, IStructFieldGetter, IPartImportsSatisfiedNotification
    {
        [Config("Fields", IsSingle = true)]
        public IDiffSpread<string> FFieldStrings;

        [Input("Input", Order = -1)]
        public ISpread<StructType> FStructIn;


        [Output("Output")]
        public ISpread<StructType> FStructOut;

        [Output("Name", Order = int.MaxValue-1)]
        public ISpread<string> FNames;

        [Import]
        public IIOManager IOManager { get; set; }

        public void OnImportsSatisfied()
        {
            IOManager.IsBinSized = true;
            IOManager.DeclarationChanged += IOManager_DeclarationChanged;
            FFieldStrings.Changed += (s) => { IOManager.SetIOs(s[0]); };
        }

        private void IOManager_DeclarationChanged(object declaration, string oldName)
        {
            var d = declaration as Declaration;
            for (int i = 0; i < FStructOut.SliceCount; i++)
                if (FStructOut[i]?.Name == d.Name)
                    FStructOut[i] = null;
        }

        public void Evaluate(int SpreadMax)
        {
            FStructOut.ResizeAndDismiss(FStructIn.SliceCount, (int i) => FStructIn[i]);
            FNames.ResizeAndDismiss(FStructIn.SliceCount, (int i) => "null");
            IOManager.SetLengthAllIOs(FStructIn.SliceCount);
            for (int i = 0; i<FStructIn.SliceCount; i++)
            {
                if (FStructOut[i] != FStructIn[i] || FStructOut[i]?.Name != FStructIn[i]?.Name)
                    FStructOut[i] = FStructIn[i];
                if (FStructIn[i] != null)
                {
                    
                    var str = FStructIn[i];
                    IOManager.WriteToIOBins(ref str, i);
                    if (FNames[i] != str.Name)
                        FNames[i] = str.Name;
                }
                else
                    FNames[i] = "null";
            }
        }
    }

    [PluginInfo(Name = "SetField", Category = "Struct", Author = "woei")]
    public class SetFieldStructNode : IPluginEvaluate, IStructFieldSetter, IPartImportsSatisfiedNotification
    {
        [Config("Fields", IsSingle = true)]
        public IDiffSpread<string> FFieldStrings;

        [Input("Input", Order = -1)]
        public ISpread<StructType> FStructIn;
        ISpread<StructType> FLastIn = new Spread<StructType>(0);

        [Input("Enabled", Order = 500, IsSingle = true, DefaultBoolean = true, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<bool> FEnabled;

        [Output("Output")]
        public ISpread<StructType> FStructOut;

        [Import]
        public IIOManager IOManager { get; set; }


        public void OnImportsSatisfied()
        {
            IOManager.IsBinSized = true;
            IOManager.DeclarationChanged += IOManager_DeclarationChanged;
            FFieldStrings.Changed += (s) => { IOManager.SetIOs(s[0]); FStructOut.SliceCount = 0; };
        }

        private void IOManager_DeclarationChanged(object declaration, string oldName)
        {
            var d = declaration as Declaration;
            for (int i=0; i< FStructOut.SliceCount; i++)
                if (FStructOut[i]?.Name == d.Name)
                    FStructOut[i] = null;
        }

        public void Evaluate(int SpreadMax)
        {
            FStructOut.ResizeAndDismiss(FStructIn.SliceCount, 
                (int i) => (FStructIn[i] != null) ? IOManager.CreateStruct(FStructIn[i]) : null );
            FLastIn.ResizeAndDismiss(FStructIn.SliceCount, (int i) => FStructIn[i]);
            if (FEnabled[0])
            {
                for (int i=0; i<FStructIn.SliceCount; i++)
                {
                    if (FStructIn[i] != null)
                    {
                        if (FStructOut[i] == null //declaration changed
                            || FLastIn[i] != FStructIn[i]) //struct with same layout but different source
                        {
                            FStructOut[i] = IOManager.CreateStruct(FStructIn[i]);
                            FLastIn[i] = FStructIn[i];
                        }
                    }
                    else
                        FStructOut[i] = null;
                        
                }
                IOManager.ReadFromIOBins(FStructOut);
            }
        }
    }
}