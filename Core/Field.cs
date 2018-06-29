using System;

namespace VVVV.Struct.Core
{
    public class Comment : ILine
    {
        public string Text { get; }
        public Comment(string text) { Text = text; }

        public override string ToString() =>  Text;
    }

    public class Field : IEquatable<Field>, ILine
    {
        public string Name { get; set; }
        public string Typestring { get; set; }
        public string Default { get; set; }
       
        public Type FieldType { get; set; }
        public string ContainerType { get; set; }
        public IContainerRegistry ContainerRegistry { get; set; }

        public event EventHandler Changed;
        public void InvokeChanged() => Changed?.Invoke(this, EventArgs.Empty);

        int FHashCode = 0;

        public Field(string name, string typestring, string defaultString = "")
        {
            Name = name;
            Typestring = typestring;
            Default = defaultString;

            ContainerType = "Null";
        }

        public Field(Field other)
        {
            Name = other.Name;
            FieldType = other.FieldType;
            Typestring = other.Typestring;
            ContainerType = other.ContainerType;
            ContainerRegistry = other.ContainerRegistry;
            Default = other.Default;
        }

        public override string ToString()
        {
            string body = Typestring + " " + Name;
            if (!string.IsNullOrWhiteSpace(Default))
            {
                if (FieldType == typeof(string))
                    body += " = " + Default;
                else
                {
                    try //TODO legacy rc12
                    {
                        if (Default != Activator.CreateInstance(FieldType).ToString())
                            body += " = " + Default;
                    }
                    catch
                    {
                        body += " = " + Default;
                    }
                }
            }
            body += ";";
            return body;
        }

        int CreateHashCode()
        {
            unchecked
            {
                int hash = 17;

                var str = Name;
                if (FieldType != null)
                {
                    str += FieldType.ToString();
                    
                    if ((!FieldType.Assembly.IsDynamic) && FieldType.Assembly.Location != null)
                        str += FieldType.Assembly.Location;
                    else
                        str += FieldType.Assembly.FullName;
                }
                else
                    str += Typestring;
                str += ContainerType;
                return hash * 23 + str.GetHashCode();
            }
        }

        //IEquatable
        public bool Equals(Field other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Field other = (Field)obj;
            if (other == null)
                return false;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            if (FHashCode == 0 || FieldType == null)
                FHashCode = CreateHashCode();

            return FHashCode;
        }
    }
}