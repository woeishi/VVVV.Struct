using System;
using System.IO;
using System.Collections.Generic;
using System.Reflection;
using VVVV.Hosting.Interfaces;
using VVVV.Hosting.IO;
using VVVV.PluginInterfaces.V2;

using VVVV.Struct.Core;
using VVVV.Struct.Factories;


namespace VVVV.Struct.Hosting
{
    public partial class StructNodesFactory
    {
        List<IInternalPluginHost> FInstances;
        ConflictManager FConflictMgr;

        TypeOverloader FTypeOverloader;
        Dictionary<Type, Type> FTypeOverloads;

        List<IStructFactory> FStructFactories;
        DeclarationFactory FDeclarationFactory;
        List<IContainerRegistry> FContainerRegs;

        Dictionary<IStructDeclarer,Action> FDescriptiveUpdates;

        void Initialize()
        {
            FInstances = new List<IInternalPluginHost>();
            FConflictMgr = new ConflictManager(FInstances.AsReadOnly());

            FTypeOverloader = new TypeOverloader();
            FTypeOverloads = new Dictionary<Type, Type>();

            FStructFactories = new List<IStructFactory>();

            FContainerRegs = new List<IContainerRegistry>();
            FContainerRegs.Add(new StreamContainerRegistry());
            FContainerRegs.Add(new SpreadContainerRegistry());
            FContainerRegs.Add(new NullContainerRegistry());

            FDeclarationFactory = new DeclarationFactory();
            FDeclarationFactory.DeclarationChanged += FDeclarationFactory_DeclarationChanged;
            ComposeIDeclarationFactory(FDeclarationFactory);

            RegisterFieldtypeRegistry(new PrimitivesFieldTypeRegistry());
            RegisterFieldtypeRegistry(new CSFieldTypeRegistry(FSolution));
            var vlReg = new VLFieldTypeRegistry();
            AddonFactoriesLoaded += vlReg.VLFactoryLoaded;
            RegisterFieldtypeRegistry(vlReg);

            foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
            {
                if (a.FullName.Contains("VVVV.DX11.Core"))
                {
                    string dx11FactoryPath = Path.Combine(FBasePath, $"core{Path.DirectorySeparatorChar}Struct.DX11Factory.dll");
                    try
                    {
                        LoadExternalFactories(dx11FactoryPath);
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }
                }
            }

            RegisterFieldtypeRegistry(new RuntimeFieldTypeRegistry());
            RegisterFieldtypeRegistry(new NullFieldTypeRegistry());

            FDeclarationFactory.Initialize();

            FDescriptiveUpdates = new Dictionary<IStructDeclarer, Action>();
            FHDEHost.MainLoop.OnPrepareGraph += MainLoop_OnPrepareGraph;
        }

        void FDeclarationFactory_DeclarationChanged(object sender, string oldName)
        {
            if (!string.IsNullOrWhiteSpace(oldName)) //is empty if only fieldtype has changed
            {
                var newDeclaration = sender as Declaration;
                foreach (var i in FInstances)
                {
                    var declarer = (i.Plugin as PluginContainer).PluginBase as IStructDeclarer;
                    if (declarer?.Declaration?.Name == oldName)
                    {
                        declarer.Declaration = newDeclaration;
                        (declarer.IOManager as IOManager).UpdateFromDeclaration(newDeclaration);
                        SetDescriptiveName(declarer, newDeclaration.Name);
                    }
                }
            }
        }

        bool RegisterFieldtypeRegistry(IFieldTypeRegistry fieldtypeRegistry)
        {
            foreach (var ctr in FContainerRegs)
            {
                if (ctr.ContainerType == fieldtypeRegistry.ContainerType)
                {
                    fieldtypeRegistry.ContainerRegistry = ctr;
                    FDeclarationFactory.AddFieldTypeRegistry(fieldtypeRegistry);
                    return true;
                }
            }
            return false;
        }

        void LoadExternalFactories(string path)
        {
            var assembly = Assembly.LoadFrom(path);
            foreach (var exportedType in assembly.ExportedTypes)
            {
                if (typeof(IStructFactory).IsAssignableFrom(exportedType))
                {
                    var factory = (IStructFactory)Activator.CreateInstance(exportedType);
                    FStructFactories.Add(factory);
                    FContainerRegs.AddRange(factory.ContainerRegistries);
                    foreach (var ftr in factory.FieldTypeRegistries)
                        RegisterFieldtypeRegistry(ftr);
                }
            }
        }

        void MainLoop_OnPrepareGraph(object sender, EventArgs e)
        {
            foreach (var a in FDescriptiveUpdates.Values)
                a.Invoke();
            FDescriptiveUpdates.Clear();
        }

        void SetDescriptiveName(IStructDeclarer declarer, string text)
        {
            FDescriptiveUpdates[declarer] = () =>
            {
                var nodeHost = (declarer as IStructDeclareEmit).IOFactory.PluginHost;
                var label = nodeHost.GetPin("Descriptive Name")?.GetSpread(); //on create pins are null
                if (label != text)
                {
                    var id = nodeHost.GetID();
                    var msg = $"<PATCH><NODE id=\"{id}\"><PIN pinname=\"Descriptive Name\" slicecount=\"1\" values=\"{text}\"></PIN></NODE></PATCH>";
                    FHDEHost.SendXMLSnippet(nodeHost.ParentNode.GetNodeInfo().Filename, msg, false);
                    //nodeHost.GetPin("Descriptive Name")?.SetSpread(text); //gui doesn't save it this way
                }
            };
        }

        void ComposeOverloaded(INodeInfo nodeInfo, IInternalPluginHost nodeHost, Type baseType)
        {
            //dispose previous plugin
            var plugin = nodeHost.Plugin;
            nodeHost.Plugin = null; //delete delphi side
            ((plugin as PluginContainer)?.PluginBase as IStructIO)?.IOManager.Dispose();
            if (plugin != null) DisposePlugin(plugin);

            //now create new
            var emitType = EmitOverloaded(baseType);
            var container = CreatePluginContainer(nodeInfo, nodeHost, emitType);
            nodeHost.Plugin = container;

            var mgr = (container.PluginBase as IStructIO).IOManager as IOManager;
            mgr.IsInput = baseType.GetInterface("IStructFieldGetter") == null;
            mgr.IOFactory = (container.PluginBase as IStructEmit).IOFactory;
            FHDEHost.MainLoop.OnResetCache += (o, e) => { mgr.ResetCache(); }; //clears sync flag

            var declareEmit = container.PluginBase as IStructDeclareEmit;
            if (declareEmit != null)
                InitializeDeclarer(nodeHost, declareEmit);

            FInstances.Add(nodeHost);
        }

        //struct specifics
        Type EmitOverloaded(Type baseType)
        {
            if (!FTypeOverloads.ContainsKey(baseType))
            {
                Type emitInterface = (baseType.GetInterface("IStructDeclarer") != null) ? typeof(IStructDeclareEmit) : typeof(IStructEmit);
                var emitType = FTypeOverloader.EmitWithInterface(baseType, emitInterface);

                foreach (var isf in FStructFactories)
                    foreach (var i in isf.Interfaces(baseType))
                        emitType = FTypeOverloader.EmitWithInterface(emitType, i);

                FTypeOverloads.Add(baseType, emitType);
                return emitType;
            }
            else
                return FTypeOverloads[baseType];
        }

        void InitializeDeclarer(IInternalPluginHost nodeHost, IStructDeclareEmit declareEmit)
        {
            var declarer = declareEmit as IStructDeclarer;

            var editor = new DeclarationUI(nodeHost, FHDEHost, FDeclarationFactory.FBindingList);
            editor.SelectionChanged += (o, e) => EditorDeclarationSelectionChanged(declarer, e);
            editor.CreateDeclaration += (o, e) => EditorCreateDeclaration(o as DeclarationUI, declarer, e.Name, e.Fields);
            editor.UpdateDeclaration += (o, e) => EditorUpdateDeclaration(o as DeclarationUI, declarer, e.Name, e.Fields);

            nodeHost.Win32Window = editor;
            declareEmit.InputWindowHandle = editor.Handle;

            declarer.IOManager.IOsChanged += (o, e) =>
            {
                var d = declarer.Declaration;
                editor.SetByDeclaration(d);
                if ((o as Declaration) != null)
                {
                    var text = d.ToString();
                    if (text != declareEmit.Cache[0])
                        declareEmit.Cache[0] = text;
                }
            };
            declareEmit.Cache.Changed += (s) => LoadDeclaration(nodeHost, declarer, s[0]); //only for startup

            if (string.IsNullOrWhiteSpace(declareEmit.Cache[0]))
            {
                declareEmit.Cache[0] = FDeclarationFactory.Get("Template").ToString();
                LoadDeclaration(nodeHost, declarer, declareEmit.Cache[0]);
            }
            //else
            //    declareEmit.Cache[0] = declareEmit.Cache[0]; //HACK! making sure to get a callback once vvvv is configuring for the first time
        }

        
        //load from config pin
        void LoadDeclaration(IPluginHost2 nodeHost, IStructDeclarer declarer, string text)
        {
            if (declarer.Declaration == null || declarer.Declaration.ToString() != text) //we are only interested in cache on startup
            {
                var d = FDeclarationFactory["Template"];
                if (!string.IsNullOrWhiteSpace(text))
                {
                    d = FDeclarationFactory.Create(text);
                    var existing = FDeclarationFactory.Get(d.Name);
                    if (existing == null) //is new
                        FDeclarationFactory.Add(d, false);
                    else if (d.Equals(existing)) //equals existing, don't worry
                        d = existing;
                    else
                    {
                        var solution = FConflictMgr.Solve(nodeHost, d, FDeclarationFactory.FDeclarations);
                        switch (solution.SolutionKind)
                        {
                            case ConflictManager.ConflictSolutionKind.Ignore:
                                d = existing;
                                break;
                            case ConflictManager.ConflictSolutionKind.Split:
                                d = FDeclarationFactory.Get(solution.DeclarationName);
                                if (d == null)
                                {
                                    d = FDeclarationFactory.Create(solution.DeclarationName, solution.DeclarationBody);
                                    FDeclarationFactory.Add(d, false);
                                }
                                break;
                            case ConflictManager.ConflictSolutionKind.Overwrite:
                                d = FDeclarationFactory.Create(solution.DeclarationName, solution.DeclarationBody);
                                FDeclarationFactory.Update(existing.Name, d);
                                break;
                        }
                    }
                }
                declarer.Declaration = d;
                (declarer.IOManager as IOManager).UpdateFromDeclaration(d);
                SetDescriptiveName(declarer, d.Name);
            }
        }

        //editor
        void EditorDeclarationSelectionChanged(IStructDeclarer declarer, Declaration declaration)
        {
            if (declarer.Declaration == null || declarer.Declaration.Name != declaration.Name)
            {
                declarer.Declaration = declaration;
                (declarer.IOManager as IOManager).UpdateFromDeclaration(declaration);
                SetDescriptiveName(declarer, declaration.Name);
            }
        }

        void EditorCreateDeclaration(DeclarationUI ui, IStructDeclarer declarer, string newName, string newFields)
        {
            if (!FDeclarationFactory.WellformedDeclarationName(newName))
            {
                ui.AppendError($"Declaration name malformed");
                return;
            }
            Declaration d = FDeclarationFactory.Get(newName);
            if (d == null)
            {
                d = FDeclarationFactory.Create(newName, newFields);
                FDeclarationFactory.Add(d);
                declarer.Declaration = d;
                (declarer.IOManager as IOManager).UpdateFromDeclaration(d);
                SetDescriptiveName(declarer, d.Name);
            }
            else
                ui.AppendError($"Declaration named '{newName}' already exists");
        }

        void EditorUpdateDeclaration(DeclarationUI ui, IStructDeclarer declarer, string newName, string newFields)
        {
            var oldDecl = declarer.Declaration;
            if (oldDecl.Name != newName && FDeclarationFactory.Get(newName) != null)
            {
                ui.AppendError($"Declaration named '{newName}' already exists");
                return;
            }
            else if (!FDeclarationFactory.WellformedDeclarationName(newName))
            {
                ui.AppendError($"Declaration name malformed");
                return;
            }
            var d = FDeclarationFactory.Create(newName, newFields);
            if (!d.Equals(oldDecl))
            {
                FDeclarationFactory.Update(oldDecl.Name, d);
                SetDescriptiveName(declarer, d.Name);
            }
        }
    }
}