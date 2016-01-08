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
        [Config("Cache")]
        public IDiffSpread<string> FCache;
        bool cacheLoaded;
        bool cacheNeeded;

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

            FCache.Changed += CacheChanged;
			StructManager.DefinitionsChanged += DefinitionsChanged;
			FDefinitionIn.Changed += DefinitionSelectionChanged;
		}

        protected abstract void RefreshStruct(Struct str);
		
		private void DefinitionsChanged(object sender, Definition definition)
		{
			if ((FStructDefName == definition.Key)) //(FStructDefName != null) &&
			{
				UpdateDefinition(definition);
                FHost.Status = StatusCode.None;
            }
		}

        private void CacheChanged(IDiffSpread<string> spread)
        {
            if (!string.IsNullOrEmpty(spread[0])) //might be first changed event with no data yet
            {
                cacheLoaded = true;
                FCache.Changed -= CacheChanged;
                if (cacheNeeded) //node requested cached definition, but pin wasn't loaded yet
                {
                    cacheNeeded = false;
                    LoadCachedDefinition();
                }
            }
        }

        private void DefinitionSelectionChanged(IDiffSpread<EnumEntry> sender)
        {
            string key = sender[0].Name;
            if (StructManager.Definitions.ContainsKey(key))
            {
                UpdateDefinition(StructManager.Definitions[key]);
                FHost.Status = StatusCode.None;
            }
            else if (cacheLoaded) //user cannot select a definition not present, so must be startup
                LoadCachedDefinition();
            else
                cacheNeeded = true;
		}

        private void LoadCachedDefinition()
        {
            var s = new Serializer<Definition>();
            Definition def = DefinitionSerializer.Read(FCache[0]);
            
            UpdateDefinition(def);
            FHost.Status = StatusCode.HasInvalidData;
        }
		
		private void UpdateDefinition(Definition definition)
		{
            FStructDefName = definition.Key;
            FCache[0] = DefinitionSerializer.Write(definition);
			var s = CreatePins(definition);
			RefreshStruct(s);
		}

		private Struct CreatePins(Definition definition)
		{
			Struct s = new Struct(FStructDefName);
			var pins =  new Dictionary<string,IIOContainer>();
			foreach (var property in definition.Property)
			{
				string key = property.Name+property.Datatype; 
				if (FPins.ContainsKey(key))
				{
					pins.Add(key, FPins[key]);
					s.Data.Add(key, FPins[key].RawIOObject);
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
					Type pinType = typeof(ISpread<>).MakeGenericType(property.Datatype);
					IOAttribute attr;
					if (FIsJoin)
					{
						attr = new InputAttribute(property.Name);
						attr = StructUtils.TrySetDefault(attr as InputAttribute, property.Datatype, property.Default.ToString());
					}
					else
						attr = new OutputAttribute(property.Name);
					
					var pin = FIOFactory.CreateIOContainer(pinType, attr);
					pins.Add(key,pin);
					s.Data.Add(key, pin.RawIOObject);

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
		
		private void WriteOutputs(Dictionary<string,object> data)
		{
			foreach(var entry in data)
			{
				var inPin = entry.Value as ISpread;
				var outPin = FPins[entry.Key].RawIOObject as ISpread;
				int offset = outPin.SliceCount;
				outPin.SliceCount += inPin.SliceCount;
				for (int i=0; i<inPin.SliceCount; i++)
					outPin[i+offset] = inPin[i];
				
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
						foreach (var pin in FPins)
							(pin.Value.RawIOObject as ISpread).SliceCount = 0;
						
						if (FMatch[0].Index == 0)
							WriteOutputs(FInput[hits[0]].Data);
						else if (FMatch[0].Index == 1)
							WriteOutputs(FInput[hits[hits.Count-1]].Data);
						else
							foreach (int id in hits)
								WriteOutputs(FInput[id].Data);
						
						FStatus[0] = "OK";
					}
					else
						FStatus[0] = "No matching Definition";
				}
				else
					FStatus[0] = "Input null";
			}
		}
	}
}
