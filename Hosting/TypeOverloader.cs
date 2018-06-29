using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace VVVV.Struct.Hosting
{
    internal class TypeOverloader
    {
        ModuleBuilder FModuleBuilder;

        internal TypeOverloader()
        {
            var appDomain = AppDomain.CurrentDomain;
            var assemblyName = new AssemblyName("VVVV.Struct.Runtime");
            var assemblyBuilder = appDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
            FModuleBuilder = assemblyBuilder.DefineDynamicModule(assemblyName.Name);
        }

        public Type EmitWithInterface(Type baseType, Type newInterface)
        {
            var tb = FModuleBuilder.DefineType(baseType.Name+newInterface.Name, TypeAttributes.Class | TypeAttributes.Public, baseType);
            //copy class attributes from base class
            foreach (var ca in baseType.GetCustomAttributesData())
                tb.SetCustomAttribute(CloneAttributes(ca));

            //apply all inherited interfaces
            foreach (var i in newInterface.GetInterfaces())
                ApplyInterface(i, tb);

            ApplyInterface(newInterface,tb);

            return tb.CreateType();
        }

        void ApplyInterface(Type newInterface, TypeBuilder tb)
        {
            tb.AddInterfaceImplementation(newInterface);
            foreach (var f in newInterface.GetProperties())
            {
                var fb = tb.DefineField(f.Name, f.PropertyType, System.Reflection.FieldAttributes.Public);
                foreach (var ca in f.GetCustomAttributesData())
                    fb.SetCustomAttribute(CloneAttributes(ca));

                EmitInterfaceGetSet(newInterface, tb, fb);
            }

            foreach (var e in newInterface.GetEvents())
            {
                AddEvent(newInterface, e, tb);
               
            }
        }

        //https://stackoverflow.com/questions/752232/implementing-an-interface-on-a-dynamic-type-with-events
        void AddEvent(Type iType, EventInfo interfaceEvent, TypeBuilder proxyBuilder)
        {
            // Event methods attributes
            MethodAttributes eventMethodAttr = MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual | MethodAttributes.NewSlot | MethodAttributes.Final | MethodAttributes.SpecialName;
            MethodImplAttributes eventMethodImpAtr = MethodImplAttributes.Managed | MethodImplAttributes.Synchronized;

            string qualifiedEventName = interfaceEvent.Name;// string.Format("{0}.{1}", iType.Name, interfaceEvent.Name);
            string addMethodName = string.Format("add_{0}", interfaceEvent.Name);
            string remMethodName = string.Format("remove_{0}", interfaceEvent.Name);

            FieldBuilder eFieldBuilder = proxyBuilder.DefineField(qualifiedEventName,
                interfaceEvent.EventHandlerType, System.Reflection.FieldAttributes.Public);

            EventBuilder eBuilder = proxyBuilder.DefineEvent(qualifiedEventName, EventAttributes.None, interfaceEvent.EventHandlerType);

            // ADD method
            MethodBuilder addMethodBuilder = proxyBuilder.DefineMethod(addMethodName, eventMethodAttr, null, new Type[] { interfaceEvent.EventHandlerType });

            addMethodBuilder.SetImplementationFlags(eventMethodImpAtr);

            // We need the 'Combine' method from the Delegate type
            MethodInfo combineInfo = typeof(Delegate).GetMethod("Combine", new Type[] { typeof(Delegate), typeof(Delegate) });

            // Code generation
            ILGenerator ilgen = addMethodBuilder.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldfld, eFieldBuilder);
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(OpCodes.Call, combineInfo);
            ilgen.Emit(OpCodes.Castclass, interfaceEvent.EventHandlerType);
            ilgen.Emit(OpCodes.Stfld, eFieldBuilder);
            ilgen.Emit(OpCodes.Ret);

            // REMOVE method
            MethodBuilder removeMethodBuilder = proxyBuilder.DefineMethod(remMethodName,
                eventMethodAttr, null, new Type[] { interfaceEvent.EventHandlerType });
            removeMethodBuilder.SetImplementationFlags(eventMethodImpAtr);

            MethodInfo removeInfo = typeof(Delegate).GetMethod("Remove", new Type[] { typeof(Delegate), typeof(Delegate) });

            // Code generation
            ilgen = removeMethodBuilder.GetILGenerator();
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldarg_0);
            ilgen.Emit(OpCodes.Ldfld, eFieldBuilder);
            ilgen.Emit(OpCodes.Ldarg_1);
            ilgen.Emit(OpCodes.Call, removeInfo);
            ilgen.Emit(OpCodes.Castclass, interfaceEvent.EventHandlerType);
            ilgen.Emit(OpCodes.Stfld, eFieldBuilder);
            ilgen.Emit(OpCodes.Ret);

            // Finally, setting the AddOn and RemoveOn methods for our event
            eBuilder.SetAddOnMethod(addMethodBuilder);
            eBuilder.SetRemoveOnMethod(removeMethodBuilder);

            // Implement the method from the interface
            proxyBuilder.DefineMethodOverride(addMethodBuilder, iType.GetMethod("add_" + interfaceEvent.Name));

            // Implement the method from the interface
            proxyBuilder.DefineMethodOverride(removeMethodBuilder, iType.GetMethod("remove_" + interfaceEvent.Name));

        }


        void EmitInterfaceGetSet(Type iType, TypeBuilder tb, FieldBuilder fb)
        {
            MethodAttributes getSetAttr = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
            string name = fb.Name;//.Substring(1);
            var g = iType.GetMethod("get_" + name);
            if (g != null)
            {
                var getter = tb.DefineMethod("get_" + name, getSetAttr, fb.FieldType, Type.EmptyTypes);
                ILGenerator custNameGetIL = getter.GetILGenerator();
                custNameGetIL.Emit(OpCodes.Ldarg_0);
                custNameGetIL.Emit(OpCodes.Ldfld, fb);
                custNameGetIL.Emit(OpCodes.Ret);
                tb.DefineMethodOverride(getter, g);
            }

            var s = iType.GetMethod("set_" + name);
            if (s != null)
            {
                var setter = tb.DefineMethod("set_" + name, getSetAttr, null, new Type[] { fb.FieldType });
                ILGenerator custNameSetIL = setter.GetILGenerator();
                custNameSetIL.Emit(OpCodes.Ldarg_0);
                custNameSetIL.Emit(OpCodes.Ldarg_1);
                custNameSetIL.Emit(OpCodes.Stfld, fb);
                custNameSetIL.Emit(OpCodes.Ret);
                tb.DefineMethodOverride(setter, s);
            }
        }

        CustomAttributeBuilder CloneAttributes(CustomAttributeData data)
        {
            if (data == null)
                throw new ArgumentNullException("data");

            var constructorArguments = new List<object>();
            foreach (var ctorArg in data.ConstructorArguments)
                constructorArguments.Add(ctorArg.Value);

            var propertyArguments = new List<PropertyInfo>();
            var propertyArgumentValues = new List<object>();
            var fieldArguments = new List<FieldInfo>();
            var fieldArgumentValues = new List<object>();
            foreach (var namedArg in data.NamedArguments)
            {
                var fi = namedArg.MemberInfo as FieldInfo;
                var pi = namedArg.MemberInfo as PropertyInfo;

                if (fi != null)
                {
                    fieldArguments.Add(fi);
                    fieldArgumentValues.Add(namedArg.TypedValue.Value);
                }
                else if (pi != null)
                {
                    propertyArguments.Add(pi);
                    propertyArgumentValues.Add(namedArg.TypedValue.Value);
                }
            }
            return new CustomAttributeBuilder(
              data.Constructor,
              constructorArguments.ToArray(),
              propertyArguments.ToArray(),
              propertyArgumentValues.ToArray(),
              fieldArguments.ToArray(),
              fieldArgumentValues.ToArray());
        }
    }
}
