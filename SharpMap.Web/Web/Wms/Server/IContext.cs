namespace SharpMap.Web.Wms.Server
{
    public interface IContext
    {
        IContextRequest Request { get; }
        IContextResponse Response { get; }
    }
}
