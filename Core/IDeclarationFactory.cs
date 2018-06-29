using System;
using System.Collections.Generic;

namespace VVVV.Struct.Core
{
    public interface IDeclarationFactory
    {
        IEnumerable<Field> FieldsFromText(string text);
        event EventHandler<string> DeclarationChanged;
    }
}
