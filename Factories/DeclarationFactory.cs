using System;
using System.Collections.Generic;
using System.ComponentModel;
using VVVV.Struct.Core;

namespace VVVV.Struct.Factories
{
    public class DeclarationFactory : IDeclarationFactory
    {
        List<IFieldTypeRegistry> FFieldTypeRegs;
        public Dictionary<string, Declaration> FDeclarations;
        public BindingList<Declaration> FBindingList;

        Dictionary<string, Field> FFields;

        public event EventHandler<string> DeclarationChanged;

        public DeclarationFactory()
        {
            FFieldTypeRegs = new List<IFieldTypeRegistry>();
            FDeclarations = new Dictionary<string, Declaration>();
            FBindingList = new BindingList<Declaration>();
            FBindingList.AllowEdit = true;
            FBindingList.AllowNew = true;
            FBindingList.AllowRemove = false;
            FBindingList.RaiseListChangedEvents = true;

            FFields = new Dictionary<string, Field>();
        }

        public Declaration this[string name] => FDeclarations[name];

        public void AddFieldTypeRegistry(IFieldTypeRegistry registry)
        {
            FFieldTypeRegs.Add(registry);
            registry.TypeUpdated += (o, e) => { UpdateField(registry, e); };
        }

        public void Initialize()
        {
            var d = this.Create("Template", "double DoubleIn; string StringIn;");
            this.Add(d);

            AppDomain.CurrentDomain.AssemblyLoad += (o, e) =>
            {
                foreach (var ftr in FFieldTypeRegs)
                {
                    if (ftr.AddAssembly(e.LoadedAssembly))
                    {
                        AssemblyAdded(ftr);
                        return;
                    }
                }
            };
        }

        //---------------------------
        public Declaration Get(string name)
        {
            return FDeclarations.ContainsKey(name) ? FDeclarations[name] : null;
        }

        public Declaration Create(string name, string body)
        {
            var lines = Serializer.ReadBody(body);
            lines = RegisterLines(lines);
            return new Declaration(name, lines);
        }

        public Declaration Create(string text)
        {
            var d = Serializer.ReadDeclaration(text);
            var lines = RegisterLines(d.Lines);
            return new Declaration(d.Name, lines);
        }

        public bool Add(Declaration declaration, bool notify = true)
        {
            if (FDeclarations.ContainsKey(declaration.Name))
                return false;

            FDeclarations.Add(declaration.Name, declaration);
            FBindingList.Add(declaration);

            if (notify)
                DeclarationChanged?.Invoke(declaration, "");
            return true;
        }

        public bool Update(string oldname, Declaration declaration)
        {
            if (FDeclarations[oldname] == declaration)
                return false;
            else
            {
                var oldDecl = FDeclarations[oldname];
                FDeclarations.Remove(oldname);
                var id = FBindingList.IndexOf(oldDecl);
                FBindingList[id] = declaration;
                FDeclarations.Add(declaration.Name, declaration);

                DeclarationChanged?.Invoke(declaration, oldname);
                return true;
            }
        }

        //public string ToJson(Declaration declaration)
        //{
        //    return declaration.ToString();//DeclarationSerializer.Write(declaration);
        //}

        //-------------
        void UpdateField(IFieldTypeRegistry fieldtypeRegistry, string typestring)
        {
            foreach (var k in FFields.Keys)
            {
                if (FFields[k].Typestring == typestring)
                {
                    Type t = null;
                    if (fieldtypeRegistry.StringToType(typestring, out t))
                    {
                        FFields[k].FieldType = t;
                        FFields[k].ContainerType = fieldtypeRegistry.ContainerType;
                        FFields[k].ContainerRegistry = fieldtypeRegistry.ContainerRegistry;
                        fieldtypeRegistry.TypeToString(t, out typestring);
                        FFields[k].Typestring = typestring;

                        FFields[k].InvokeChanged();
                    }
                }
            }
        }

        void AssemblyAdded(IFieldTypeRegistry fieldtypeRegistry)
        {
            foreach (var k in FFields.Keys)
            {
                if (FFields[k].ContainerType == "Null")
                {
                    Type t = null;
                    if (fieldtypeRegistry.StringToType(FFields[k].Typestring, out t))
                    {
                        FFields[k].FieldType = t;
                        FFields[k].ContainerType = fieldtypeRegistry.ContainerType;
                        FFields[k].ContainerRegistry = fieldtypeRegistry.ContainerRegistry;
                        var typestring = FFields[k].Typestring;
                        fieldtypeRegistry.TypeToString(t, out typestring); //normalize typestring
                        FFields[k].Typestring = typestring;

                        FFields[k].InvokeChanged();
                    }
                }
            }
        }

        public IEnumerable<Field> FieldsFromText(string text)
        {
            foreach (var l in RegisterLines(Serializer.ReadBody(text)))
                if ((l as Field) != null)
                    yield return (l as Field);
        }

        IEnumerable<ILine> RegisterLines(IEnumerable<ILine> lines)
        {
            foreach (var i in lines)
            {
                var f = i as Field;
                if (f != null)
                    yield return TryRegisterField(f);
                else
                    yield return i;
            }
        }

        Field TryRegisterField(Field f)
        {
            Type t;
            foreach (var reg in FFieldTypeRegs)
            {
                if (reg.StringToType(f.Typestring, out t))
                {
                    f.FieldType = t;
                    string typeString = f.Typestring;
                    if (t != null)
                        reg.TypeToString(t, out typeString);
                    f.Typestring = typeString;
                    f.ContainerType = reg.ContainerType;
                    f.ContainerRegistry = reg.ContainerRegistry;

                    if (!FFields.ContainsKey(f.ToString()))
                        FFields.Add(f.ToString(), f);
                    return FFields[f.ToString()];
                }
            }

            if (!FFields.ContainsKey(f.ToString()))
                FFields.Add(f.ToString(), f);
            return FFields[f.ToString()];
        }
    }
}