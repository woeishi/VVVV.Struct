using System;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace VVVV.Struct.Core
{
    [DataContract, XmlRoot(ElementName = "Declaration")]
    public class Field_v2rc12 : IEquatable<Field_v2rc12>
    {
        [DataMember, XmlAttribute]
        public string Name { get; set; }

        Type FFieldtype;
        [XmlIgnore]
        public Type FieldType
        {
            get { return FFieldtype; }
            set { FFieldtype = value; }
        }
        string FTypestring;
        [DataMember, XmlAttribute(AttributeName = "Type")]
        public string Typestring
        {
            get { return FTypestring; }
            set { FTypestring = value; } //access fieldtypemapper here
        }
        [DataMember(IsRequired = false, EmitDefaultValue = false), XmlAttribute(AttributeName = "fType")]
        public string TypestringDeprecated
        {
            get { return default(string); }
            set { FTypestring = value; } //access fieldtypemapper here
        }

        [XmlIgnore]
        public string ContainerType { get; set; }
        [XmlIgnore]
        public IContainerRegistry ContainerRegistry { get; set; }

        [DataMember(IsRequired = false, EmitDefaultValue = false), XmlAttribute]
        public string Default { get; set; }

        public Field_v2rc12()
        {
            Name = string.Empty;
            FFieldtype = typeof(double);
            FTypestring = "double";
            ContainerType = "Stream";
            Default = default(string);
        }

        public override string ToString()
        {
            string body = Typestring + " " + Name;
            if (!string.IsNullOrWhiteSpace(Default))
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
            body += ";";
            return body;
        }

        //IEquatable
        public bool Equals(Field_v2rc12 other)
        {
            return this.GetHashCode() == other.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            Field_v2rc12 other = (Field_v2rc12)obj;
            if (other == null)
                return false;
            return Equals(other);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hash = 17;
                hash = hash * 23 + Name.GetHashCode();
                hash = hash * 23 + ((FFieldtype != null) ? FFieldtype.GetHashCode() : FTypestring.GetHashCode());
                hash = hash * 23 + ContainerType.GetHashCode();
                return hash;
            }
        }
    }
}
