using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;

using System.Linq;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Struct.Core
{
    [Export(typeof(IOManager))]
    public class IOManager : IIOManager
    {
        Dictionary<Field, IIOContainer> FIOContainers;
        Dictionary<Field, IPinListener> FPinListeners;
        IDeclarationFactory FDeclarationFactory;

        [Import(typeof(IIOFactory), AllowDefault = true, AllowRecomposition = true, Source = ImportSource.Any)]
        IIOFactory IOFactory;

        //[Import("IsInput")]
        bool IsInput;
        public bool IsBinSized { get; set; }

        //public IReadOnlyCollection<Field> Fields => FIOContainers.Keys;

        public event EventHandler IOsChanged;

        public event EventHandler<string> DeclarationChanged;

        public IOManager()
        {
            FIOContainers = new Dictionary<Field, IIOContainer>();
            FPinListeners = new Dictionary<Field, IPinListener>();
        }

        [ImportingConstructor]
        public IOManager(IDeclarationFactory declarationFactory) : base()
        {
            FIOContainers = new Dictionary<Field, IIOContainer>();
            FPinListeners = new Dictionary<Field, IPinListener>();
            FDeclarationFactory = declarationFactory;
            FDeclarationFactory.DeclarationChanged += DeclarationFactory_DeclarationChanged;

            
        }

        void DeclarationFactory_DeclarationChanged(object sender, string e)
        {
            DeclarationChanged?.Invoke(sender, e);
        }

        public void Dispose()
        {
            FDeclarationFactory.DeclarationChanged -= DeclarationFactory_DeclarationChanged;
            foreach (var f in FIOContainers.Keys)
            {
                f.OnChanged -= FieldChanged;
                FIOContainers[f].Dispose();
            }
               
            FIOContainers.Clear();
        }

        public void SetIOFactory(IIOFactory ioFactory)
        {
            IOFactory = ioFactory;
        }


        public void UpdateFromDeclaration(Declaration declaration)
        {
            UpdateIOs(declaration.Fields);
            IOsChanged?.Invoke(declaration, EventArgs.Empty);
        }

        public bool CreateIO(Field field, int order = 0, bool isInput = true, bool isBinSized = false)
        {
            var created = field.ContainerRegistry.CreateIO(IOFactory, FIOContainers, field, order, isInput, isBinSized);
            return created;
        }

        public bool RemoveIO(Field field)
        {
            if (!FIOContainers.ContainsKey(field))
                return false;
            else
            {
                FIOContainers[field].Dispose();
                FIOContainers.Remove(field);
                field.OnChanged -= FieldChanged;
                return true;
            }
        }

        void UpdateIOs(IEnumerable<Field> fields)
        {
            
            List<Field> oldFields = FIOContainers.Keys.ToList();
            int i = 0;
            
            foreach (var f in fields)
            {
                if (!CreateIO(f, i, IsInput, IsBinSized))
                    oldFields.Remove(f);
                else
                    f.OnChanged += FieldChanged;
                i++;
            }

            foreach (var f in oldFields)
                RemoveIO(f);
            
        }

        void FieldChanged(object sender, EventArgs e)
        {
            var f = sender as Field;
            try
            {
                var order = FIOContainers[f].GetPluginIO().Order;
                FIOContainers[f].Dispose();
                FIOContainers.Remove(f);
                var b = CreateIO(f, order, IsInput, IsBinSized);

                IOsChanged?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex);
            }
        }

        //used by nodes
        public void SetIOs(string fieldString)
        {
            UpdateIOs(FDeclarationFactory.FieldsFromText(fieldString));
        }

        public Struct CreateStruct(Declaration declaration)
        {
            return new Struct(declaration);
        }

        public int CalculateSpreadMax()
        {
            int spreadMax = 0;
            foreach (var kv in FIOContainers)
            {
                var l = kv.Key.ContainerRegistry.GetLength(kv.Value);
                if (l == 0)
                    return 0;
                else
                    spreadMax = Math.Max(spreadMax, l);
            }
            return spreadMax;
        }

        public void SyncIO(Field f)
        {
            f.ContainerRegistry.OnSync(IOFactory.PluginHost);
            (FIOContainers[f].RawIOObject as ISynchronizable)?.Sync();
        }

        public void SyncAllIOs()
        {
            foreach (var f in FIOContainers.Keys)
                SyncIO(f);
        }

        public void ReadFromIOs(ref Struct str)
        {
            foreach (var f in FIOContainers.Keys)
                str[f] = FIOContainers[f].RawIOObject;
        }

        public void ReadFromIOBins(ISpread<Struct> structs)
        {
            foreach (var f in FIOContainers.Keys)
                f.ContainerRegistry.ReadFromIOBin(f, FIOContainers[f], structs);
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

        public void WriteToIOs(ref Struct str)
        {
            foreach (var f in FIOContainers.Keys)
                f.ContainerRegistry.WriteToIO(f, FIOContainers[f], ref str);
        }

        public void WriteToIOBins(ref Struct str, int index = 0)
        {
            foreach (var f in FIOContainers.Keys)
                f.ContainerRegistry.WriteToIOBin(f, FIOContainers[f], ref str, index);
        }

        //public void FlushIO(Field f)
        //{
        //    (FIOContainers[f].RawIOObject as IFlushable).Flush();
        //}

        //public void FlushAllIOs()
        //{
        //    foreach (var f in FIOContainers.Keys)
        //        FlushIO(f);
        //}
    }
}
