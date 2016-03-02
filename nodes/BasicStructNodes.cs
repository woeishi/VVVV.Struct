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
				Help = "outputs basic information on the incoming structs", 
				Author = "woei")]
	#endregion PluginInfo
	public class InfoNode : IPluginEvaluate
	{
		#region fields & pins
		[Input("Input")]
		public ISpread<Struct> FInput;

		[Output("Struct Name")]
		public ISpread<string> FName;

        [Output("Source Node Path")]
        public ISpread<string> FSrcNodePath;

        [Output("Source Node", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<string> FSrcNode;

        [Output("Definition Node Path")]
        public ISpread<string> FDefNodePath;

        [Output("Definition Node", Visibility = PinVisibility.OnlyInspector)]
        public ISpread<string> FDefNode;

        [Import]
        IHDEHost FHDE;
        #endregion fields & pins

        public void Evaluate(int spreadMax) 
		{
			
			FName.SliceCount = FInput.SliceCount;
            FSrcNode.SliceCount = FInput.SliceCount;
            FDefNode.SliceCount = FInput.SliceCount;
			for (int i=0; i<FInput.SliceCount; i++)
			{
                if (FInput[i] != null)
                {
                    FName[i] = FInput[i].Key;
                    var srcPath = FInput[i].SourcePath;
                    if (FSrcNode[i] != srcPath)
                        FSrcNodePath[i] = CreateReadablePath(srcPath);
                    FSrcNode[i] = srcPath;
                    var split = srcPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);

                    var defPath = "/" + split[0] + StructManager.Definitions[FName[i]].HandlerPath;
                    if (FDefNode[i] != defPath)
                        FDefNodePath[i] = CreateReadablePath(defPath);
                    FDefNode[i] = defPath;
                }
                else
                {
                    FName[i] = string.Empty;
                    FSrcNode[i] = string.Empty;
                    FSrcNodePath[i] = string.Empty;
                    FDefNode[i] = string.Empty;
                    FDefNodePath[i] = string.Empty;
                }
			}
			
		}

        string CreateReadablePath(string path)
        {
            var n = FHDE.GetNodeFromPath(path);
            string result = string.Format("{0} [{1}]",n.NodeInfo.Name,n.ID);
            while (n.Parent.Name!="root")
            {
                n = n.Parent;
                if (n.Parent.Name == "root")
                    result = n.Name+"/"+result;
                else
                    result = string.Format("{0} [{1}]/{2}",n.Name,n.ID,result);
            }
            return result;
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