using System;
using System.Collections.Generic;
using System.Linq;

using VVVV.PluginInterfaces.V2;

namespace VVVV.Struct
{
	/// <summary>
	/// static class to hold the struct definitions
	/// </summary>
	public static class StructManager
	{
		public static void CreateDefinition(string key, ISpread<string> names, ISpread<string> types, ISpread<string> defaults)
		{
			Definition def =  new Definition(key);
			
			int spreadmax = names.CombineWith(types).CombineWith(defaults);
            for (int i = 0; i < spreadmax; i++)
                if (!string.IsNullOrEmpty(names[i]))
                    def.AddProperty(new Property(names[i].Trim(), StructTypeMapper.Map(types[i].Trim()), defaults[i].Trim()));
			
			Definitions[key] = def;
			
			//update enum
			var structDefs = Definitions.Keys.ToArray();
			if(structDefs.Length > 0)
				EnumManager.UpdateEnum("StructDefinitionNames", structDefs[0], structDefs);
			
			//raise event
			var handler = DefinitionsChanged;
			if(handler != null)
			{
				handler(key, def);
			}
		}
		
		/// <summary>
		/// all struct definitions
		/// </summary>
		public static Dictionary<string, Definition> Definitions = new Dictionary<string, Definition>();
		
		/// <summary>
		/// fires when AddStructDefinition was called
		/// </summary>
		public static event EventHandler<Definition> DefinitionsChanged;
	}
}
