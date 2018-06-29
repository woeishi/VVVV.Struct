using System;
using System.Collections.Generic;
using System.ComponentModel.Composition.Hosting;
using System.ComponentModel.Composition.Primitives;

using System.Runtime.InteropServices;

namespace VVVV.Struct.Core
{
    [ComVisible(false)]
    public class IOMgrExportProvider : ExportProvider
    {
        IDeclarationFactory FDeclarationFactory;
        public bool IsInput { get; set; }

        public IOMgrExportProvider(IDeclarationFactory declarationFactory)
        {
            FDeclarationFactory = declarationFactory;

        }

        protected override IEnumerable<Export> GetExportsCore(ImportDefinition definition, AtomicComposition atomicComposition)
        {
            var contractName = definition.ContractName;
            if (contractName == typeof(IOManager).FullName)
            {
                var mgr = new IOManager(FDeclarationFactory, IsInput);
                yield return new Export(contractName, () => mgr);
            }
        }
    }
}
