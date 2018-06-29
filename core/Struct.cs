using System;
using System.Collections.Generic;

namespace VVVV.Struct.Core
{
    public class Sync
    {
        public event EventHandler Requested;
        public void Request() => Requested?.Invoke(this, EventArgs.Empty);
    }

    public class Struct
    {
        readonly string FName;
        public string Name => FName;
        readonly Dictionary<Field, object> FData;
        public IReadOnlyCollection<Field> Fields => FData.Keys;

        public Dictionary<Field, Sync> Sync { get; }

        public Struct(Declaration declaration)
        {
            FData = new Dictionary<Field, object>();
            Sync = new Dictionary<Field, Sync>();
            FName = declaration.Name;
            foreach (var f in declaration.Fields)
            {
                FData.Add(f, null);
                Sync.Add(f, new Sync());
            }
        }

        public Struct(Struct other)
        {
            FName = other.Name;
            FData = new Dictionary<Field, object>(other.FData);
            Sync = new Dictionary<Field, Sync>();
            foreach (var f in FData.Keys)
                Sync.Add(f, new Sync());
        }

        public object this[Field field]
        {
            get
            {
                if (!FData.ContainsKey(field))
                    return null;

                Sync[field].Request();
                return FData[field];
            }
            set
            {
                if (FData.ContainsKey(field))
                    FData[field] = value;
            }
        }

        public object GetClonedData(Field field)
        {
            //Sync[field].Request();
            return field.ContainerRegistry.CloneData(this[field]);
        }
    }
}