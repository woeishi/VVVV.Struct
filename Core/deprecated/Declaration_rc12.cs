using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Xml;
using System.Xml.Serialization;


namespace VVVV.Struct.Core
{
    [DataContract, XmlRoot(ElementName = "Declaration")]
    public class Declaration_v2rc12
    {
        [DataMember, XmlAttribute]
        public string Name { get; set; }
        [DataMember, XmlElement(ElementName = "Field")]
        public List<Field_v2rc12> Fields { get; set; }

        private Declaration_v2rc12()
        {
            Name = string.Empty;
            Fields = new List<Field_v2rc12>();
        }
    }

    public static class Declaration_v2rc12Serializer
    {
        public static Declaration Read(string xml)
        {
            var s = new XmlSerializer(typeof(Declaration_v2rc12));
            Declaration_v2rc12 d2;
            using (var ms = new System.IO.MemoryStream(System.Text.UTF8Encoding.UTF8.GetBytes(xml)))
            {
                d2 = (Declaration_v2rc12)s.Deserialize(ms);
            }
            var ls = new List<Field>();
            foreach (var f in d2.Fields)
                ls.Add(new Field(f.Name, f.Typestring, f.Default));

            var d = new Declaration(d2.Name, ls);
            return d;
        }
    }
}
