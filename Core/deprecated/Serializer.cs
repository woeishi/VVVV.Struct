using System;
using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace VVVV.Struct.Core
{
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
            try
            {
                serializer = new XmlSerializer(typeof(T));
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine(e);
            }
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

        public string Write(T obj)
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

        public T Read(string xml)
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
