using System;
using System.Collections.Generic;

using VVVV.DX11;
using FeralTic.DX11;
using FeralTic.DX11.Resources;

namespace VVVV.Struct.Factories
{
    public class DX11GraphPartFieldtypeRegistry : BaseFieldTypeRegistry
    {
        public override string ContainerType => "Spread.DX11Resource";

        public DX11GraphPartFieldtypeRegistry():base()
        {
            FMappings.Add("dx11layer", typeof(DX11Resource<DX11Layer>));

            FMappings.Add("dx11geometry", typeof(DX11Resource<IDX11Geometry>));

            FMappings.Add("dx11texture1d", typeof(DX11Resource<DX11Texture1D>));
            FMappings.Add("dx11texture2d", typeof(DX11Resource<DX11Texture2D>));
            FMappings.Add("dx11texture3d", typeof(DX11Resource<DX11Texture3D>));

            FMappings.Add("dx11buffer", typeof(DX11Resource<IDX11ReadableResource>));
        }
    }

    public class DX11FieldtypeRegistry : BaseFieldTypeRegistry
    {
        public override string ContainerType => "Stream";

        public DX11FieldtypeRegistry() : base()
        {
            FMappings.Add("dx11renderstate", typeof(DX11RenderState));
            FMappings.Add("dx11samplerstates", typeof(DX11SamplerStates));

            FMappings.Add("dx11rendersemantic", typeof(IDX11RenderSemantic));

            FMappings.Add("dx11objectvalidator", typeof(IDX11ObjectValidator));
        }
    }
}
