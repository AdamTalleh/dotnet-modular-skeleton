using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Skeleton.Modules.Sample.Contracts;
using Skeleton.Modules.Sample.Features;
using Skeleton.SharedKernel;

namespace Skeleton.Modules.Sample;

public sealed class SampleModule : IModule
{
    public void Register(IServiceCollection services, IConfiguration configuration)
    {
        // Built-in validation source generator only sees endpoints in the assembly that calls AddValidation,
        // so every module that maps endpoints calls it.
        services.AddValidation();
        services.TryAddSingleton(TimeProvider.System);

        services.AddSingleton<NoteStore>();
        services.AddScoped<CreateNoteHandler>();
        services.AddScoped<GetNoteHandler>();
        services.AddScoped<ISampleModuleApi, SampleModuleApi>();
    }

    public void MapEndpoints(IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/sample").WithTags("Sample");

        CreateNoteEndpoint.Map(group);
        GetNoteEndpoint.Map(group);
    }
}
