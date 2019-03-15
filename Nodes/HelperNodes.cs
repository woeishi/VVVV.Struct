using System;
using System.Reflection;
using System.ComponentModel.Composition;

using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

namespace VVVV.Struct.Nodes
{
    

    #region PluginInfo
    [PluginInfo(Name = "Type", Category = "Struct", Author = "woei", AutoEvaluate = true,
        Help = "Gets type information of the connected input.")]
    #endregion PluginInfo
    public class GetFieldtypeNode : IPluginEvaluate
    {
        [Input("Input")]
        public INodeIn FInput;

        [Input("Update", IsBang = true, IsSingle = true)]
        public IDiffSpread<bool> FUpdate;

        [Output("Name")]
        public ISpread<string> FName;

        [Output("Assembly")]
        public ISpread<string> FAssembly;

        [Output("Assembly Location")]
        public ISpread<string> FLocation;

        public void Evaluate(int spreadmax)
        {
            if (FUpdate[0])
            {
                object data;
                FInput.GetUpstreamInterface(out data);
                FName.SliceCount = 0;
                FAssembly.SliceCount = 0;
                FLocation.SliceCount = 0;
                if (data != null)
                {
                    var t = data.GetType();
                   
                    FName.Add(t.FullName);
                    FAssembly.Add(t.Assembly.FullName);
                    FLocation.Add(t.Assembly.Location);


                    if (t.Name.Contains("DynamicTypeWrapper"))
                    {
                        t = t.GetField("Value").GetValue(data).GetType();
                        FName.Add(t.FullName);
                        FAssembly.Add(t.Assembly.FullName);
                        FLocation.Add(t.Assembly.Location);
                    }
                    if (t.IsGenericType)
                    {
                        foreach (var it in t.GetGenericArguments())
                        {
                            FName.Add(it.FullName);
                            FAssembly.Add(it.Assembly.FullName);
                            FLocation.Add(it.Assembly.Location);
                        }
                    }
                }
            }
        }
    }
}
