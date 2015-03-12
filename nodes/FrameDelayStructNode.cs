#region usings
using System;
using System.Reflection;
using System.Collections.Generic;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;
using System.ComponentModel.Composition;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Struct
{
	#region PluginInfo
	[PluginInfo(Name = "FrameDelay", 
				Category = "Struct", 
				Help = "Delays the input value one calculation frame, no default on initial frame yet", 
				Tags = "",
				Author = "woei",
				AutoEvaluate = true)]
	#endregion PluginInfo
	public class FrameDelayStruct : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Input("Input")]
		public ISpread<Struct> FInput;

		[Output("Output", AllowFeedback = true)]
		public ISpread<Struct> FOutput;
		
		[Import()]
		public ILogger FLogger;
		
		private Spread<Struct> FBuffer;
		#endregion fields & pins
		
		public void OnImportsSatisfied()
		{
			FBuffer = new Spread<Struct>(0);
			FOutput.SliceCount = 0;
		}
		
		public void Evaluate(int spreadMax) 
		{
			FBuffer.ResizeAndDismiss(FInput.SliceCount, (slice) => { return FInput[slice].DeepCopy(); });
			FOutput.ResizeAndDismiss(FInput.SliceCount, (slice) => { return FInput[slice].DeepCopy(); });
			for (int i=0; i<FInput.SliceCount; i++)
			{
				if (FBuffer[i] == null)
					FOutput[i] = null;
				else if (FOutput[i] == null  || FOutput[i].Key != FBuffer[i].Key)
					FOutput[i] = FBuffer[i].DeepCopy();
				else
					foreach (var e in FBuffer[i].Data)
						FOutput[i].Data[e.Key] = (e.Value as ISpread).Clone();
				
				if (FInput[i] == null)
					FBuffer[i] = null;
				else if (FBuffer[i] == null || FBuffer[i].Key != FInput[i].Key)
					FBuffer[i] = FInput[i].DeepCopy();
				else
					foreach (var e in FInput[i].Data)
						FBuffer[i].Data[e.Key] = (e.Value as ISpread).Clone();
			}
		}
	}
}
