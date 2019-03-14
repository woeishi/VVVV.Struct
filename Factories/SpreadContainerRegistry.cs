using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.PluginInterfaces.V2.NonGeneric;

using VVVV.Struct.Core;

namespace VVVV.Struct.Factories
{
    public class SpreadContainerRegistry : IContainerRegistry
    {
        public virtual string ContainerType => "Spread";

        public bool CreateIO(IIOFactory ioFactory, Dictionary<Field, IOContainer> ioContainers, Field field, bool isInput = true, bool isBinSized = false, int order = 0)
        {
            Type t = typeof(ISpread<>).MakeGenericType(field.FieldType);
            if (isBinSized)
                t = typeof(ISpread<>).MakeGenericType(t);

            if (ioContainers.ContainsKey(field))
            {
                ioContainers[field].SetOrder(order);
                return false;
            }
            else
            {
                if (isInput)
                {
                    var inAttr = new InputAttribute(field.Name) { Order = order, BinOrder = order + 1, AutoValidate = false };
                    inAttr.SetDefaultAttribute(field);
                    var ioCont = ioFactory.CreateIOContainer(t, inAttr);

                    ioContainers.Add(field, new IOContainer(ioCont, isInput, isBinSized, order));
                }
                else
                {
                    var outAttr = new OutputAttribute(field.Name) { Order = order, BinOrder = order + 1 }; //, AutoFlush = false
                    var ioCont = ioFactory.CreateIOContainer(t, outAttr);
                    ioContainers.Add(field,new IOContainer(ioCont, isInput, isBinSized, order));
                    (ioContainers[field].RawIOObject as ISpread).SliceCount = 0;
                }
            }
            return true;
        }

        public virtual object CloneData(object data)
        {
            dynamic spread = data;
            return Clone(spread);
        }

        ISpread<T> Clone<T>(ISpread<T> spread)
        {
            var mem = new Spread<T>();
            mem.AssignFrom(spread);
            return mem;
        }

        public virtual void OnSync(IIOContainer ioContainer) { }

        public int GetLength(IIOContainer ioContainer)
        {
            return (ioContainer.RawIOObject as ISpread).SliceCount;
        }

        public void ReadFromIOBin(Field field, IIOContainer ioContainer, ISpread<Core.Struct> structs)
        {
            dynamic multidim = ioContainer.RawIOObject;
            var spread = GetMultidim(multidim);
            for (int i = 0; i < structs.SliceCount; i++)
            {
                structs[i][field] = spread[i];
            }
        }
        ISpread<ISpread<T>> GetMultidim<T>(ISpread<ISpread<T>> multidim)
        {
            return multidim;
        }

        public virtual void SetLength(Field field, IIOContainer ioContainer, int length)
        {
            (ioContainer.RawIOObject as ISpread).SliceCount = length;
        }

        public void WriteToIO(Field field, IIOContainer ioContainer, ref Core.Struct str)
        {
            dynamic source = str[field];
            dynamic dest = ioContainer.RawIOObject;
            WriteToIO(field, source, dest);
        }

        protected virtual void WriteToIO<T>(Field field, ISpread<T> source, ISpread<T> destination)
        {
            destination.AssignFrom(source);
        }

        public virtual void NilToIOBin(Field field, IIOContainer ioContainer, int index = 0)
        {
            dynamic dest = ioContainer.RawIOObject;
            WriteSpread(null, dest, index);
        }

        public virtual void WriteToIOBin(Field field, IIOContainer ioContainer, ref Core.Struct str, int index = 0)
        {
            dynamic source = str[field];
            dynamic dest = ioContainer.RawIOObject;
            WriteSpread(source, dest, index);
        }

        void WriteSpread<T>(ISpread<T> source, ISpread<ISpread<T>> dest, int index)
        {
            if (dest.SliceCount < (index + 1))
                dest.SliceCount = index + 1;

            if (source == null)
                dest[index] = new Spread<T>(0);
            else
                dest[index].AssignFrom(source);
        }

        public virtual string ToString(object rawIOObject)
        {
            dynamic spread = rawIOObject;
            return ToString(spread);
        }
        string ToString<T>(ISpread<T> source)
        {
            string result = string.Empty;
            foreach (var item in source)
            {
                if (item == null)
                    result += "'null'";
                else
                    result += item.ToString();
                result += ", ";
            }
            return result.Substring(0, result.Length - 2);
        }
    }
}
