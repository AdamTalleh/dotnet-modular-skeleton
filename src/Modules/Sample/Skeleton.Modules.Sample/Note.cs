using System.Collections.Concurrent;
using Skeleton.Modules.Sample.Contracts;
using Skeleton.SharedKernel;

namespace Skeleton.Modules.Sample;

internal sealed record Note(Guid Id, string Title, DateTimeOffset CreatedAt)
{
    public NoteDto ToDto() => new(Id, Title, CreatedAt);
}

internal static class SampleErrors
{
    public static Error NoteNotFound(Guid id) => Error.NotFound("Sample.NoteNotFound", $"Note '{id}' was not found.");
}

// NOTE: in-memory, per-process, lost on restart. Replace with the product's persistence (e.g. an EF Core DbContext).
internal sealed class NoteStore
{
    private readonly ConcurrentDictionary<Guid, Note> _notes = new();

    public void Add(Note note) => _notes[note.Id] = note;

    public Note? Find(Guid id) => _notes.GetValueOrDefault(id);
}
