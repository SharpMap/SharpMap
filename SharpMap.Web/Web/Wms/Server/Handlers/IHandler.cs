namespace SharpMap.Web.Wms.Server.Handlers
{
    public interface IHandler
    {
        void Handle(Map map, IContext context);
    }
}