using System;

namespace VVVV.Struct.Factories
{
    public class RuntimeFieldTypeRegistry : BaseFieldTypeRegistry
    {
        public override string ContainerType => "Stream";

        public override bool AddAssembly(System.Reflection.Assembly a) => true;

        public override bool StringToType(string typestring, out Type type)
        {
            typestring = typestring.ToLowerInvariant();
            if (FMappings.ContainsKey(typestring))
            {
                type = FMappings[typestring];
                return true;
            }
            else
            {
                foreach (var a in AppDomain.CurrentDomain.GetAssemblies())
                {
                    type = a.GetType(typestring, false, true);
                    if (type != null && (!type.IsAbstract)) //static is also abstract&sealed
                    {
                        FMappings.Add(type.FullName.ToLower(), type);
                        return true;
                    }
                }
                type = null;
                return false;
            }
        }
    }
}