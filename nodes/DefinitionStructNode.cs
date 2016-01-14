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
	public class StructDefinitionNode : IPluginEvaluate, IPartImportsSatisfiedNotification, IDisposable
	{
        #region fields & pins
        #pragma warning disable 649
        [Config("Struct Name Cache", IsSingle = true, Visibility = PinVisibility.False)]
		public IDiffSpread<string> FConfigStructName;
		[Input("Struct Name", IsSingle = true)]
		IDiffSpread<string> FStructNameIn;
		
		[Config("Pin Type Cache", Visibility = PinVisibility.False)]
		public IDiffSpread<string> FDatatype;
		[Input("Pin Type")]
		IDiffSpread<string> FPinTypeIn;
		
		[Config("Pin Name Cache", Visibility = PinVisibility.False)]
		public IDiffSpread<string> FName;
		[Input("Pin Name")]
		IDiffSpread<string> FPinNameIn;
		
		[Config("Pin Default", Visibility = PinVisibility.False)]
		public IDiffSpread<string> FDefault;
		[Input("Pin Default")]
		IDiffSpread<string> FPinDefaultIn;
		
		
		[Output("Definition")]
		ISpread<string> FLocalDef;
		
		[Output("Pin Name")]
		ISpread<string> FLocalName;
		
		[Output("Pin Type")]
		ISpread<string> FLocalType;
		
        [Import()]
        IPluginHost2 FHost;

        [Import()]
        IHDEHost FHDE;
        #pragma warning restore

        internal Definition FDefinition;
        internal string FNodePath;

        internal event EventHandler Disposing;
        #endregion fields & pins

        public void OnImportsSatisfied()
		{
            FNodePath = FHost.GetNodePath(false);

            StructManager.Register(this);

            FLocalDef[0] = string.Empty;
            FLocalName.SliceCount = 0;
            FLocalType.SliceCount = 0;

            StructManager.DefinitionsChanged += SetLocalOutput;

            FConfigStructName.Changed += DefinitionPinshanged;
            FName.Changed += DefinitionPinshanged;
            FDatatype.Changed += DefinitionPinshanged;
            FDefault.Changed += DefinitionPinshanged;

            FHDE.MainLoop.OnPrepareGraph += MainLoop_OnPrepareGraph;
        }

        private void DefinitionPinshanged(IDiffSpread<string> sender)
        {
            if (!string.IsNullOrEmpty(FConfigStructName[0]) &&
                FName != null &&
                FDatatype != null &&
                FDefault != null)
            {
                StructManager.CreateDefinition(this);
            }
        }

        private void MainLoop_OnPrepareGraph(object sender, EventArgs e)
        {
            FConfigStructName.Changed -= DefinitionPinshanged;
            FName.Changed -= DefinitionPinshanged;
            FDatatype.Changed -= DefinitionPinshanged;
            FDefault.Changed -= DefinitionPinshanged;

            FHDE.MainLoop.OnPrepareGraph -= MainLoop_OnPrepareGraph;
        }
		
		private void SetLocalOutput(object sender, Definition definition)
		{
            if (definition.Key == FConfigStructName[0])
            {
                FLocalDef[0] = definition.Key;
                foreach (var property in definition.Property)
                {
                    FLocalName.Add(property.Name);
                    FLocalType.Add(property.Datatype.Name);
                }
            }
		}

        public void Dispose()
        {
            var handler = Disposing;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
		
		// Called when data for any output pin is requested.
		public void Evaluate(int SpreadMax)
        {
            bool definitionChanged = false;
            if (FPinTypeIn.IsChanged)
            {
                FDatatype.AssignFrom(FPinTypeIn);
                definitionChanged = true;
            }
            if (FPinNameIn.IsChanged)
            {
                FName.AssignFrom(FPinNameIn);
                definitionChanged = true;
            }
            if (FPinDefaultIn.IsChanged)
            {
                FDefault.AssignFrom(FPinDefaultIn);
                definitionChanged = true;
            }
            if (FStructNameIn.IsChanged && FStructNameIn.SliceCount != 0)
            {
                if (!string.IsNullOrEmpty(FStructNameIn[0]))
                {
                    FConfigStructName[0] = FStructNameIn[0];
                    definitionChanged = true;
                }
            }
            if (definitionChanged)
                SetDefinition();
        }

        void SetDefinition()
        {
            FLocalDef[0] = string.Empty;
            FLocalName.SliceCount = 0;
            FLocalType.SliceCount = 0;
            StructManager.CreateDefinition(this);
        }
    }
}
