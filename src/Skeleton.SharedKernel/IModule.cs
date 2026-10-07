using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Skeleton.SharedKernel;

/// <summary>
/// Entry point of a module. The Host calls <see cref="Register"/> while building services
/// and <see cref="MapEndpoints"/> after the app is built.
/// </summary>
public interface IModule
{
    void Register(IServiceCollection services, IConfiguration configuration);

    void MapEndpoints(IEndpointRouteBuilder endpoints);
}
