using System;

namespace VVVV.Struct.Factories
{
    public class RuntimeFieldTypeRegistry : BaseFieldTypeRegistry
    {
        public override string ContainerType => "Stream";

        public override bool AddAssembly(System.Reflection.Assembly a) => !a.IsDynamic;

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
                    try
                    {
                        if (!a.IsDynamic)
                        {
                            type = a.GetType(typestring, false, true);
                            //static is also abstract&sealed
                            // \..\ is an exception to avoid loading from falsly loaded assemblies (notui)
                            if (type != null && (!type.IsAbstract) && (!a.Location.Contains(@"\..\")))
                            {
                                FMappings.Add(type.FullName.ToLower(), type);
                                return true;
                            }
                        }
                        
                    }
                    catch (Exception e)
                    {
                        System.Diagnostics.Debug.WriteLine(e);
                    }
                }
                type = null;
                return false;
            }
        }
    }
}