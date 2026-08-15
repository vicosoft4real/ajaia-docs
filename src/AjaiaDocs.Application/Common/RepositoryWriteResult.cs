namespace AjaiaDocs.Application.Common;

public sealed record RepositoryWriteResult(bool DocumentExists, bool IsOwner,
    int? CurrentVersion);
