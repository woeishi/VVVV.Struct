using System;
using VVVV.Nodes.Generic;
using VVVV.PluginInterfaces.V2;
using VVVV.Utils.Streams;

namespace VVVV.Struct
{
	#region SingleValue
	
	[PluginInfo(Name = "Cast",
                Category = "Struct",
                Help = "Casts any type to a type of this category, so be sure the input is of the required type",
                Tags = "convert, as, generic"
                )]
    public class StructCastNode : Cast<Struct> {}
    
    #endregion SingleValue
    
    #region SpreadOps
	
	[PluginInfo(Name = "Cons",
                Category = "Struct",
                Help = "Concatenates all input spreads to one output spread.",
                Tags = "generic, spreadop"
                )]
    public class StructConsNode : Cons<Struct> {}
	
	[PluginInfo(Name = "CAR", 
	            Category = "Struct",
	            Version = "Bin", 
	            Help = "Splits a given spread into first slice and remainder.", 
	            Tags = "split, generic, spreadop",
	            Author = "woei"
	           )]
	public class StructCARBinNode : CARBin<Struct> {}
	
	[PluginInfo(Name = "CDR", 
	            Category = "Struct", 
	            Version = "Bin", 
	            Help = "Splits a given spread into remainder and last slice.", 
	            Tags = "split, generic, spreadop",
	            Author = "woei"
	           )]
	public class StructCDRBinNode : CDRBin<Struct> {}
	
	[PluginInfo(Name = "Reverse", 
	            Category = "Struct", 
	            Version = "Bin",
	            Help = "Reverses the order of slices in a given spread.",
	            Tags = "invert, generic, spreadop",
	            Author = "woei"
	           )]
	public class StructReverseBinNode : ReverseBin<Struct> {}

	[PluginInfo(Name = "Shift", 
	            Category = "Struct", 
	            Version = "Bin", 
	            Help = "Shifts the slices in a spread upwards by the given phase.", 
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class StructShiftBinNode : ShiftBin<Struct> {}
	
	[PluginInfo(Name = "SetSlice",
	            Category = "Struct",
	            Version = "Bin",
	            Help = "Replaces individual slices of a spread with the given input",
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class StructSetSliceNode : SetSlice<Struct> {}
    
	[PluginInfo(Name = "DeleteSlice",
	            Category = "Struct",
	            Help = "Deletes the slice at the given index.",
	            Tags = "remove, generic, spreadop",
	            Author = "woei"
	           )]
	public class StructDeleteSliceNode : DeleteSlice<Struct> {}
	
	[PluginInfo(Name = "Select",
                Category = "Struct",
                Help = "Select which slices and how many form the output spread.",
	            Tags = "resample, generic, spreadop"
	           )]
    public class StructSelectNode : Select<Struct> {}
    
    [PluginInfo(Name = "Select", 
				Category = "Struct",
				Version = "Bin",				
				Help = "Select the slices which form the new spread.", 
				Tags = "repeat, generic, spreadop",
				Author = "woei"
			)]
    public class StructSelectBinNode : SelectBin<Struct> {}
    
	[PluginInfo(Name = "Unzip", 
	            Category = "Struct",
	            Help = "Unzips a spread into multiple spreads.", 
	            Tags = "split, generic, spreadop"
	           )]
	public class StructUnzipNode : Unzip<Struct> {}
	
	[PluginInfo(Name = "Unzip", 
	            Category = "Struct",
	            Version = "Bin",
	            Help = "Unzips a spread into multiple spreads.", 
	            Tags = "split, generic, spreadop"
	           )]
	public class StructUnzipBinNode : Unzip<IInStream<Struct>> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "Struct",
	            Help = "Zips spreads together.", 
	            Tags = "join, generic, spreadop"
	           )]
	public class StructZipNode : Zip<Struct> {}
	
	[PluginInfo(Name = "Zip", 
	            Category = "Struct",
				Version = "Bin",	            
	            Help = "Zips spreads together.", 
	            Tags = "join, generic, spreadop"
	           )]
	public class StructZipBinNode : Zip<IInStream<Struct>> {}
	
    [PluginInfo(Name = "GetSpread",
                Category = "Struct",
                Version = "Bin",
                Help = "Returns sub-spreads from the input specified via offset and count",
                Tags = "generic, spreadop",
                Author = "woei")]
    public class StructGetSpreadNode : GetSpreadAdvanced<Struct> {}
    
	[PluginInfo(Name = "SetSpread",
	            Category = "Struct",
	            Version = "Bin",
	            Help = "Allows to set sub-spreads into a given spread.",
	            Tags = "generic, spreadop",
	            Author = "woei"
	           )]
	public class StructSetSpreadNode : SetSpread<Struct> {}
    
    [PluginInfo(Name = "Pairwise",
                Category = "Struct",
                Help = "Returns all pairs of successive slices. From an input ABCD returns AB, BC, CD.",
                Tags = "generic, spreadop"
                )]
    public class StructPairwiseNode : Pairwise<Struct> {}

    [PluginInfo(Name = "SplitAt",
                Category = "Struct",
                Help = "Splits a spread at the given index.",
                Tags = "generic, spreadop"
                )]
    public class StructSplitAtNode : SplitAtNode<Struct> { }
    
   	#endregion SpreadOps
}

