using System;
using System.Collections.Generic;
using System.Runtime.Serialization.Json;
using System.Xml;

namespace VVVV.Struct.Core
{
    public static class DeclarationSerializer_v2rc15
    {
        public static Declaration Read(string json)
        {
            if (json.StartsWith("<")) //checking for legacy here
                return Declaration_v2rc12Serializer.Read(json);

            var buffer = System.Text.Encoding.UTF8.GetBytes(json);
            using (var reader = JsonReaderWriterFactory.CreateJsonReader(buffer, new XmlDictionaryReaderQuotas()))
            {
                reader.MoveToStartElement();
                reader.Read(); //read root element

                if (reader.Name == "Name")
                {
                    var declarationName = reader.ReadString();
                    var d = (Declaration)Activator.CreateInstance(typeof(Declaration), declarationName, ReadLines(reader));
                    return d;
                }
            }
            return null;
        }

        static IEnumerable<ILine> ReadLines(XmlDictionaryReader reader)
        {
            while (!reader.EOF)
            {
                if (reader.NodeType != XmlNodeType.EndElement)
                {
                    switch (reader.Name)
                    {
                        case "Field":
                            yield return ReadField(reader);
                            break;
                        case "Comment":
                            yield return ReadComment(reader);
                            break;
                        default:
                            throw new Exception();
                    }
                }
                else
                    reader.Read();
            }
        }

        static Field ReadField(XmlDictionaryReader reader)
        {
            reader.Read(); //step into array
            var name = reader.ReadString();
            reader.ReadEndElement();
            var typeString = reader.ReadString();
            reader.ReadEndElement();
            var defaultString = string.Empty;
            if (reader.NodeType != XmlNodeType.EndElement) //closing array
                defaultString = reader.ReadString();
            return new Field(name, typeString, defaultString);
        }

        static Comment ReadComment(XmlDictionaryReader reader)
        {
            return new Comment(reader.ReadString());
        }

        public static string Write(Declaration declaration)
        {
            var inner = string.Empty;
            foreach (var l in declaration.Lines)
            {
                if ((l as Field) != null)
                    inner += WriteField((Field)l);
                else if ((l as Comment) != null)
                    inner += WriteComment((Comment)l);
                inner += ",";
            }
            inner = inner.TrimEnd(',');
            return $@"{{""Name"":""{declaration.Name}"",{inner}}}";
        }

        static string WriteField(Field f)
        {
            var inner = $@"""Name"":""{f.Name}"",""Type"":""{f.Typestring}""";
            if (!string.IsNullOrWhiteSpace(f.Default))
                inner += $@",""Default"":""{f.Default}""";
            return $@"""Field"":{{{inner}}}";
        }

        static string WriteComment(Comment c)
        {
            return $@"""Comment"":""{c.Text}""";
        }
    }
}
