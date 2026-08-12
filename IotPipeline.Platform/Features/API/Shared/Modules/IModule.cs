namespace IotPipeline.Platform.Features.API.Shared.Modules;

public interface IModule
{
    void AddRoutes(IEndpointRouteBuilder app);
}
