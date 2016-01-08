using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace VVVV.Struct
{
    /// <summary>
    /// simple definition serializer singleton
    /// </summary>
    public static class DefinitionSerializer
    {
        static Serializer<Definition> serializer = new Serializer<Definition>();

        /// <summary>
        /// Deserialize Definition from utf8 xml string
        /// </summary>
        /// <param name="xml">xml string representation of the definition</param>
        /// <returns>Definition</returns>
        public static Definition Read(string xml)
        {
            return (Definition)serializer.Read(xml);
        }

        /// <summary>
        /// Serializes a Definition to utf8 xml string, without namespace prefix and xml declaration
        /// </summary>
        /// <param name="definition">definition to serialize</param>
        /// <returns>utf8 xml string</returns>
        public static string Write(Definition definition)
        {
            return serializer.Write(definition);
        }
    }

    /// <summary>
    /// Generic serializer stripping namespace prefixes and omitting xml declaration strings
    /// </summary>
    /// <typeparam name="T">type to serialize</typeparam>
    public class Serializer<T>
    {
        XmlSerializerNamespaces ns;
        XmlSerializer serializer;
        XmlWriterSettings settings;
        Encoding encoding;

        public Serializer()
        {
            ns = new XmlSerializerNamespaces();
            ns.Add("", "");
            settings = new XmlWriterSettings();
            settings.OmitXmlDeclaration = true;
            encoding = UTF8Encoding.UTF8;
            serializer = new XmlSerializer(typeof(T));
        }

        /// <summary>
        /// Serialize into a stream, strips namespace prefixes
        /// </summary>
        /// <param name="stream">stream to be written to</param>
        /// <param name="obj">object to serialize</param>
        public void Write(Stream stream, T obj)
        {
            serializer.Serialize(stream, obj, ns);
        }

        internal string Write(T obj)
        {
            string result;
            using (var ms = new MemoryStream())
            {
                using (var writer = XmlTextWriter.Create(ms, settings))
                {
                    serializer.Serialize(writer, obj, ns);
                }
                result = encoding.GetString(ms.ToArray());
            }
            return result;
        }

        /// <summary>
        /// Deserialize from a stream
        /// </summary>
        /// <param name="stream">stream to read from</param>
        /// <returns>deserialized object</returns>
        public T Read(Stream stream)
        {
            return (T)serializer.Deserialize(stream);
        }

        internal T Read(string xml)
        {
            T result;
            using (var ms = new MemoryStream(encoding.GetBytes(xml)))
            {
                result = Read(ms);
            }
            return result;
        }
    }
}
