namespace VVVV.Struct.Core
{
    public interface IStruct { }

    public interface IStructIO : IStruct
    {
        //[Import] inheriting attributes not supported
        IIOManager IOManager { get; set; }
    }

    public interface IStructFieldGetter : IStructIO { }

    public interface IStructFieldSetter : IStructIO { }

    public interface IStructDeclarer : IStructIO
    {
        Declaration Declaration { get; set; }
    }
}