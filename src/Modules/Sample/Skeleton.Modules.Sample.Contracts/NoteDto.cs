namespace Skeleton.Modules.Sample.Contracts;

public sealed record NoteDto(Guid Id, string Title, DateTimeOffset CreatedAt);
