using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using System.Linq;
using VVVV.PluginInterfaces.V2;

using VVVV.Struct.Core;

namespace VVVV.Struct.Hosting
{
    [Export(typeof(IIOManager))]
    [Export(typeof(IOManager))]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class IOManager : IIOManager
    {
        readonly object dictLock = new object();
        Dictionary<Field, IOContainer> FIOContainers;
        IDeclarationFactory FDeclarationFactory;

        //[Import(AllowDefault = true, AllowRecomposition = true, Source = ImportSource.Any)]
        public IIOFactory IOFactory;

        public bool IsInput;
        public bool IsBinSized { get; set; }

        public event EventHandler IOsChanged;
        public event EventHandler<string> DeclarationChanged;

        [ImportingConstructor]
        public IOManager(IDeclarationFactory declarationFactory)
        {
            FIOContainers = new Dictionary<Field, IOContainer>();
            FDeclarationFactory = declarationFactory;
            FDeclarationFactory.DeclarationChanged += OnFactoryDeclarationChanged;
        }

        void OnFactoryDeclarationChanged(object sender, string e)
        {
            DeclarationChanged?.Invoke(sender, e);
        }

        public void Dispose()
        {
            FDeclarationFactory.DeclarationChanged -= OnFactoryDeclarationChanged;
            foreach (var f in FIOContainers.Keys)
            {
                f.Changed -= FieldChanged;
                FIOContainers[f].Dispose();
            }
            FIOContainers.Clear();
        }

        public void ResetCache(bool reset = true)
        {
            foreach (var io in FIOContainers.Values)
                io.ResetCache(reset);
        }

        public void UpdateFromDeclaration(Declaration declaration)
        {
            UpdateIOs(declaration.Fields);
            IOsChanged?.Invoke(declaration, EventArgs.Empty);
        }

        void FieldChanged(object sender, EventArgs e)
        {
            var f = sender as Field;
            try
            {
                lock (dictLock)
                {
                    var order = FIOContainers[f].Order;
                    FIOContainers[f].Dispose();
                    FIOContainers.Remove(f);
                    var b = CreateIO(f, IsInput, IsBinSized, order);

                    IOsChanged?.Invoke(null, EventArgs.Empty);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        bool CreateIO(Field field, bool isInput = true, bool isBinSized = false, int order = 0)
        {
            var created = field.ContainerRegistry.CreateIO(IOFactory, FIOContainers, field, isInput, isBinSized, order);
            return created;
        }

        bool RemoveIO(Field field)
        {
            if (!FIOContainers.ContainsKey(field))
                return false;
            else
            {
                FIOContainers[field].Dispose();
                FIOContainers.Remove(field);
                field.Changed -= FieldChanged;
                return true;
            }
        }

        void UpdateIOs(IEnumerable<Field> fields)
        {
            List<Field> oldFields = FIOContainers.Keys.ToList();
            int i = 0;
            foreach (var f in fields)
            {
                if (!CreateIO(f, IsInput, IsBinSized, i))
                    oldFields.Remove(f);
                else
                    f.Changed += FieldChanged;

                i+=2; //step by two due to binsize pins
            }

            foreach (var f in oldFields)
                RemoveIO(f);
        }



        //used by nodes
        public void SetIOs(string fieldString)
        {
            UpdateIOs(FDeclarationFactory.FieldsFromText(fieldString));
        }

        public Core.Struct CreateStruct(Declaration declaration)
        {
            var s = new Core.Struct(declaration);
            foreach (var f in s.Sync.Keys)
                s.Sync[f].Requested += (o, e) => { SyncIO(f); };
            return s;
        }

        public Core.Struct CreateStruct(Core.Struct other)
        {
            var s = new Core.Struct(other);
            foreach (var f in s.Sync.Keys)
            {
                if (FIOContainers.ContainsKey(f))
                    s.Sync[f].Requested += (o, e) => { SyncIO(f); }; //sync local pin
                else
                    s.Sync[f].Requested += (o, e) => { other.Sync[f].Request(); }; //bubble up sync request
            }
            return s;
        }

        public int CalculateSpreadMax()
        {
            int spreadMax = 0;
            foreach (var kv in FIOContainers)
            {
                SyncIO(kv.Key);
                var l = kv.Key.ContainerRegistry.GetLength(kv.Value);
                if (l == 0)
                    return 0;
                else if (l > spreadMax)
                    spreadMax = l;
            }
            return spreadMax;
        }

        public void SyncIO(Field f)
        {
            f.ContainerRegistry.OnSync(FIOContainers[f]); //necessary only for overloaded actions
            FIOContainers[f].Sync();
        }

        public void ReadFromIOs(ref Core.Struct str)
        {
            foreach (var f in FIOContainers.Keys)
                str[f] = FIOContainers[f].RawIOObject;
        }

        public void ReadFromIOBins(ISpread<Core.Struct> structs)
        {
            foreach (var f in FIOContainers.Keys)
            {
                SyncIO(f); //cannot rely on downstream sync request, correct bins are needed beforehand
                f.ContainerRegistry.ReadFromIOBin(f, FIOContainers[f], structs);
            }
        }

        public void SetLengthIO(Field field, int length)
        {
            field.ContainerRegistry.SetLength(field, FIOContainers[field], length);
        }

        public void SetLengthAllIOs(int length)
        {
            foreach (var f in FIOContainers.Keys)
                SetLengthIO(f, length);
        }

        public void WriteToIOs(ref Core.Struct str)
        {
            foreach (var f in FIOContainers.Keys)
                if (FIOContainers[f].IsVisible)
                    f.ContainerRegistry.WriteToIO(f, FIOContainers[f], ref str);
        }

        public void WriteToIOBins(ref Core.Struct str, int index = 0)
        {
            foreach (var f in FIOContainers.Keys)
                if (FIOContainers[f].IsVisible)
                    f.ContainerRegistry.WriteToIOBin(f, FIOContainers[f], ref str, index);
        }

        public void WriteNilToIOBins(int index = 0)
        {
            foreach (var f in FIOContainers.Keys)
                if (FIOContainers[f].IsVisible)
                    f.ContainerRegistry.NilToIOBin(f, FIOContainers[f], index);
        }
    }
}