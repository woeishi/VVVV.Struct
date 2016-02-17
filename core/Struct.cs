using System;
using System.Collections.Generic;

using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace VVVV.Struct
{
    [DataContract]
    public class Property : IEquatable<Property>
    {
        [DataMember, XmlAttribute]
        public string Name;

        string typestring;
        [DataMember, XmlAttribute]
        public string DatatypeString
        {
            get { return typestring; }
            set { typestring = value; Datatype = StructTypeMapper.Map(value); }
        }

        [XmlIgnore]
        public Type Datatype;

        [DataMember, XmlAttribute]
        public string Default;

        public Property()
        {
            Name = string.Empty;
            typestring = "double";
            Datatype = typeof(double);
            Default = string.Empty;
        }

        public Property(string name, Type type, string defaultValue)
        {
            Name = name;
            Datatype = type;
            foreach (var kv in StructTypeMapper.Mappings)
            {
                if (type == kv.Value)
                {
                    typestring = kv.Key;
                    break;
                }
            }
            Default = defaultValue;
        }

        public override bool Equals(object obj)
        {
            if (obj is Property)
                return Equals((Property)obj);
            else
                return false;
        }

        public bool Equals(Property other)
        {
            return
                this.Name == other.Name &&
                this.Datatype == other.Datatype &&
                this.Default == other.Default;
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode() ^ this.Datatype.GetHashCode() ^ this.Default.GetHashCode();
        }
    }

    [DataContract]
    public class Definition : EventArgs
	{
        [DataMember, XmlAttribute]
        public string Key;
        [DataMember, XmlElement]
        public List<Property> Property;
        [DataMember, XmlAttribute]
        public string HandlerPath;

        private Definition()
        {
            Property = new List<Property>();
            Key = string.Empty;
            HandlerPath = string.Empty;
        }
		internal Definition(string key):this()
		{
            Key = key;
		}
		
        internal bool TryAddProperty(Property property)
        {
            bool nameExists = false;
            foreach (var p in Property)
                if (p.Name == property.Name)
                    nameExists = true;
            if (!nameExists)
                Property.Add(property);
            return !nameExists;
        }
	}
	
	/// <summary>
	/// class to hold the struct data
	/// </summary>
	public class Struct
	{
		private string key;
		public string Key { get { return key; } }

		private Dictionary<Property,object> data;
		public Dictionary<Property,object> Data { get { return data; } }
		
		public Struct(string name)
		{
			key = name;
			data = new Dictionary<Property,object>();
		}
	}
}
