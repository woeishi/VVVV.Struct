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
        public static Definition CreateDefinition(StructDefinitionNode node)
        {
            Definition def = new Definition(node.FConfigStructName[0]);
            def.HandlerPath = node.FNodePath;

            int spreadmax = node.FName.CombineWith(node.FDatatype).CombineWith(node.FDefault);
            for (int i = 0; i < spreadmax; i++)
                if (!string.IsNullOrEmpty(node.FName[i]))
                    def.AddProperty(new Property(node.FName[i].Trim(), StructTypeMapper.Map(node.FDatatype[i].Trim()), node.FDefault[i].Trim()));

            Definitions[def.Key] = def;

            //update enum
            var structDefs = Definitions.Keys.ToArray();
            if (structDefs.Length > 0)
                EnumManager.UpdateEnum("StructDefinitionNames", structDefs[0], structDefs);

            //raise event
            var handler = DefinitionsChanged;
            if (handler != null)
            {
                handler(def.Key, def);
            }

            return def;
        }

        /// <summary>
        /// list of all definition nodes
        /// </summary>
        static List<StructDefinitionNode> definitionNodes = new List<StructDefinitionNode>();
        public static IEnumerable<string> DefinitionNodes
        {
            get { return definitionNodes.Select(n => n.FNodePath); }
        }
        public static void Register(StructDefinitionNode node)
        {
            node.Disposing += DefinitionNode_Disposing;
            definitionNodes.Add(node);
        }
        private static void DefinitionNode_Disposing(object sender, EventArgs e)
        {
            StructDefinitionNode node = (StructDefinitionNode)sender;
            definitionNodes.Remove(node);
            node.Disposing -= DefinitionNode_Disposing;
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
