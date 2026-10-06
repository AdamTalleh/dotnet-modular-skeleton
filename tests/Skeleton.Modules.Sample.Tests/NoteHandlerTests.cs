using Skeleton.Modules.Sample.Features;
using Skeleton.SharedKernel;

namespace Skeleton.Modules.Sample.Tests;

public class NoteHandlerTests
{
    private static readonly DateTimeOffset Now = new(2026, 1, 1, 12, 0, 0, TimeSpan.Zero);

    private readonly NoteStore _store = new();

    [Fact]
    public void CreateNote_TrimsTitle_AndStampsCurrentTime()
    {
        var handler = new CreateNoteHandler(_store, new FixedTimeProvider(Now));

        var note = handler.Handle(new CreateNoteRequest("  hello  "));

        note.Title.ShouldBe("hello");
        note.CreatedAt.ShouldBe(Now);
        _store.Find(note.Id).ShouldNotBeNull();
    }

    [Fact]
    public void GetNote_ReturnsNote_WhenItExists()
    {
        var created = new CreateNoteHandler(_store, new FixedTimeProvider(Now)).Handle(new CreateNoteRequest("hello"));

        var result = new GetNoteHandler(_store).Handle(created.Id);

        result.IsSuccess.ShouldBeTrue();
        result.Value.ShouldBe(created);
    }

    [Fact]
    public void GetNote_ReturnsNotFoundError_WhenMissing()
    {
        var result = new GetNoteHandler(_store).Handle(Guid.NewGuid());

        result.IsSuccess.ShouldBeFalse();
        result.Error.Type.ShouldBe(ErrorType.NotFound);
        Should.Throw<InvalidOperationException>(() => result.Value);
    }

    private sealed class FixedTimeProvider(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now;
    }
}
