using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Struct.Core;

namespace VVVV.Struct.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "FrameDelay",
                Category = "Struct",
                Help = "Delays the input value one calculation frame, no default on initial frame yet",
                Tags = "",
                Author = "woei")]
    #endregion PluginInfo
    public class FrameDelayStruct : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
    {
        #region fields & pins
        [Input("Input", AutoValidate = false)]
        public ISpread<Core.Struct> FInput;

        [Output("Output", AllowFeedback = true, AutoFlush = false)]
        public ISpread<Core.Struct> FOutput;

        [Import]
        protected IMainLoop FMainloop;

        [Import]
        protected IIOFactory FIOFactory;

        private Spread<Core.Struct> FBuffer;
        #endregion fields & pins

        public void OnImportsSatisfied()
        {
            FMainloop.OnPrepareGraph += FMainloop_OnPrepareGraph;
            FBuffer = new Spread<Core.Struct>(0);
            FOutput.SliceCount = 0;
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
            FOutput.SliceCount = FBuffer.SliceCount;
            for (int i = 0; i < FBuffer.SliceCount; i++)
            {
                if (FBuffer[i] == null)
                    FOutput[i] = null;
                else
                {
                    if (FOutput[i] == null || FOutput[i].Name != FBuffer[i].Name)
                        FOutput[i] = new Core.Struct(FBuffer[i]);
                    foreach (var f in FBuffer[i].Fields)
                        FOutput[i][f] = FBuffer[i].GetClonedData(f);
                }
            }
            FOutput.Flush();

            FInput.Sync();
            FBuffer.SliceCount = FInput.SliceCount;
            for (int i = 0; i < FInput.SliceCount; i++)
            {
                if (FInput[i] == null)
                    FBuffer[i] = null;
                else
                {
                    if (FBuffer[i] == null || FBuffer[i].Name != FInput[i].Name)
                        FBuffer[i] = new Core.Struct(FInput[i]);
                    foreach (var f in FInput[i].Fields)
                        FBuffer[i][f] = FInput[i].GetClonedData(f);
                }
            }
        }
    }
}
