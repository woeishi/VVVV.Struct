using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Struct.Core;

using StructType = VVVV.Struct.Core.Struct;

namespace VVVV.Struct.Nodes
{
    [PluginInfo(Name = "Split", Category = "Struct", Author="woei", AutoEvaluate = true,
        Help = "Get data of a struct (~a named group of arbitrary pins); declaring a new one or selecting an existing one.")]
    public class SplitStructNode : IPluginEvaluate, IStructFieldGetter, IStructDeclarer
    {
        [Input("Input")]
        public ISpread<StructType> FStruct;

        [Input("Hold", DefaultBoolean = true, IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<bool> FHold;

        [Input("Last", IsSingle = true)]
        public ISpread<bool> FLast;

        [Output("Matching Input", Order = int.MaxValue-1, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<bool> FHasMatch;

        [Output("Former Slice", Order = int.MaxValue, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<int> FFormerSlice;

        [Import]
        public IIOManager IOManager { get; set; }

        public Declaration Declaration { get; set; }

        public void Evaluate(int SpreadMax)
        {
            bool dataCopied = false;
            FFormerSlice.SliceCount = 0;
            if (FLast[0])
            {
                for (int i = FStruct.SliceCount - 1; i >= 0; i--)
                {
                    if (FStruct[i]?.Name == Declaration.Name)
                    {
                        var str = FStruct[i];
                        IOManager.WriteToIOs(ref str);
                        dataCopied = true;
                        FFormerSlice.Add(i);
                        break;
                    }
                }
            }
            else
            {
                for (int i = 0; i < FStruct.SliceCount; i++)
                {
                    if (FStruct[i]?.Name == Declaration.Name)
                    {
                        var str = FStruct[i];
                        IOManager.WriteToIOs(ref str);
                        dataCopied = true;
                        FFormerSlice.Add(i);
                        break;
                    }
                }
            }
            if (!(FHold[0] || dataCopied))
                IOManager.SetLengthAllIOs(0);
            FHasMatch[0] = dataCopied;
        }
    }

    [PluginInfo(Name = "SplitAll", Category = "Struct", Author="woei", AutoEvaluate = true,
        Help = @"Get data of all input structs (~named group of arbitrary pins); declaring a new one or selecting an existing one.
                In case you just need the data of one struct, use the simple Split node for better performance")]
    public class SplitAllStructNode : IPluginEvaluate, IStructFieldGetter, IStructDeclarer, IPartImportsSatisfiedNotification
    {
        [Input("Input")]
        public ISpread<StructType> FStruct;

        [Input("Hold", DefaultBoolean = true, IsSingle = true, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<bool> FHold;

        [Output("Matching Input", Order = int.MaxValue-1, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<bool> FHasMatch;

        [Output("Former Slice", Order = int.MaxValue, Visibility = PinVisibility.OnlyInspector)]
        public ISpread<int> FFormerSlice;

        [Import]
        public IIOManager IOManager { get; set; }

        public Declaration Declaration { get; set; }

        public void OnImportsSatisfied() => IOManager.IsBinSized = true;

        public void Evaluate(int SpreadMax)
        {
            bool dataCopied = false;
            FFormerSlice.SliceCount = 0;
            
            if (Declaration != null && FStruct.SliceCount > 0)
            {
                int incr = 0;
                foreach (var s in FStruct)
                {
                    if (s != null && s.Name == Declaration.Name)
                    {
                        var str = s;
                        IOManager.WriteToIOBins(ref str, incr);
                                
                        dataCopied = true;
                        FFormerSlice.Add(incr);
                        incr++;
                    }
                }
                IOManager.SetLengthAllIOs(incr);
            }
            if (!(FHold[0] || dataCopied))
                IOManager.SetLengthAllIOs(0);
            
            FHasMatch[0] = dataCopied;
        }
    }
}