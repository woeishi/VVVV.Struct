using System;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.Graph;
using VVVV.Hosting;
using VVVV.Hosting.IO;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.Core.Logging;

namespace VVVV.Struct
{
	#region PluginInfo
	[PluginInfo(Name = "Info", 
				Category = "Struct", 
				Help = "Basic template with a dynamic amount of in- and outputs", 
				Tags = "")]
	#endregion PluginInfo
	public class InfoNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Input("Input")]
		public ISpread<Struct> FInput;

		[Output("Struct Name")]
		public ISpread<string> FName;
		#endregion fields & pins

		public void OnImportsSatisfied()
		{
			StructManager.DefinitionsChanged += DefinitionsChanged;
		}

		private void DefinitionsChanged(object sender, Definition definition)
		{
			var key = sender as string;
		}
		
		public void Evaluate(int spreadMax) 
		{
			if ((FInput.SliceCount == 1) && (FInput[0] == null))
			{
				FName.SliceCount = 0;
			}
			else
			{
				FName.SliceCount = FInput.SliceCount;
				
				for (int i=0; i<FInput.SliceCount; i++)
				{
                    try
                    {
                        FName[i] = FInput[i].Key;
                    }
                    catch
                    {
                        FName[i] = "";
                    }
				}
				
			}
		}
	}
	
//	[PluginInfo(Name = "PinTypes", 
//				Category = "Struct", 
//				Help = "", 
//				Tags = "")]
	public class PinTypesNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		#pragma warning disable 169, 649
		[Input("Get Selected", IsBang = true)]
		ISpread<bool> FGet;
		
		[Output("Pin Name")]
		ISpread<string> FNames;
		
		[Output("Type Name")]
		ISpread<string> FTypeName;
		
		[Output("Type")]
		ISpread<Type> FType;
		
		[Import()]
		IHDEHost FHDE;
		
		[Import()]
		ILogger FLogger;
		
		[Import()]
		INodeInfoFactory NodeInfoFactory;
		
		[Import()]
		INodeBrowserHost NodeBrowserHost;
		
		INode2[] FSelection;
		#pragma warning restore
		#endregion fields & pins
		
		public void OnImportsSatisfied()
		{
			FSelection = null;
			FHDE.NodeSelectionChanged += NodeSelectionChanged;
		}
		private void NodeSelectionChanged(object sender, NodeSelectionEventArgs e)
		{
			FSelection = e.Nodes;
		}
		
		public void Evaluate(int spreadMax)
		{
			if (FGet[0])
			{
				FNames.SliceCount = 0;
				FTypeName.SliceCount = 0;
				FType.SliceCount = 0;
				foreach (var pin in FSelection[0].Pins)
				{
					if (pin.Direction != PinDirection.Configuration)
					{	
						FNames.Add(pin.Name);
						if (pin.CLRType == null)
							FTypeName.Add(pin.Type);
						else
							FTypeName.Add(pin.CLRType.FullName);
						FType.Add(pin.CLRType);
					}
				}
			}
		}
	}
	
//	[PluginInfo(Name = "AddPinType", 
//				Category = "Struct", 
//				Help = "", 
//				Tags = "",
//				AutoEvaluate = true)]
	public class AddPinTypeNode : IPluginEvaluate
	{
		#region fields & pins
		#pragma warning disable 169, 649
		[Input("Type Name")]
		ISpread<string> FTypeName;
		
		[Input("Type")]
		ISpread<Type> FType;
		
		[Input("Use")]
		ISpread<bool> FUse;
		
		[Import()]
		ILogger FLogger;
		#pragma warning restore
		#endregion fields & pins
		
		public void Evaluate(int spreadMax)
		{
			for (int i=0; i<spreadMax; i++)
			{
				if (FUse[i])
				{
					try
					{
						if (!StructTypeMapper.Mappings.ContainsKey(FTypeName[i]))
						{
							StructTypeMapper.Mappings.Add(FTypeName[i],FType[i]);
						}
					}
					catch (Exception e)
					{
						FLogger.Log(e);
					}
					
				}
			}
		}
	}
}