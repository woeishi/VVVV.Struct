using System;
using System.Collections.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

using VVVV.Struct.Core;

namespace VVVV.Struct.Factories
{
    public class StreamContainerRegistry : IContainerRegistry
    {
        public virtual string ContainerType => "Stream";

        public bool CreateIO(IIOFactory ioFactory, Dictionary<Field, IOContainer> ioContainers, Field field, bool isInput = true, bool isBinSized = false, int order = 0)
        {
            Type t = typeof(IIOStream<IInStream<double>>);
            if (isBinSized)
            {
                t = typeof(IInStream<>).MakeGenericType(field.FieldType);
                if (isInput)
                    t = typeof(IInStream<>).MakeGenericType(t);
                else
                    t = typeof(IIOStream<>).MakeGenericType(t);
            }
            else
                if (isInput)
                t = typeof(IInStream<>).MakeGenericType(field.FieldType);
            else
                t = typeof(IOutStream<>).MakeGenericType(field.FieldType);


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
                    ioContainers.Add(field, new IOContainer(ioCont, isInput, isBinSized, order));
                    (ioContainers[field].RawIOObject as IOutStream).Length = 0;
                }
            }
            return true;
        }

        public object CloneData(object data)
        {
            dynamic stream = data;
            return CloneStream(stream);
        }
        IIOStream<T> CloneStream<T>(IInStream<T> stream)
        {
            var mem = new MemoryIOStream<T>();
            mem.AssignFrom(stream);
            return mem;
        }

        public virtual void OnSync(IIOContainer ioContainer) { }

        public int GetLength(IIOContainer ioContainer)
        {
            return (ioContainer.RawIOObject as IIOStream).Length;
        }

        public void ReadFromIOBin(Field field, IIOContainer ioContainer, ISpread<Core.Struct> structs)
        {
            dynamic multidim = ioContainer.RawIOObject;
            using (var r = GetCyclicReader(multidim))
            {
                for (int i = 0; i < structs.SliceCount; i++)
                {
                    if (structs[i] != null)
                        structs[i][field] = r.Read();
                }
            }
        }
        CyclicStreamReader<IInStream<T>> GetCyclicReader<T>(IInStream<IInStream<T>> multidim)
        {
            return multidim.GetCyclicReader();
        }

        public void SetLength(Field field, IIOContainer ioContainer, int length)
        {
            dynamic stream = ioContainer.RawIOObject;
            stream.Length = length;
        }

        public void WriteToIO(Field field, IIOContainer ioContainer, ref Core.Struct str)
        {
            dynamic source = str[field];
            dynamic dest = ioContainer.RawIOObject;
            AssignStream(source, dest);
        }
        void AssignStream<T>(IInStream<T> source, IOutStream<T> dest)
        {
            dest.AssignFrom(source);
        }

        public void WriteToIOBin(Field field, IIOContainer ioContainer, ref Core.Struct str, int index = 0)
        {
            dynamic source = str[field];
            dynamic dest = ioContainer.RawIOObject;
            WriteStream(source, dest, index);
        }
        void WriteStream<T>(IInStream<T> source, IIOStream<IInStream<T>> dest, int index)
        {
            if (dest.Length < (index + 1))
                dest.Length = index + 1;
            using (var w = dest.GetWriter())
            {
                w.Position = index;
                if (source == null)
                    w.Write(StreamUtils.GetEmptyStream<T>());
                else
                    w.Write(source);
            }
        }

        public virtual string ToString(object rawIOObject)
        {
            dynamic stream = rawIOObject;
            return ToString(stream);
        }
        string ToString<T>(IInStream<T> source)
        {
            string result = string.Empty;
            using (var r = source.GetReader())
            {
                while (!r.Eos)
                {
                    var item = r.Read();
                    if (item == null)
                        result += "'null'";
                    else
                        result += item.ToString();
                    result += ", ";
                }
            }
            return result.Substring(0, result.Length - 2);
        }
    }
}
