using System.Reflection;

namespace IotPipeline.Platform.Features.API.Shared.Modules;

public static class ModuleExtensions
{
    public static IEndpointRouteBuilder MapProjectModules(this IEndpointRouteBuilder app)
    {
        var moduleTypes = Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(t => typeof(IModule).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

        foreach (var type in moduleTypes)
        {
            var module = (IModule)Activator.CreateInstance(type)!;
            module.AddRoutes(app);
        }

        return app;
    }
}
