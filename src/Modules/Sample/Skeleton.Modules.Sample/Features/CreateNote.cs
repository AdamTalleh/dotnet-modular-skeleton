using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Routing;
using Skeleton.Modules.Sample.Contracts;

namespace Skeleton.Modules.Sample.Features;

// Must be public: the .NET 10 validation source generator ignores non-public request types (validation silently skipped).
public sealed record CreateNoteRequest([Required, StringLength(200, MinimumLength = 1)] string Title);

internal sealed class CreateNoteHandler(NoteStore store, TimeProvider time)
{
    // Synchronous because the store is in-memory; real handlers doing I/O are async and take a CancellationToken.
    public NoteDto Handle(CreateNoteRequest request)
    {
        var note = new Note(Guid.NewGuid(), request.Title.Trim(), time.GetUtcNow());
        store.Add(note);
        return note.ToDto();
    }
}

internal static class CreateNoteEndpoint
{
    public static void Map(IEndpointRouteBuilder endpoints) =>
        endpoints.MapPost("/notes", Handle)
            .WithName("CreateNote")
            .ProducesValidationProblem();

    private static Created<NoteDto> Handle(CreateNoteRequest request, CreateNoteHandler handler)
    {
        var note = handler.Handle(request);
        return TypedResults.Created($"/sample/notes/{note.Id}", note);
    }
}
