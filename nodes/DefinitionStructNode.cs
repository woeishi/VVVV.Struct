#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Reflection;
using System.Linq;

using VVVV.PluginInterfaces.V2;


using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Struct
{
	#region PluginInfo
	[PluginInfo(Name = "Definition", Category = "Struct", Help = "creates a struct definition", Author = "tonfilm, woei", AutoEvaluate = true, Tags = "")]
	#endregion PluginInfo
	public class StructDefinitionAltNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Config("Struct Name", IsSingle = true)]
		public IDiffSpread<string> FConfigStructName;
		[Input("Struct Name", IsSingle = true)]
		public IDiffSpread<string> FStructNameIn;
		
		[Config("Pin Type")]
		public IDiffSpread<string> FConfigPinType;
		[Input("Pin Type")]
		public IDiffSpread<string> FPinTypeIn;
		
		[Config("Pin Name")]
		public IDiffSpread<string> FConfigPinName;
		[Input("Pin Name")]
		public IDiffSpread<string> FPinNameIn;
		
		[Config("Pin Default")]
		public IDiffSpread<string> FConfigPinDefault;
		[Input("Pin Default")]
		public IDiffSpread<string> FPinDefaultIn;
		
		
		[Output("Definition")]
		public ISpread<string> FLocalDef;
		
		[Output("Pin Name")]
		public ISpread<string> FLocalName;
		
		[Output("Pin Type")]
		public ISpread<string> FLocalType;
		
		[Import()]
		public IIOFactory FIOFactory;
		
		[Import()]
		public ILogger FLogger;
		#endregion fields & pins
		
		public void OnImportsSatisfied()
		{
			FConfigStructName.Changed += HandleDefinitionChanged;
			FConfigPinName.Changed += HandleDefinitionChanged;
			FConfigPinType.Changed += HandleDefinitionChanged;
			FConfigPinDefault.Changed += HandleDefinitionChanged;
			
			StructManager.DefinitionsChanged += SetLocalOutput;
			
			FLocalDef.SliceCount = 0;
			FLocalName.SliceCount = 0;
			FLocalType.SliceCount = 0;
		}
		
		private void HandleDefinitionChanged(IDiffSpread<string> sender)
		{
			if(FConfigStructName[0] != "")
				StructManager.CreateDefinition(FConfigStructName[0],FConfigPinName,FConfigPinType,FConfigPinDefault);
		}
		
		private void SetLocalOutput(object sender, Definition definition)
		{
			FLocalDef.SliceCount = 1;
			FLocalDef[0] = definition.Key;
			
			FLocalName.SliceCount = 0;
			FLocalType.SliceCount = 0;
			foreach (var entry in definition.Types)
			{
				FLocalName.Add(entry.Key);
				FLocalType.Add(entry.Value.Name);
			}
		}
		
		// Called when data for any output pin is requested.
		public void Evaluate(int SpreadMax)
		{
			if (FStructNameIn.IsChanged && FStructNameIn.SliceCount!=0)
			{
				if (!string.IsNullOrEmpty(FStructNameIn[0]))
				{
					FConfigStructName[0] = FStructNameIn[0];
				}
			}
			if (FPinTypeIn.IsChanged)
				FConfigPinType.AssignFrom(FPinTypeIn);
			if (FPinNameIn.IsChanged)
				FConfigPinName.AssignFrom(FPinNameIn);
			if (FPinDefaultIn.IsChanged)
				FConfigPinDefault.AssignFrom(FPinDefaultIn);
		}
	}
	

}
