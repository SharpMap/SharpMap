namespace SharpMap.Web.Wms.Server.Handlers
{
    public interface IHandler
    {
        IHandlerResponse Handle(Map map, IContextRequest request);
    }

    public interface IHandlerResponse
    {
        void WriteToContextAndFlush(IContextResponse response);
    }
}