using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Struct.Core;

using StructType = VVVV.Struct.Core.Struct;


namespace VVVV.Struct.Nodes
{
    //won't work as long as sync has no effect
    //#region PluginInfo
    //[PluginInfo(Name = "S+H",
    //            Category = "Struct",
    //            Help = "Sample and Hold - if set is 1 just passes the input through, but take a sample and hold it, as long as set is 0",
    //            Tags = "",
    //            Author = "woei")]
    //#endregion PluginInfo
    public class SHStruct : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        #region fields & pins
        [Input("Input")]
        public ISpread<StructType> FInput;

        [Input("Set", DefaultBoolean = true)]
        public ISpread<bool> FSet;

        [Output("Output")]
        public ISpread<StructType> FOutput;

        [Import]
        protected IMainLoop FMainloop;

        [Import]
        protected IIOFactory FIOFactory;

        Spread<StructType> FBuffer;
        Spread<bool> FLastSet;
        #endregion fields & pins

        public void OnImportsSatisfied()
        {
            FBuffer = new Spread<StructType>(0);
            FLastSet = new Spread<bool>(0);
            FOutput.SliceCount = 0;
            FMainloop.OnPrepareGraph += FMainloop_OnPrepareGraph;
        }

        private void FMainloop_OnPrepareGraph(object sender, EventArgs e)
        {
            FIOFactory.PluginHost.Evaluate();
        }

        public void Dispose()
        {
            FMainloop.OnPrepareGraph -= FMainloop_OnPrepareGraph;
        }

        public void Evaluate(int spreadMax)
        {
            FLastSet.ResizeAndDismiss(FSet.SliceCount, (int i) => !FSet[i]);
            if (FSet.SliceCount == 1)
            {
                if (FSet[0])
                {
                    FInput.Sync();
                    FOutput.ResizeAndDismiss(spreadMax, (int i) => FInput[i]);
                    FBuffer.ResizeAndDismiss(spreadMax, (int i) => FInput[i] == null ? null : new StructType(FInput[i]));
                    for (int i = 0; i < FInput.SliceCount; i++)
                    {
                        if (FOutput[i] != FInput[i])
                        {
                            FOutput[i] = FInput[i];
                            if (FInput[i] != null)
                                FBuffer[i] = new StructType(FInput[i]);
                        }
                    }
                }
                else if ((FSet[0] != FLastSet[0]) && (!FSet[0]))
                {
                    FOutput.SliceCount = FBuffer.SliceCount;
                    for (int i = 0; i < FInput.SliceCount; i++)
                    {
                        if (FBuffer[i] != null)
                        {
                            FOutput[i] = new StructType(FBuffer[i]);
                            foreach (var f in FBuffer[i].Fields)
                                FOutput[i][f] = FBuffer[i].GetClonedData(f);
                        }
                        else
                            FOutput[i] = null;
                    }
                }
                
            }
            else
            {
                FInput.Sync();
                FOutput.ResizeAndDismiss(spreadMax, (int i) => null);
                FBuffer.ResizeAndDismiss(spreadMax, (int i) => null);
                for (int i = 0; i < spreadMax; i++)
                {
                   
                    if (FSet[i])
                    {
                        if (FOutput[i] != FInput[i])
                        {
                            FOutput[i] = FInput[i];
                            if (FInput[i] != null)
                                FBuffer[i] = new StructType(FInput[i]);
                        }
                    }
                    else if ((FSet[i] != FLastSet[i]) && (!FSet[i]))
                    {
                        if (FBuffer[i] == null)
                            FOutput[i] = null;
                        else
                        {
                            FOutput[i] = new StructType(FBuffer[i]);
                            foreach (var f in FBuffer[i].Fields)
                                FOutput[i][f] = FBuffer[i].GetClonedData(f);
                        }
                    } 
                }
            }
            FLastSet.AssignFrom(FSet);
        }
    }
}
