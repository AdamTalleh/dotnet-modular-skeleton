using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Skeleton.Modules.Sample.Contracts;
using Skeleton.SharedKernel;

namespace Skeleton.Modules.Sample.Features;

internal sealed class GetNoteHandler(NoteStore store)
{
    public Result<NoteDto> Handle(Guid id)
    {
        var note = store.Find(id);
        if (note is null)
        {
            return SampleErrors.NoteNotFound(id);
        }

        return note.ToDto();
    }
}

internal static class GetNoteEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints) =>
        endpoints.MapGet("/notes/{id:guid}", Handle)
            .WithName("GetNote");

    private static Results<Ok<NoteDto>, ProblemHttpResult> Handle(Guid id, GetNoteHandler handler)
    {
        var result = handler.Handle(id);
        return result.IsSuccess ? TypedResults.Ok(result.Value) : result.Error.ToProblem();
    }
}
