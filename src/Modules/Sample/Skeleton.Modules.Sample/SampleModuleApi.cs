using Skeleton.Modules.Sample.Contracts;

namespace Skeleton.Modules.Sample;

internal sealed class SampleModuleApi(NoteStore store) : ISampleModuleApi
{
    public Task<NoteDto?> GetNoteAsync(Guid id, CancellationToken cancellationToken = default) =>
        Task.FromResult(store.Find(id)?.ToDto());
}
