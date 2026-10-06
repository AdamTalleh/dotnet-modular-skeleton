namespace Skeleton.Modules.Sample.Contracts;

/// <summary>
/// The only way other modules may talk to the Sample module (in-process call).
/// Other modules reference this Contracts project, never Skeleton.Modules.Sample itself.
/// </summary>
public interface ISampleModuleApi
{
    Task<NoteDto?> GetNoteAsync(Guid id, CancellationToken cancellationToken = default);
}
