using System;
using System.Collections.Generic;
using System.Reflection;
using VVVV.PluginInterfaces.V2;
using VVVV.Struct.Core;

namespace VVVV.Struct.Factories
{
    public class VLFieldTypeRegistry : IFieldTypeRegistry
    {
        protected Dictionary<string, Type> FMappings;

        public virtual string ContainerType => "Stream";
        public IContainerRegistry ContainerRegistry { get; set; }

        public event EventHandler<string> TypeUpdated;

        List<Assembly> FVLAss;
        IHDEHost FHDE;

        public VLFieldTypeRegistry(IHDEHost hde)
        {
            FMappings = new Dictionary<string, Type>();
            FVLAss = new List<Assembly>();
            FHDE = hde;
            FHDE.MainLoop.OnPrepareGraph += MainLoop_OnPrepareGraph;
        }

        void MainLoop_OnPrepareGraph(object sender, EventArgs e)
        {
            foreach (var af in FHDE.AddonFactories)
            {
                if (af.JobStdSubPath == "vl")
                {
                    dynamic vlaf = af;
                    dynamic host = vlaf.GetType().GetProperty("Host", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(vlaf);

                    object runtimehost = null;
                    try
                    {
                        runtimehost = host.GetType().GetProperty("RuntimeHost").GetValue(host);
                    }
                    catch
                    {
                        runtimehost = host.GetType().GetField("RuntimeHost").GetValue(host);
                    }

                    if (runtimehost != null)
                    {
                        EventInfo updateEvent = runtimehost.GetType().GetEvent("Updated");
                        var mInfo = this.GetType().GetMethod("SyncAssembly", BindingFlags.Instance | BindingFlags.NonPublic);
                        var d = Delegate.CreateDelegate(updateEvent.EventHandlerType, this, mInfo, true); 
                        updateEvent.AddEventHandler(runtimehost, d);
                        FHDE.MainLoop.OnPrepareGraph -= MainLoop_OnPrepareGraph;
                    }
                }
            }
        }

        public bool AddAssembly(Assembly a)
        {
            if (a.IsDynamic && a.FullName.Contains("VL.Dynamic"))
            {
                if (FVLAss.Count == 0)
                    FVLAss.Add(a);
                else
                    FVLAss.Insert(0, a);
                return true;
            }
            return false;
        }

        void SyncAssembly(object sender, dynamic e)
        {
            foreach (var kv in FMappings)
            {
                var t = FVLAss[0]?.GetType(kv.Key, false, true);
                if (t != null && t != FMappings[kv.Key])
                {
                    FMappings[kv.Key] = t;
                    TypeUpdated?.Invoke(this, kv.Key);
                    return;
                }
            }
        }

        public virtual bool StringToType(string typestring, out Type type)
        {
            var key = typestring.ToLower();
            if (FMappings.ContainsKey(key) && FMappings[key] != null)
            {
                type = FMappings[key];
                return true;
            }

            foreach (var vl in FVLAss)
            {
                type = vl?.GetType(key, false, true);
                if (type != null)
                {
                    FMappings.Add(key, type);
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