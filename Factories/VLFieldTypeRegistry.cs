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

        readonly object lockObj = new object();
        List<Assembly> FVLAss;
        int lastSyncLength = 0;

        public VLFieldTypeRegistry()
        {
            FMappings = new Dictionary<string, Type>();
            FVLAss = new List<Assembly>();
        }

        public void VLFactoryLoaded(object sender, EventArgs e)
        {
            foreach (var af in sender as List<IAddonFactory>)
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
                    }
                }
            }
        }

        public bool AddAssembly(Assembly a)
        {
            if (a.IsDynamic && a.FullName.Contains("VL.Dynamic"))
            {
                lock(lockObj)
                {
                    if (FVLAss.Count == 0)
                        FVLAss.Add(a);
                    else
                        FVLAss.Insert(0, a);
                    return true;
                }
            }
            return false;
        }

        void SyncAssembly(object sender, dynamic e)
        {
            if(FVLAss.Count > 0)
            {
                lock(lockObj)
                {
                    for (int i = 0; i < FVLAss.Count-lastSyncLength; i++)
                    {
                        foreach (var t in FVLAss[i].DefinedTypes)
                        {
                            var typestring = t.FullName.ToLower();
                            if (FMappings.ContainsKey(typestring))
                                FMappings[typestring] = t;
                            TypeUpdated?.Invoke(this, t.FullName.ToLower());
                        }
                    }
                    lastSyncLength = FVLAss.Count;
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
            typestring = type.FullName.ToLower();
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