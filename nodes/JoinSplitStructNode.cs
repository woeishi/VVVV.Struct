#region usings
using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

using VVVV.Core.Logging;
#endregion usings

namespace VVVV.Struct
{
	public abstract class StructNode : IPluginEvaluate, IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Config("Definition",  IsSingle = true, EnumName = "StructDefinitionNames")]
		public IDiffSpread<EnumEntry> FDefinitionIn;
		
		[Import()]
		public IIOFactory FIOFactory;
		
		[Import()]
		public ILogger FLogger;
		
		[Import()]
		public IPluginHost2 FHost;
		
		[Import()]
		public IHDEHost FHDE;
		
		protected Dictionary<string,IIOContainer> FPins = new Dictionary<string,IIOContainer>();
		protected string FStructDefName;
		private bool FIsJoin;
		#endregion
		
		public StructNode(bool isJoin)
		{
			FIsJoin = isJoin;
		}
		
		protected abstract void BaseOnImportSatisfied();
		public void OnImportsSatisfied()
		{
			BaseOnImportSatisfied();
			
			StructManager.DefinitionsChanged += DefinitionsChanged;
			FDefinitionIn.Changed += DefinitionSelectionChanged;
		}
		
		protected abstract void RefreshStruct(Struct str);
		
		private void DefinitionsChanged(object sender,Definition definition)
		{
			if ((FStructDefName == definition.Key)) //(FStructDefName != null) &&
			{
				UpdateDefinition(definition);
			}
		}
		
		private void DefinitionSelectionChanged(IDiffSpread<EnumEntry> sender)
		{
			string key = sender[0].Name;
			FStructDefName = key;
			if (StructManager.Definitions.ContainsKey(key))
			{
				var layout = StructManager.Definitions[FStructDefName];
				UpdateDefinition(layout);
			}
		}
		
		private void UpdateDefinition(Definition definition)
		{
			var s = CreatePins(definition);
			RefreshStruct(s);
		}
		
		private Struct CreatePins(Definition definition)
		{
			Struct s = new Struct(FStructDefName);
			var pins =  new Dictionary<string,IIOContainer>();
			foreach (var entry in definition.Types)
			{
				string key = entry.Key+entry.Value; 
				if (FPins.ContainsKey(key))
				{
					pins.Add(key, FPins[key]);
					s.Data.Add(key, FPins[key]);
					FPins.Remove(key);
					
					if (!FIsJoin) //create output bin size
					{
						string binKey = "Bin"+key;
						pins.Add(binKey, FPins[binKey]);
						FPins.Remove(binKey);
					}
				}
				else
				{
					Type pinType = typeof(ISpread<>).MakeGenericType(entry.Value);
					IOAttribute attr;
					if (FIsJoin)
					{
						attr = new InputAttribute(entry.Key);
						attr = TrySetDefault(attr as InputAttribute, entry.Value, definition.Defaults[entry.Key]);
					}
					else
						attr = new OutputAttribute(entry.Key);
					
					var pin = FIOFactory.CreateIOContainer(pinType, attr);
					pins.Add(key,pin);
					s.Data.Add(key, pin);

					if (!FIsJoin) //create output bin size
					{
						attr.Name = attr.Name + " Bin Size";
						attr.Visibility = PinVisibility.OnlyInspector;
						pinType = typeof(ISpread<>).MakeGenericType(typeof(int));
						pins.Add("Bin"+key,FIOFactory.CreateIOContainer(pinType,attr));
					}
				}
			}
			
			foreach (var oldPin in FPins.Values)
				oldPin.Dispose();
			
			FPins = pins;
			return s;
		}
		
		
		private InputAttribute TrySetDefault(InputAttribute attr, Type type, string defaultString)
		{
			if (!string.IsNullOrEmpty(defaultString))
			{
				double doubleDefault = 0;
				switch(type.ToString())
				{
					case "System.Boolean":
						bool boolDefault = false;
						if (bool.TryParse(defaultString,out boolDefault))
							attr.DefaultBoolean = boolDefault;
						break;
					case "System.Double":
					case "System.Single":
					case "System.Int32":
						if (double.TryParse(defaultString, out doubleDefault))
							attr.DefaultValue = doubleDefault;
						break;
					case "System.String":
						attr.DefaultString = defaultString;
						break;
					case "VVVV.Utils.VMath.Vector2D":
					case "VVVV.Utils.VMath.Vector3D":
					case "VVVV.Utils.VMath.Vector4D":
						var vectorString = defaultString.Split(new char[]{','},StringSplitOptions.RemoveEmptyEntries);
						attr.DefaultValues = new double[vectorString.Length];
						for(int i=0; i<vectorString.Length; i++)
						{
							if (double.TryParse(vectorString[i].Trim(), out doubleDefault))
								attr.DefaultValues[i] = doubleDefault;

						}
						break;
					case "VVVV.Utils.VColor.RGBAColor":
						var colorString = defaultString.Split(new char[]{','},StringSplitOptions.RemoveEmptyEntries);
						attr.DefaultColor = new double[colorString.Length];
						for(int i=0; i<colorString.Length; i++)
						{
							if (double.TryParse(colorString[i].Trim(), out doubleDefault))
								attr.DefaultColor[i] = doubleDefault;

						}
						break;
						
				}
			}
			return attr;
		}
		
		/// <summary>
		/// issues HDEHost to set a descriptiv name; hack, since Labelpin doesn't work 
		/// </summary>
		/// <param name="key">string to set</param>
		private void SetDescName(string key)
		{
			string xml = string.Format("<PATCH><NODE id=\"{0}\"><PIN pinname=\"Descriptive Name\" values=\"{1}\"></PIN></NODE></PATCH>",FHost.GetID(), key);
			FHDE.SendXMLSnippet(FHost.ParentNode.GetNodeInfo().Filename,xml,true);
		}
		
		protected abstract void BaseEvaluate(int spreadmax);
		public void Evaluate(int spreadMax)
		{
			BaseEvaluate(spreadMax);
		}
		
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Join", Category = "Struct", Help = "creates inputs along the selected definition", Author = "woei", AutoEvaluate = true, Tags = "")]
	#endregion PluginInfo
	public class StructJoinNode : StructNode,  IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Output("Output")]
		public ISpread<Struct> FOutput;
		
		#endregion fields & pins
		public StructJoinNode() : base(true) {}
		
		protected override void BaseOnImportSatisfied()
		{
			FOutput[0] = null;
		}
		protected override void RefreshStruct(Struct str)
		{
			FOutput[0] = str;
		}
		
		protected override void BaseEvaluate(int spreadMax) {}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Split", Category = "Struct", Help = "creates inputs along the selected definition", Author = "woei", AutoEvaluate = true, Tags = "")]
	#endregion PluginInfo
	public class StructSplitNode : StructNode,  IPartImportsSatisfiedNotification
	{
		#region fields & pins
		[Input("Input")]
		public ISpread<Struct> FInput;
		
		[Input("Match", EnumName="OccurenceMode", IsSingle = true)]
		public ISpread<EnumEntry> FMatch;
		
		[Output("Status", Order = int.MaxValue, Visibility = PinVisibility.OnlyInspector)]
		public ISpread<string> FStatus;
		#endregion fields & pins
		public StructSplitNode() : base(false) {}
	
		protected override void BaseOnImportSatisfied() {}
		protected override void RefreshStruct(Struct str) {}
		
		private void WritePin(Dictionary<string,IIOContainer> data, Dictionary<string,int> offset)
		{
			foreach(var entry in data)
			{
				var inPin = entry.Value.RawIOObject as ISpread;
				var outPin = FPins[entry.Key].RawIOObject as ISpread;
				outPin.SliceCount += inPin.SliceCount;
				for (int i=0; i<inPin.SliceCount; i++)
				{
					outPin[i+offset[entry.Key]] = inPin[i];
				}
				offset[entry.Key] += inPin.SliceCount;
				
				//set Bin Size
				var binOutPin = FPins["Bin"+entry.Key].RawIOObject as ISpread;
				binOutPin.SliceCount += 1;
				binOutPin[binOutPin.SliceCount-1] = inPin.SliceCount;
			}
		}
		
		protected override void BaseEvaluate(int spreadMax)
		{
			if (FInput.SliceCount>0)
			{
				if ((FInput[0] != null) && (!string.IsNullOrEmpty(FStructDefName)))
				{
					List<int> hits = new List<int>();
					for (int i=0; i<FInput.SliceCount; i++)
						if (FInput[i]!=null && FInput[i].Key == FStructDefName)
							hits.Add(i);
					
					if (hits.Count>0)
					{
						Dictionary<string, int> binOffset = new Dictionary<string, int>();
						foreach (var pin in FPins)
						{
							(pin.Value.RawIOObject as ISpread).SliceCount = 0;
							binOffset[pin.Key] = 0;
						}
						
						if (FMatch[0].Index == 0)
							WritePin(FInput[hits[0]].Data, binOffset);
						else if (FMatch[0].Index == 1)
							WritePin(FInput[hits[hits.Count-1]].Data, binOffset);
						else
						{
							foreach (int id in hits)
							{
								WritePin(FInput[id].Data, binOffset);
							}
						}
						FStatus[0] = "OK";
					}
					else
					{
						FStatus[0] = "No matching Definition";
					}
					
				}
				else
					FStatus[0] = "Input null";
			}
		}
	}
}
