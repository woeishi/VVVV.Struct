using System;
using VVVV.PluginInterfaces.V2;
using System.ComponentModel.Composition;


namespace VVVV.Struct.Nodes
{
    [PluginInfo(Name = "Filter", Category = "Struct", Author = "woei")]
    public class FilterNode : IPluginEvaluate
    {
        #region fields & pins
        [Input("Input")]
        public ISpread<Core.Struct> FInput;

        [Input("Struct Name")]
        public ISpread<string> FName;

        [Output("Output")]
        public ISpread<Core.Struct> FOutput;

        [Output("Former Index")]
        public ISpread<int> FFormerIndex;

        [Output("Filter Index")]
        public ISpread<int> FFilterIndex;
        #endregion

        public void Evaluate(int spreadMax)
        {
            FOutput.SliceCount = 0;
            FFormerIndex.SliceCount = 0;
            FFilterIndex.SliceCount = 0;
            for (int i = 0; i < FInput.SliceCount; i++)
            {
                for (int f = 0; f < FName.SliceCount; f++)
                {
                    if (FInput[i] != null && FInput[i].Name == FName[f])
                    {
                        FOutput.Add(FInput[i]);
                        FFormerIndex.Add(i);
                        FFilterIndex.Add(f);
                    }
                }
            }
        }
    }

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
		public ISpread<Core.Struct> FInput;

		[Output("Struct Name")]
		public ISpread<string> FName;

        //[Output("Source Node Path")]
        //public ISpread<string> FSrcNodePath;

        //[Output("Source Node", Visibility = PinVisibility.OnlyInspector)]
        //public ISpread<string> FSrcNode;

        [Import]
        IHDEHost FHDE;
        #endregion fields & pins

        public void Evaluate(int spreadMax) 
		{
			FName.SliceCount = FInput.SliceCount;
            //FSrcNode.SliceCount = FInput.SliceCount;
			for (int i=0; i<FInput.SliceCount; i++)
			{
                if (FInput[i] != null)
                {
                    FName[i] = FInput[i].Name;
                    //var srcPath = FInput[i].SourcePath;
                    //if (FSrcNode[i] != srcPath)
                    //    FSrcNodePath[i] = CreateReadablePath(srcPath);
                    //FSrcNode[i] = srcPath;
                    //var split = srcPath.Split(new char[] { '/' }, StringSplitOptions.RemoveEmptyEntries);
                }
                else
                {
                    FName[i] = string.Empty;
                    //FSrcNode[i] = string.Empty;
                    //FSrcNodePath[i] = string.Empty;
                }
			}
			
		}

        //string CreateReadablePath(string path)
        //{
        //    var n = FHDE.GetNodeFromPath(path);
        //    string result = string.Format("{0} [{1}]",n.NodeInfo.Name,n.ID);
        //    while (n.Parent.Name!="root")
        //    {
        //        n = n.Parent;
        //        if (n.Parent.Name == "root")
        //            result = n.Name+"/"+result;
        //        else
        //            result = string.Format("{0} [{1}]/{2}",n.Name,n.ID,result);
        //    }
        //    return result;
        //}
	}
}