using System;
using System.Collections.Generic;

namespace VVVV.Struct.Core
{
    public interface ILine { }

    public class Declaration : IStruct, IEquatable<Declaration>
    {
        public string Name { get; private set; }

        public IReadOnlyCollection<Field> Fields => fields;
        List<Field> fields;
        public List<ILine> Lines { get; }

        public Declaration(string name, IEnumerable<ILine> lines)
        {
            Name = name;
            Lines = new List<ILine>(lines);
            fields = new List<Field>();
            foreach (var l in lines)
                if (l as Field != null)
                    fields.Add(l as Field);
        }

        public bool Equals(Declaration other)
        {
            if (other == null)
                return false;
            if (Name != other.Name)
                return false;
            if (Lines.Count != other.Lines.Count)
                return false;

            for (int i = 0; i < Lines.Count; i++)
            {
                if (Lines[i].ToString() != other.Lines[i].ToString())
                    return false;
            }
            return true;
        }

        public bool Equals(IStruct other)
        {
            if (this.Name != other.Name)
                return false;
            if (this.Fields.Count != other.Fields.Count)
                return false;
            foreach (var f in other.Fields)
                if (fields.IndexOf(f) < 0)
                    return false;
                
            return true;
        }

        public override string ToString()
        {
            var ret = Name + "{";
            foreach (var l in Lines)
                ret += l;
            return ret+"}";
        }

        
    }
}
