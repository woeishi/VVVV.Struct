using System;
using System.Collections.Generic;

namespace VVVV.Struct.Core
{
    public interface ILine { }

    public class Declaration : IEquatable<Declaration>
    {
        public string Name { get; private set; }

        public IReadOnlyCollection<Field> Fields { get; }
        public List<ILine> Lines { get; }

        public Declaration(string name, IEnumerable<ILine> lines)
        {
            Name = name;
            Lines = new List<ILine>(lines);
            List<Field> fs = new List<Field>();
            foreach (var l in lines)
                if (l as Field != null)
                    fs.Add(l as Field);
            Fields = new List<Field>(fs);
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

        public override string ToString()
        {
            var ret = Name + "{";
            foreach (var l in Lines)
                ret += l;
            return ret+"}";
        }
    }
}
