VVVV.Struct
==============
lightweight quasi datastructure in [vvvv beta](https://vvvv.org) 

## short
__bundle links/pins__ into named groups (think Cons with multiple types married with s/r) while not giving up visual __dataflow__, __performance__ and vvvvs graph evaluation strategy (only evaluate what is requested from downstream.

## Features
* bundle __any types__ of pins together: native, c#, dx11, even from dynamic plugins or vl. system will update automatically when you change the vl or c# type. or drop in any plugin, the type will immediately be usable with struct.
* system is __evaluation aware__. if no split is pulling data, the join will disable evaluation upstream. even better: this happens on a per pin basis.
* Nil vs S+H: split nodes automatically hold the last valid output. useful for calculating initial config values (then just disabling the nodes doing that) or io data not arriving every frame. can be disabled on the node.
* __minimal data copying__ - optimal performance: no matter through how many subpatches and nodes struct is piped, actual moving of data is performed directly between the join input pin and the split output pin. (means this should perform better than a couple of zip and unzips in a row)
* datatype safety is enforced via the name: no accidental splitting of the wrong data. but Getters and Setters provide a way to access and modify different structs together.

## Performance and Stability
* the only performance penalty is the one of copying between native and c# world which you also have on vl integration nodes or any plugins. as mentioned, copying is kept to an absolute minimum and performed on the faster stream interface where possible.
* the rest of the system (gui windows, type checking, pin handling,...) only draws performance while you are actively patching and changing things.
* the system just calls the usual plugininterface methods, no extra copying or allocation or creation of big datatypes, thus memoryfootprint is be minimal. (even the automatic s+h feature just relies on the backing buffer of the plugininterface)
* has been in use successfully in various agencies in big projects already

## Build
* vvvv beta required
* dx11-vvvv optional
* edit ./Default.Project.settings and insert your local vvvvPath
* launch sln with VS and build

## License
free for non-commercial and educational use
commercial usage requires licensing - get in touch, play fair and support
(license ranges from a note of appreciation for indy use, to fair share for industrial scale)