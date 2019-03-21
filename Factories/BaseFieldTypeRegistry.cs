using System;
using System.Collections.Generic;

using VVVV.Struct.Core;

namespace VVVV.Struct.Factories
{
    public abstract class BaseFieldTypeRegistry : IFieldTypeRegistry
    {
        public virtual string ContainerType => "Null";
        public IContainerRegistry ContainerRegistry { get; set; }
        public event EventHandler<string> TypeUpdated;

        protected Dictionary<string, Type> FMappings;

        public BaseFieldTypeRegistry()
        {
            FMappings = new Dictionary<string, Type>();
        }

        public virtual bool AddAssembly(System.Reflection.Assembly a) => false;

        public virtual bool StringToType(string typestring, out Type type)
        {
            var key = typestring.ToLower();
            if (FMappings.ContainsKey(key))
            {
                type = FMappings[key];
                return true;
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
