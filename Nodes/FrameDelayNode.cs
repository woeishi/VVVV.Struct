using System;
using System.ComponentModel.Composition;
using VVVV.PluginInterfaces.V2;
using VVVV.Struct.Core;

namespace VVVV.Struct.Nodes
{
    #region PluginInfo
    [PluginInfo(Name = "FrameDelay",
                Category = "Struct",
                Help = "Delays the input value one calculation frame, no default on initial frame",
                Tags = "",
                Author = "woei")]
    #endregion PluginInfo
    public class FrameDelayStruct : IPluginEvaluate, IStructFieldGetter, IPartImportsSatisfiedNotification, IDisposable
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

        [Import]
        public IIOManager IOManager { get; set; }

        private Spread<Core.Struct> FBuffer;

        bool declarationChanged = false;
        Declaration changedDeclaration;
        string oldDeclarationName;
        #endregion fields & pins

        public void OnImportsSatisfied()
        {
            FMainloop.OnPrepareGraph += FMainloop_OnPrepareGraph;
            IOManager.DeclarationChanged += IOManager_DeclarationChanged;
            FBuffer = new Spread<Core.Struct>(0);
            FOutput.SliceCount = 0;
        }

        private void FMainloop_OnPrepareGraph(object sender, EventArgs e)
        {
            if (declarationChanged)
                HandleDeclarationChanged();
            FIOFactory.PluginHost.Evaluate();
        }

        private void IOManager_DeclarationChanged(object declaration, string oldName)
        {
            changedDeclaration = declaration as Declaration;
            oldDeclarationName = oldName;
            declarationChanged = true;
            //not calling HandleDeclarationChanged directly, buffer changes have to be done after upstream node finished evaluation
        }

        void HandleDeclarationChanged()
        {
            bool invalidate = false;
            for (int i = 0; i < FBuffer.SliceCount; i++)
            {
                if (FBuffer[i] != null && 
                    (FBuffer[i].Name == oldDeclarationName || 
                    !FBuffer[i].Equals(changedDeclaration)))
                {
                    if (!invalidate)
                        FInput.Sync();
                    FBuffer[i] = new Core.Struct(FInput[i]);
                    FOutput[i] = new Core.Struct(FInput[i]);
                    invalidate = true;
                }
            }
            declarationChanged = invalidate;
        }

        public void Dispose()
        {
            IOManager.DeclarationChanged -= IOManager_DeclarationChanged;
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
                    if (FOutput[i] == null)
                        FOutput[i] = new Core.Struct(FBuffer[i]);
                    foreach (var f in FBuffer[i].Fields)
                        FOutput[i][f] = FBuffer[i].GetClonedData(f);
                }
            }

            if (!declarationChanged)
            {
                FOutput.Flush();
                FInput.Sync();
            }
                
            FBuffer.SliceCount = FInput.SliceCount;
            for (int i = 0; i < FInput.SliceCount; i++)
            {
                if (FInput[i] == null)
                    FBuffer[i] = null;
                else
                {
                    if (FBuffer[i] == null || FBuffer[i].Name != FInput[i].Name)
                    {
                        FBuffer[i] = new Core.Struct(FInput[i]);
                        FOutput[i] = new Core.Struct(FBuffer[i]);
                        declarationChanged = true;
                    }
                    foreach (var f in FInput[i].Fields)
                        FBuffer[i][f] = FInput[i].GetClonedData(f);
                }
            }
            if (declarationChanged)
            {
                FOutput.Flush();
                declarationChanged = false;
            }
        }
    }
}
