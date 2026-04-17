using vitacure.Application.Abstractions;

namespace vitacure.Application;

public sealed class SlugConflictException : InvalidOperationException
{
    public SlugConflictException(
        string slug,
        string message,
        SlugEntityType? conflictingEntityType = null,
        int? conflictingEntityId = null)
        : base(message)
    {
        Slug = slug;
        ConflictingEntityType = conflictingEntityType;
        ConflictingEntityId = conflictingEntityId;
    }

    public string Slug { get; }

    public SlugEntityType? ConflictingEntityType { get; }

    public int? ConflictingEntityId { get; }
}
