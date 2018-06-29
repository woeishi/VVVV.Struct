using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace VVVV.Struct.Core
{
    public sealed class Serializer
    {
        static readonly Regex FDeclRegex;
        static readonly Regex FSplitRegex;
        static readonly Regex FParseRegex;

        private Serializer() { }

        static Serializer()
        {
            FDeclRegex = new Regex("(^\\s*(?<name>\\w+)\\s*{\\s*(?<body>.*)\\s*}\\s*$)");
            FSplitRegex = new Regex(@"(?(?<=;)\n*|\n)+");
            FParseRegex = new Regex("(^\\s*(?<type>[\\w|\\.]+?\\s+)?(?<name>\\w+)(\\s*=\\s*(?!;)(?<default>[^\\s;]*))?;*?$)");
        }

        public static Declaration ReadDeclaration(string text)
        {
            if (text.StartsWith("{"))
                return DeclarationSerializer_v2rc15.Read(text);

            else
            {
                var m = FDeclRegex.Match(text);
                return new Declaration(m.Groups["name"].Value, ReadBody(m.Groups["body"].Value));
            }
        }

        public static IEnumerable<ILine> ReadBody(string body)
        {
            foreach (var l in FSplitRegex.Split(body))
            {
                var f = ReadLine(l);
                if (f != null)
                    yield return f;
            }
        }

        public static ILine ReadLine(string line)
        {
            var l = line;
            var m = FParseRegex.Match(l);
            if (!(m.Groups["type"].Success && m.Groups["name"].Success))
            {
                if (string.IsNullOrWhiteSpace(l))
                    return null;
                else
                {
                    if (!(l.StartsWith("//") || l.StartsWith("#")))
                        l = "#" + l;
                    return new Comment(l);
                }
            }
            return new Field(m.Groups["name"].Value, m.Groups["type"].Value.Trim(), m.Groups["default"].Value.Trim());
        }
    }
}
