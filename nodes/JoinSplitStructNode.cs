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
        [Config("Definition XML Cache")]
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
		
		protected Dictionary<Property,IIOContainer> FPins = new Dictionary<Property,IIOContainer>();
		protected string FStructDefName;
		private bool FIsJoin;
		#endregion
		
		public StructNode(bool isJoin)
		{
			FIsJoin = isJoin;
		}
		
		public virtual void OnImportsSatisfied()
		{
            FCache.Changed += CacheChanged;
			StructManager.DefinitionsChanged += DefinitionsChanged;
			FDefinitionIn.Changed += DefinitionSelectionChanged;
		}

        protected abstract void RefreshStruct(Struct str);

        public virtual void Evaluate(int spreadMax) { }

        #region Definition cache
        private void LoadCachedDefinition()
        {
            var s = new Serializer<Definition>();
            Definition def = DefinitionSerializer.Read(FCache[0]);

            if (def != null)
                UpdateDefinition(def);
            FHost.Status = StatusCode.HasInvalidData;
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
                    //since loading from cache is delayed as well, definition could be up already
                    if (FDefinitionIn[0] != null && StructManager.Definitions.ContainsKey(FDefinitionIn[0].Name))
                        UpdateDefinition(StructManager.Definitions[FDefinitionIn[0].Name]);
                    else
                        LoadCachedDefinition();
                }
            }
        }
        #endregion Definition cache

        private void DefinitionsChanged(object sender, Definition definition)
		{
			if ((FStructDefName == definition.Key)) //(FStructDefName != null) &&
			{
				UpdateDefinition(definition);
                FHost.Status = StatusCode.None;
            }
		}

        private void DefinitionSelectionChanged(IDiffSpread<EnumEntry> sender)
        {
            string key = sender[0].Name;
            if (key != null)
            {
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
			var pins =  new Dictionary<Property,IIOContainer>();
            int order = 0;
			foreach (var property in definition.Property)
			{
                var binProperty = CreateBinSizeProperty(property);
                if (FPins.ContainsKey(property))
				{
                    FPins[property].GetPluginIO().Order = order;
                    pins.Add(property, FPins[property]);
					s.Data.Add(property, FPins[property].RawIOObject);
					FPins.Remove(property);
                    order++;

                    if (!FIsJoin) //don't forget output bin size
					{
                        FPins[binProperty].GetPluginIO().Order = order;
                        pins.Add(binProperty, FPins[binProperty]);
						FPins.Remove(binProperty);
					}
                    order++;
				}
                else if (!pins.ContainsKey(property))
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

                    attr.Order = order;
					var pin = FIOFactory.CreateIOContainer(pinType, attr);
					pins.Add(property,pin);
					s.Data.Add(property, pin.RawIOObject);
                    order++;
					if (!FIsJoin) //create output bin size
					{
                        attr.Name = binProperty.Name;
                        attr.Order = order;
                        attr.Visibility = PinVisibility.OnlyInspector;
                        pinType = typeof(ISpread<>).MakeGenericType(binProperty.Datatype);
                        pins.Add(binProperty, FIOFactory.CreateIOContainer(pinType, attr));
                    }
                    order++;
                }
                else
                    System.Diagnostics.Debug.WriteLine("Pin creation weirdness at " + FHost.GetNodePath(false));
			}
			
			foreach (var oldPin in FPins.Values)
				oldPin.Dispose();
			
			FPins = pins;
			return s;
		}

        protected Property CreateBinSizeProperty(Property property)
        {
            var result = new Property();
            result.Name = property.Name + " Bin Size";
            result.DatatypeString = "int";
            return result;
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
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Join", Category = "Struct", Help = "creates inputs along the selected definition", Author = "woei", AutoEvaluate = true, Tags = "")]
	#endregion PluginInfo
	public class StructJoinNode : StructNode
	{
        #region fields & pins
        [Input("Enabled", Order = int.MaxValue, IsSingle = true, DefaultBoolean = true, Visibility = PinVisibility.OnlyInspector)]
        public IDiffSpread<bool> FEnabled;

        [Output("Output")]
		public ISpread<Struct> FOutput;

        Struct FStruct;

        #endregion fields & pins
        public StructJoinNode() : base(true) {}
		
		public override void OnImportsSatisfied()
		{
			FOutput[0] = null;
            FEnabled.Changed += FEnabled_Changed;
            base.OnImportsSatisfied();
		}

        private void FEnabled_Changed(IDiffSpread<bool> spread)
        {
            if (spread.SliceCount > 0 && spread[0])
            {
                FOutput.SliceCount = 1;
                FOutput[0] = FStruct;
            }
            else
                FOutput.SliceCount = 0;
        }

        protected override void RefreshStruct(Struct str)
		{
            FStruct = str;
            if (FEnabled.SliceCount > 0 && FEnabled[0])
			    FOutput[0] = str;
		}
	}
	
	#region PluginInfo
	[PluginInfo(Name = "Split", Category = "Struct", Help = "creates inputs along the selected definition", Author = "woei", AutoEvaluate = true, Tags = "")]
	#endregion PluginInfo
	public class StructSplitNode : StructNode
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
	
		protected override void RefreshStruct(Struct str) {}
		
		private void WriteOutputs(Dictionary<Property,object> data)
		{
			foreach(var entry in data)
			{
                var inPin = entry.Value as ISpread;
                var outPin = FPins[entry.Key].RawIOObject as ISpread;
                int offset = outPin.SliceCount;
                outPin.SliceCount += inPin.SliceCount;
                for (int i = 0; i < inPin.SliceCount; i++)
                    outPin[i + offset] = inPin[i];

                //set Bin Size
                var binOutPin = FPins[CreateBinSizeProperty(entry.Key)].RawIOObject as ISpread;
                binOutPin.SliceCount += 1;
				binOutPin[binOutPin.SliceCount-1] = inPin.SliceCount;
			}
		}

		public override void Evaluate(int spreadMax)
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
