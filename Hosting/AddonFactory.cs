using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.ComponentModel.Composition.Hosting;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using VVVV.Core.Model;
using VVVV.Hosting.Factories;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.IO;
using VVVV.PluginInterfaces.V1;
using VVVV.PluginInterfaces.V2;

using VVVV.Struct.Core;

namespace VVVV.Struct.Hosting
{
    [Export(typeof(IAddonFactory))]
    [Export(typeof(StructNodesFactory))]
    [ComVisible(false)]
    public partial class StructNodesFactory : DotNetPluginFactory, IPartImportsSatisfiedNotification
    {
        CompositionContainer FParentContainer;
        IORegistry FIORegistry;
        ISolution FSolution;
        
        [ImportMany]
        public List<IAddonFactory> AddonFactories
        {
            get;
            private set;
        }
        public event EventHandler AddonFactoriesLoaded;

        string FBasePath;

        [ImportingConstructor()]
        public StructNodesFactory(CompositionContainer parentContainer, IORegistry ioreg, IHDEHost hdeHost, ISolution solution) : base(parentContainer, ".dll;.struct")
        {
            FHDEHost = hdeHost;

            FParentContainer = parentContainer;
            FIORegistry = ioreg;
            FSolution = solution;

            FBasePath = Assembly.GetExecutingAssembly().Location.Replace($"FACTORIES{System.IO.Path.DirectorySeparatorChar}STRUCTNODESFACTORY.dll", "");

            Initialize();
        }

        public void OnImportsSatisfied() => AddonFactoriesLoaded?.Invoke(AddonFactories, EventArgs.Empty);

        void ComposeIDeclarationFactory(IDeclarationFactory f)
        {
            FParentContainer.ComposeExportedValue<IDeclarationFactory>(f);
        }

        /// <summary>
        /// set to 'struct' so factory is only notified for nodes inside these folders
        /// </summary>
        public override string JobStdSubPath => "struct";

        protected override void AddSubDir(string dir, bool recursive)
        {
            if (!dir.Contains("struct")) return;
            base.AddSubDir(dir, recursive);
        }

        protected override void DoAddFile(string filename)
        {
            if (!filename.Contains("struct")) return;
            base.DoAddFile(filename);
        }

        protected override IEnumerable<INodeInfo> LoadNodeInfos(string filename)
        {
            //place to filter for our nodes hook into our nodes
            //for the pack itself this check is not necessary
            foreach (var nodeInfo in LoadFromFile(filename))
            {
                if (nodeInfo.Category == "Struct")
                {
                    nodeInfo.InitialWindowSize = new System.Drawing.Size(400, 300); //needs to be aligned to node
                    nodeInfo.Type = NodeType.Plugin;
                    nodeInfo.Factory = this;
                    nodeInfo.CommitUpdate();
                    yield return nodeInfo;
                }
            }
        }

        IEnumerable<INodeInfo> LoadFromFile(string filename)
        {
            var assembly = Assembly.ReflectionOnlyLoadFrom(filename);
            foreach (var type in assembly.GetExportedTypes())
            {
                var isIPluginBase = type.GetInterface("VVVV.PluginInterfaces.V1.IPluginBase") != null;
                if (!type.IsAbstract && !type.IsGenericTypeDefinition && isIPluginBase)
                {
                    var attribute = GetPluginInfoAttributeData(type);
                    if (attribute != null && attribute.NamedArguments.Any(n => n.MemberInfo.Name == "Category" && n.TypedValue.Value.ToString() == "Struct"))
                    {
                        var nodeInfo = ExtractNodeInfoFromAttributeData(attribute, filename);
                        nodeInfo.Arguments = type.FullName;
                        yield return nodeInfo;
                    }
                }
            }
        }

        //straight copy from vvvv-sdk
        private static CustomAttributeData GetPluginInfoAttributeData(Type type)
        {
            var attributes = CustomAttributeData.GetCustomAttributes(type).Where(ca => ca.Constructor.DeclaringType.FullName == typeof(PluginInfoAttribute).FullName).ToArray();
            return attributes.Length > 0 ? attributes[0] : null;
        }

        //straight copy from vvvv-sdk
        INodeInfo ExtractNodeInfoFromAttributeData(CustomAttributeData attribute, string filename)
        {
            var namedArguments = new Dictionary<string, object>();
            foreach (var namedArgument in attribute.NamedArguments)
            {
                namedArguments[namedArgument.MemberInfo.Name] = namedArgument.TypedValue.Value;
            }

            var nodeInfo = FNodeInfoFactory.CreateNodeInfo(
                (string)namedArguments.ValueOrDefault("Name"),
                (string)namedArguments.ValueOrDefault("Category"),
                (string)namedArguments.ValueOrDefault("Version"),
                filename,
                true);

            namedArguments.Remove("Name");
            namedArguments.Remove("Category");
            namedArguments.Remove("Version");

            if (namedArguments.ContainsKey("InitialWindowWidth") && namedArguments.ContainsKey("InitialWindowHeight"))
            {
                nodeInfo.InitialWindowSize = new Size((int)namedArguments["InitialWindowWidth"], (int)namedArguments["InitialWindowHeight"]);
                namedArguments.Remove("InitialWindowWidth");
                namedArguments.Remove("InitialWindowHeight");
            }

            if (namedArguments.ContainsKey("InitialBoxWidth") && namedArguments.ContainsKey("InitialBoxHeight"))
            {
                nodeInfo.InitialBoxSize = new Size((int)namedArguments["InitialBoxWidth"], (int)namedArguments["InitialBoxHeight"]);
                namedArguments.Remove("InitialBoxWidth");
                namedArguments.Remove("InitialBoxHeight");
            }
            else
                nodeInfo.InitialBoxSize = new Size(200, 200);

            if (namedArguments.ContainsKey("InitialComponentMode"))
            {
                nodeInfo.InitialComponentMode = (TComponentMode)namedArguments["InitialComponentMode"];
                namedArguments.Remove("InitialComponentMode");
            }

            foreach (var entry in namedArguments)
            {
                nodeInfo.GetType().InvokeMember((string)entry.Key, BindingFlags.Instance | BindingFlags.Public | BindingFlags.SetProperty, Type.DefaultBinder, nodeInfo, new object[] { entry.Value });
            }

            return nodeInfo;
        }

        protected override bool CreateNode(INodeInfo nodeInfo, IInternalPluginHost nodeHost)
        {
            var baseType = GetPluginType(nodeInfo);
            //nothing special to do, node doesn't carry any of our interfaces
            if (baseType.GetInterface("IStructIO") == null)
            {
                if (nodeInfo.Username.Contains("Struct") || nodeInfo.Filename.Contains(FBasePath))
                    return base.CreateNode(nodeInfo, nodeHost);
                else
                {
                    //this actually shouldn't happen. vvvv thinks this factory should handle the plugin.
                    //throw; 
                    return false;
                }
            }

            else
            {
                ComposeOverloaded(nodeInfo, nodeHost, baseType);
                return true;
            }
        }

        PluginContainer CreatePluginContainer(INodeInfo nodeInfo, IInternalPluginHost nodeHost, Type emitType)
        {
            return new PluginContainer(nodeHost, FIORegistry, FParentContainer, FNodeInfoFactory, this, emitType, nodeInfo);
        }

        protected override bool CloneNode(INodeInfo nodeInfo, string path, string name, string category, string version, out string newFilename)
        {
            //can't handle cloning (yet)
            newFilename = string.Empty;
            return false;
        }

        protected override bool DeleteNode(INodeInfo nodeInfo, IInternalPluginHost pluginHost)
        {
            FInstances.Remove(pluginHost);
            ((pluginHost.Plugin as PluginContainer)?.PluginBase as IStructIO)?.IOManager.Dispose();
            return base.DeleteNode(nodeInfo, pluginHost);
        }
    }
}
