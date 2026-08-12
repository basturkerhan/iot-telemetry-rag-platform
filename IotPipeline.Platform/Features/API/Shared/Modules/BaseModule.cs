namespace IotPipeline.Platform.Features.API.Shared.Modules;

public abstract class BaseModule : IModule
{
    protected abstract string RouteSegment { get; }
    protected virtual string ApiPrefix => "/api";

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var prefix = ApiPrefix.TrimEnd('/');
        var segment = RouteSegment.TrimStart('/');

        var group = app.MapGroup($"{prefix}/{segment}");

        DefineRoutes(group);
    }

    protected abstract void DefineRoutes(RouteGroupBuilder group);
}
