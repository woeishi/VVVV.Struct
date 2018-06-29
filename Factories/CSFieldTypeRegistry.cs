using System;
using System.Collections.Generic;
using System.Reflection;
using VVVV.Core.Model;
using VVVV.Core.Model.CS;

using VVVV.Struct.Core;

namespace VVVV.Struct.Factories
{
    public class CSFieldTypeRegistry : IFieldTypeRegistry
    {
        public virtual string ContainerType => "Stream";
        public IContainerRegistry ContainerRegistry { get; set; }

        protected Dictionary<string, Type> FMappings;

        ISolution FSolution;
        Dictionary<CSProject,Assembly> FCSProjects;
        Dictionary<string, CSProject> FTypeToProject;

        public event EventHandler<string> TypeUpdated;

        public CSFieldTypeRegistry(ISolution solution)
        {
            FMappings = new Dictionary<string, Type>();

            FSolution = solution;
            FCSProjects = new Dictionary<CSProject, Assembly>();
            FTypeToProject = new Dictionary<string, CSProject>();

            FSolution.Projects.Added += (c, i) =>
            {
                var cs = i as CSProject;
                if (cs != null)
                {
                    AddOrUpdateAssembly(cs);
                    //better than compiled successfully, now also loaded
                    cs.CompileCompleted += (o, e) => { AddOrUpdateAssembly(cs); };
                }
            };
        }

        public bool AddAssembly(Assembly a)
        {
            if (!a.IsDynamic)
            {
                foreach (var p in FSolution.Projects)
                {
                    var cs = p as CSProject;
                    if (cs != null && cs.AssemblyLocation == a.Location)
                    {
                        if (!FCSProjects.ContainsKey(cs))
                            FCSProjects.Add(cs, a);
                        return true;
                    }
                }
            }
            return false;
        }

        void AddOrUpdateAssembly(CSProject cs)
        {
            if (cs.AssemblyLocation != null)
            {
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        if ((!a.IsDynamic) && a.Location == cs.AssemblyLocation)
                        {
                            if (FCSProjects.ContainsKey(cs))
                                UpdateAssembly(cs, a);
                            else
                                FCSProjects.Add(cs, a);
                        }
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }
                }
            }
        }

        void UpdateAssembly(CSProject cs, Assembly a)
        {
            FCSProjects[cs] = a;
            foreach (var kv in FTypeToProject)
            {
                if (kv.Value == cs)
                {
                    var t = a.GetType(kv.Key, false, true);
                    if (t != null)
                    {
                        FMappings[kv.Key] = t;
                        TypeUpdated?.Invoke(this, kv.Key);
                    }
                }
            }
        }

        public virtual bool StringToType(string typestring, out Type type)
        {
            var key = typestring.ToLower();
            if (FMappings.ContainsKey(key))
            {
                type = FMappings[key];
                return true;
            }

            var css = FCSProjects.Keys;
            foreach (var cs in css)
            {
                var a = FCSProjects[cs];
                type = a.GetType(key, false, true);
                if (type != null) //assembly is relevant
                {
                    if (a.ReflectionOnly)
                    {
                        //Assembly.Load exits with FileNotFound Exception the first time
                        FCSProjects[cs] = Assembly.LoadFrom(a.Location);
                        type = FCSProjects[cs].GetType(key, false, true);
                    }
                    FMappings.Add(key, type);
                    FTypeToProject.Add(key, cs);

                    return true;
                }
            }
            type = null;
            return false;
        }

        public virtual bool TypeToString(Type type, out string typestring)
        {
            typestring = type.Name.ToLower();
            if (FMappings.ContainsValue(type))
            {
                foreach (var kv in FMappings)
                {
                    if (type == kv.Value)
                    {
                        typestring = kv.Key;
                        return true;
                    }
                }
            }
            return false;
        }
    }
}