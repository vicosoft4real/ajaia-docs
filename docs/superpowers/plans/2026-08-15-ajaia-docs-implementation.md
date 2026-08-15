# Ajaia Docs Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Ship a one-URL .NET 10 and React collaborative document editor that lets seeded reviewers create, format, import, persist, share, and safely co-edit documents.

**Architecture:** A four-project Clean Architecture backend exposes thin Carter Minimal API modules over Dapper/PostgreSQL feature handlers. A React/Vite SPA uses RTK Query and Lexical, is compiled into the API image, and serializes title/content mutations through an optimistic-concurrency save coordinator.

**Tech Stack:** .NET 10, C# 14, ASP.NET Core cookie authentication and antiforgery, Carter, FluentValidation, Dapper, PostgreSQL 16, xUnit, Testcontainers, React 18, TypeScript 5.9, Vite 7, RTK Query, Lexical 0.41, Tailwind 3, Radix Dialog, Vitest, Testing Library, Playwright with Google Chrome, Docker, Render.

**Spec:** `docs/superpowers/specs/2026-08-15-ajaia-docs-design.md`

**Tracking issue:** https://github.com/vicosoft4real/ajaia-docs/issues/1 — keep open until implementation is merged.

## Global Constraints

- Target `net10.0`; pin SDK `10.0.0` with `rollForward: latestMajor`.
- Use Node 22 and `pnpm@10.33.4`; commit `pnpm-lock.yaml`.
- Use PostgreSQL for local, test, and deployed persistence; never store runtime documents on the service filesystem.
- Use one production origin: the .NET API serves `web/dist` as its SPA.
- Use direct handler injection rather than MediatR; this preserves feature slices without adding a commercial-license concern or a bespoke dispatcher to the timebox.
- Use result values for expected failures and sanitized `application/problem+json` responses at the API boundary.
- Derive the acting user only from the encrypted HttpOnly cookie; never accept an actor/owner ID from a document mutation body.
- Allow collaborators to edit content only; rename, share, revoke, and delete stay owner-only in the UI and API.
- Require an expected version for title/content writes; a stale version returns `409 conflict` and never overwrites data.
- Accept only strict UTF-8 `.txt` and `.md` imports up to 1 MiB; normal editor payloads are at most 2 MiB.
- Use the approved semantic colors, Manrope UI font, Literata editor font, visible focus, WCAG AA contrast, and reduced-motion support.
- Treat generated project/package/configuration scaffolding as the sole TDD setup exception. Every custom production function or method starts with a test that is run and observed failing for the expected reason.
- Use Google Chrome for the release browser journey and visual validation.
- Preserve the human handoffs: the candidate records the video and uploads the final package to Google Drive.

## File and Responsibility Map

```text
src/AjaiaDocs.Core/                 domain entities, results, access decisions
src/AjaiaDocs.Application/          feature commands/queries, validators, ports, DTOs
src/AjaiaDocs.Infrastructure/       Dapper repositories, imports, migrations, seeding
src/AjaiaDocs.Api/                  Carter routes, cookies, antiforgery, HTTP mapping, SPA
tests/AjaiaDocs.UnitTests/          real domain/application behavior with narrow substitutes
tests/AjaiaDocs.IntegrationTests/   Testcontainers PostgreSQL and HTTP journeys
web/src/app/                        Redux store, router, application bootstrap
web/src/store/api/                  same-origin RTK Query contract
web/src/components/ui/              accessible visual primitives
web/src/features/auth/              demo session selection and route guard
web/src/features/documents/         library, import, cards, owner actions
web/src/features/editor/            Lexical, toolbar, serialization, save coordination
web/src/features/sharing/           grant/revoke dialog
web/e2e/                            Chrome product journey and screenshot capture
docs/screenshots/                   committed desktop/mobile evidence
```

---

### Task 1: Repository Foundation and Core Document Domain

**Files:**

- Create: `global.json`
- Create: `Directory.Build.props`
- Create: `Directory.Packages.props`
- Create: `.editorconfig`
- Create: `.gitignore`
- Create: `AjaiaDocs.sln`
- Create: `src/AjaiaDocs.Core/AjaiaDocs.Core.csproj`
- Create: `src/AjaiaDocs.Application/AjaiaDocs.Application.csproj`
- Create: `src/AjaiaDocs.Infrastructure/AjaiaDocs.Infrastructure.csproj`
- Create: `src/AjaiaDocs.Api/AjaiaDocs.Api.csproj`
- Create: `tests/AjaiaDocs.UnitTests/AjaiaDocs.UnitTests.csproj`
- Create: `tests/AjaiaDocs.IntegrationTests/AjaiaDocs.IntegrationTests.csproj`
- Create: `src/AjaiaDocs.Core/Common/ErrorType.cs`
- Create: `src/AjaiaDocs.Core/Common/AjaiaError.cs`
- Create: `src/AjaiaDocs.Core/Common/Result.cs`
- Create: `src/AjaiaDocs.Core/Users/User.cs`
- Create: `src/AjaiaDocs.Core/Documents/ContentFormat.cs`
- Create: `src/AjaiaDocs.Core/Documents/DocumentOperation.cs`
- Create: `src/AjaiaDocs.Core/Documents/DocumentAccessDecision.cs`
- Create: `src/AjaiaDocs.Core/Documents/DocumentAccessPolicy.cs`
- Create: `src/AjaiaDocs.Core/Documents/Document.cs`
- Create: `src/AjaiaDocs.Core/Documents/DocumentShare.cs`
- Test: `tests/AjaiaDocs.UnitTests/Core/DocumentTests.cs`
- Test: `tests/AjaiaDocs.UnitTests/Core/DocumentAccessPolicyTests.cs`
- Test: `tests/AjaiaDocs.UnitTests/Core/DocumentShareTests.cs`

**Interfaces:**

- Produces: `Result<T>`, `AjaiaError`, `User`, `Document`, `DocumentShare`, `ContentFormat`, and `DocumentAccessPolicy` for every later backend task.
- `Document.Create`, `Rename`, and `UpdateContent` return new validated documents; only a matching `expectedVersion` increments `Version`.

- [ ] **Step 1: Generate behavior-free solution scaffolding**

Run from the repository root:

```bash
dotnet new sln -n AjaiaDocs --format sln
dotnet new classlib -n AjaiaDocs.Core -o src/AjaiaDocs.Core --framework net10.0
dotnet new classlib -n AjaiaDocs.Application -o src/AjaiaDocs.Application --framework net10.0
dotnet new classlib -n AjaiaDocs.Infrastructure -o src/AjaiaDocs.Infrastructure --framework net10.0
dotnet new web -n AjaiaDocs.Api -o src/AjaiaDocs.Api --framework net10.0
dotnet new xunit -n AjaiaDocs.UnitTests -o tests/AjaiaDocs.UnitTests --framework net10.0
dotnet new xunit -n AjaiaDocs.IntegrationTests -o tests/AjaiaDocs.IntegrationTests --framework net10.0
dotnet sln AjaiaDocs.sln add src/AjaiaDocs.Core/AjaiaDocs.Core.csproj src/AjaiaDocs.Application/AjaiaDocs.Application.csproj src/AjaiaDocs.Infrastructure/AjaiaDocs.Infrastructure.csproj src/AjaiaDocs.Api/AjaiaDocs.Api.csproj tests/AjaiaDocs.UnitTests/AjaiaDocs.UnitTests.csproj tests/AjaiaDocs.IntegrationTests/AjaiaDocs.IntegrationTests.csproj
dotnet add src/AjaiaDocs.Application/AjaiaDocs.Application.csproj reference src/AjaiaDocs.Core/AjaiaDocs.Core.csproj
dotnet add src/AjaiaDocs.Infrastructure/AjaiaDocs.Infrastructure.csproj reference src/AjaiaDocs.Core/AjaiaDocs.Core.csproj src/AjaiaDocs.Application/AjaiaDocs.Application.csproj
dotnet add src/AjaiaDocs.Api/AjaiaDocs.Api.csproj reference src/AjaiaDocs.Core/AjaiaDocs.Core.csproj src/AjaiaDocs.Application/AjaiaDocs.Application.csproj src/AjaiaDocs.Infrastructure/AjaiaDocs.Infrastructure.csproj
dotnet add tests/AjaiaDocs.UnitTests/AjaiaDocs.UnitTests.csproj reference src/AjaiaDocs.Core/AjaiaDocs.Core.csproj src/AjaiaDocs.Application/AjaiaDocs.Application.csproj
dotnet add tests/AjaiaDocs.IntegrationTests/AjaiaDocs.IntegrationTests.csproj reference src/AjaiaDocs.Api/AjaiaDocs.Api.csproj src/AjaiaDocs.Infrastructure/AjaiaDocs.Infrastructure.csproj
```

Pin Carter `10.0.0`, FluentValidation `12.1.0`, Dapper `2.1.66`, Npgsql `10.0.0`, Microsoft.AspNetCore.Mvc.Testing `10.0.0`, Testcontainers.PostgreSql `4.12.0`, NSubstitute `5.3.0`, xUnit `2.9.3`, xunit.runner.visualstudio `3.1.5`, Microsoft.NET.Test.Sdk `18.6.0`, and coverlet.collector `6.0.4` in `Directory.Packages.props`. Enable nullable, implicit usings, deterministic builds, and warnings as errors in `Directory.Build.props`.

Remove generated inline `Version` attributes and add versionless package references with this ownership:

```xml
<!-- AjaiaDocs.Application.csproj -->
<PackageReference Include="FluentValidation" />

<!-- AjaiaDocs.Infrastructure.csproj -->
<PackageReference Include="Dapper" />
<PackageReference Include="Npgsql" />

<!-- AjaiaDocs.Api.csproj -->
<PackageReference Include="Carter" />

<!-- AjaiaDocs.UnitTests.csproj -->
<PackageReference Include="Microsoft.NET.Test.Sdk" />
<PackageReference Include="NSubstitute" />
<PackageReference Include="xunit" />
<PackageReference Include="xunit.runner.visualstudio" PrivateAssets="all" />
<PackageReference Include="coverlet.collector" PrivateAssets="all" />

<!-- AjaiaDocs.IntegrationTests.csproj -->
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" />
<PackageReference Include="Microsoft.NET.Test.Sdk" />
<PackageReference Include="Testcontainers.PostgreSql" />
<PackageReference Include="xunit" />
<PackageReference Include="xunit.runner.visualstudio" PrivateAssets="all" />
<PackageReference Include="coverlet.collector" PrivateAssets="all" />
```

- [ ] **Step 2: Write the first failing document tests**

```csharp
[Fact]
public void Create_trims_title_and_starts_at_version_one()
{
    var now = new DateTimeOffset(2026, 8, 15, 12, 0, 0, TimeSpan.Zero);
    var result = Document.Create(
        Guid.Parse("10000000-0000-0000-0000-000000000001"),
        Guid.Parse("00000000-0000-0000-0000-000000000001"),
        "  Review notes  ",
        ContentFormat.Lexical,
        "{\"root\":{\"children\":[]}}",
        string.Empty,
        now);

    Assert.True(result.IsSuccess);
    Assert.Equal("Review notes", result.Value.Title);
    Assert.Equal(1, result.Value.Version);
}

[Theory]
[InlineData("")]
[InlineData("   ")]
public void Create_rejects_blank_title(string title)
{
    var result = Document.Create(Guid.NewGuid(), Guid.NewGuid(), title,
        ContentFormat.Lexical, "{}", string.Empty, DateTimeOffset.UtcNow);

    Assert.False(result.IsSuccess);
    Assert.Equal("title_required", result.Error.Code);
}
```

Add literal boundary cases for 120 and 121 characters plus matching/stale versions.

- [ ] **Step 3: Run the tests and verify RED**

Run:

```bash
dotnet test tests/AjaiaDocs.UnitTests/AjaiaDocs.UnitTests.csproj --filter "FullyQualifiedName~DocumentTests"
```

Expected: compilation fails because `Document`, `ContentFormat`, and `Result<T>` do not exist.

- [ ] **Step 4: Implement the minimal result and document model**

Use these exact public contracts:

```csharp
public enum ErrorType { Validation, NotFound, Forbidden, Conflict, Failure }

public sealed record AjaiaError(string Code, string Message, ErrorType Type);

public sealed class Result<T>
{
    private readonly T? _value;
    private readonly AjaiaError? _error;
    private Result(T? value, AjaiaError? error) => (_value, _error) = (value, error);
    public bool IsSuccess => _error is null;
    public T Value => IsSuccess ? _value! : throw new InvalidOperationException("A failed result has no value.");
    public AjaiaError Error => !IsSuccess ? _error! : throw new InvalidOperationException("A successful result has no error.");
    public static Result<T> Success(T value) => new(value, null);
    public static Result<T> Failure(AjaiaError error) => new(default, error);
}

public enum ContentFormat { Lexical, Markdown, PlainText }

public sealed record Document(
    Guid Id,
    Guid OwnerId,
    string Title,
    ContentFormat ContentFormat,
    string Content,
    string PlainText,
    int Version,
    DateTimeOffset CreatedAt,
    DateTimeOffset UpdatedAt)
{
    public const int MaxTitleLength = 120;
    public const int MaxContentBytes = 2 * 1024 * 1024;

    public static Result<Document> Create(Guid id, Guid ownerId, string? title,
        ContentFormat contentFormat, string content, string plainText,
        DateTimeOffset now);

    public Result<Document> Rename(string? title, int expectedVersion,
        DateTimeOffset now);

    public Result<Document> UpdateContent(string content, string plainText,
        ContentFormat contentFormat, int expectedVersion, DateTimeOffset now);
}
```

Return `title_required`, `title_too_long`, `content_too_large`, `invalid_content_format`, and `conflict` with the corresponding `ErrorType`.

- [ ] **Step 5: Run the document tests and verify GREEN**

```bash
dotnet test tests/AjaiaDocs.UnitTests/AjaiaDocs.UnitTests.csproj --filter "FullyQualifiedName~DocumentTests"
```

Expected: all `DocumentTests` pass with zero warnings.

- [ ] **Step 6: Write failing access and share tests**

```csharp
[Theory]
[InlineData(DocumentOperation.Read, true)]
[InlineData(DocumentOperation.EditContent, true)]
[InlineData(DocumentOperation.Rename, false)]
[InlineData(DocumentOperation.Share, false)]
[InlineData(DocumentOperation.RevokeShare, false)]
[InlineData(DocumentOperation.Delete, false)]
public void Collaborator_only_reads_and_edits(DocumentOperation operation, bool allowed)
{
    var decision = DocumentAccessPolicy.Decide(Guid.NewGuid(), Guid.NewGuid(), true, operation);
    Assert.Equal(allowed, decision.Allowed);
    Assert.Equal(allowed ? null : "owner_required", decision.ErrorCode);
}

[Fact]
public void No_access_is_reported_as_not_found()
{
    var decision = DocumentAccessPolicy.Decide(Guid.NewGuid(), Guid.NewGuid(), false,
        DocumentOperation.Read);
    Assert.False(decision.Allowed);
    Assert.True(decision.IsNotFound);
}
```

Add `DocumentShare.Create` tests for owner self-share and valid collaborator share.

- [ ] **Step 7: Run access tests and verify RED**

```bash
dotnet test tests/AjaiaDocs.UnitTests/AjaiaDocs.UnitTests.csproj --filter "FullyQualifiedName~DocumentAccessPolicyTests|FullyQualifiedName~DocumentShareTests"
```

Expected: compilation fails because access/share types are missing.

- [ ] **Step 8: Implement access and share types, then verify GREEN**

```csharp
public enum DocumentOperation { Read, EditContent, Rename, Share, RevokeShare, Delete }

public sealed record DocumentAccessDecision(bool Allowed, bool IsNotFound, string? ErrorCode);

public static class DocumentAccessPolicy
{
    public static DocumentAccessDecision Decide(Guid actorId, Guid ownerId,
        bool hasShare, DocumentOperation operation);
}

public sealed record DocumentShare(Guid DocumentId, Guid UserId,
    Guid SharedByUserId, DateTimeOffset CreatedAt)
{
    public static Result<DocumentShare> Create(Guid documentId, Guid ownerId,
        Guid userId, Guid sharedByUserId, DateTimeOffset now);
}
```

Run:

```bash
dotnet test tests/AjaiaDocs.UnitTests/AjaiaDocs.UnitTests.csproj --filter "FullyQualifiedName~Core"
```

Expected: every Core test passes.

- [ ] **Step 9: Commit the core slice**

```bash
git add global.json Directory.Build.props Directory.Packages.props .editorconfig .gitignore AjaiaDocs.sln src/AjaiaDocs.Core src/AjaiaDocs.Application/AjaiaDocs.Application.csproj src/AjaiaDocs.Infrastructure/AjaiaDocs.Infrastructure.csproj src/AjaiaDocs.Api/AjaiaDocs.Api.csproj tests/AjaiaDocs.UnitTests tests/AjaiaDocs.IntegrationTests/AjaiaDocs.IntegrationTests.csproj
git commit -m "feat(core): define the document ownership domain"
```

---

### Task 2: Application Contracts and Create/List/Open Handlers

**Files:**

- Create: `src/AjaiaDocs.Application/Common/DocumentScope.cs`
- Create: `src/AjaiaDocs.Application/Common/DocumentContentDefaults.cs`
- Create: `src/AjaiaDocs.Application/Common/RepositoryWriteResult.cs`
- Create: `src/AjaiaDocs.Application/Common/Interfaces/IDocumentRepository.cs`
- Create: `src/AjaiaDocs.Application/Common/Interfaces/IUserRepository.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/DocumentDtos.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/CreateDocument/CreateDocumentCommand.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/CreateDocument/CreateDocumentValidator.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/CreateDocument/CreateDocumentHandler.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/ListDocuments/ListDocumentsQuery.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/ListDocuments/ListDocumentsHandler.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/GetDocument/GetDocumentQuery.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/GetDocument/GetDocumentHandler.cs`
- Create: `src/AjaiaDocs.Application/DependencyInjection.cs`
- Create: `tests/AjaiaDocs.UnitTests/Fixtures/DocumentFixtures.cs`
- Test: `tests/AjaiaDocs.UnitTests/Application/CreateDocumentHandlerTests.cs`
- Test: `tests/AjaiaDocs.UnitTests/Application/ListAndGetDocumentHandlerTests.cs`

**Interfaces:**

- Consumes: Core `Document`, `User`, `Result<T>`, and access decisions.
- Produces: document DTOs and repository interfaces used verbatim by Infrastructure, API, and frontend contracts.

```csharp
public enum DocumentScope { All, Owned, Shared }

public static class DocumentContentDefaults
{
    public const string EmptyLexical =
        "{\"root\":{\"children\":[{\"children\":[],\"direction\":null," +
        "\"format\":\"\",\"indent\":0,\"type\":\"paragraph\",\"version\":1," +
        "\"textFormat\":0,\"textStyle\":\"\"}],\"direction\":null," +
        "\"format\":\"\",\"indent\":0,\"type\":\"root\",\"version\":1}}";
}

public sealed record UserSummaryDto(Guid Id, string DisplayName, string Email,
    string AvatarColor);

public sealed record DocumentListItemDto(Guid Id, Guid OwnerId, string Title,
    string ContentFormat, string PlainText, int Version, DateTimeOffset UpdatedAt,
    UserSummaryDto Owner, bool IsOwner);

public sealed record DocumentDto(Guid Id, Guid OwnerId, string Title,
    string ContentFormat, string Content, string PlainText, int Version,
    DateTimeOffset CreatedAt, DateTimeOffset UpdatedAt, UserSummaryDto Owner,
    bool IsOwner, bool CanEdit, bool CanRename, bool CanShare, bool CanDelete);

public interface IDocumentRepository
{
    Task<Result<DocumentDto>> CreateAsync(Document document, CancellationToken ct);
    Task<Result<IReadOnlyList<DocumentListItemDto>>> ListAsync(
        Guid actorId, DocumentScope scope, CancellationToken ct);
    Task<Result<DocumentDto>> GetAsync(Guid actorId, Guid documentId,
        CancellationToken ct);
    Task<Result<DocumentDto>> UpdateContentAsync(Guid actorId, Guid documentId,
        string content, string plainText, ContentFormat format,
        int expectedVersion, CancellationToken ct);
    Task<Result<DocumentDto>> RenameAsync(Guid actorId, Guid documentId,
        string title, int expectedVersion, CancellationToken ct);
    Task<Result<bool>> DeleteAsync(Guid actorId, Guid documentId,
        CancellationToken ct);
}

public interface IUserRepository
{
    Task<Result<User>> GetSeededAsync(Guid userId, CancellationToken ct);
    Task<Result<IReadOnlyList<User>>> ListShareCandidatesAsync(Guid actorId,
        Guid documentId, CancellationToken ct);
}

public static class DocumentFixtures
{
    public static DocumentDto Dto(Document document, bool isOwner) => new(
        document.Id, document.OwnerId, document.Title,
        document.ContentFormat switch {
            ContentFormat.Lexical => "lexical",
            ContentFormat.Markdown => "markdown",
            ContentFormat.PlainText => "plainText",
            _ => throw new ArgumentOutOfRangeException()
        }, document.Content,
        document.PlainText, document.Version, document.CreatedAt,
        document.UpdatedAt,
        new UserSummaryDto(document.OwnerId, "Amina Okafor",
            "amina@example.test", "#365CF5"),
        isOwner, true, isOwner, isOwner, isOwner);
}
```

- [ ] **Step 1: Write failing handler tests**

```csharp
[Fact]
public async Task Create_uses_cookie_actor_as_owner_and_returns_version_one()
{
    var actorId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    var repository = Substitute.For<IDocumentRepository>();
    repository.CreateAsync(Arg.Any<Document>(), Arg.Any<CancellationToken>())
        .Returns(call => Result<DocumentDto>.Success(DocumentFixtures.Dto(
            call.Arg<Document>(), isOwner: true)));
    var handler = new CreateDocumentHandler(repository,
        new CreateDocumentValidator(), TimeProvider.System);

    var result = await handler.HandleAsync(actorId,
        new CreateDocumentCommand("  Sprint brief  "), CancellationToken.None);

    Assert.True(result.IsSuccess);
    Assert.Equal(actorId, result.Value.OwnerId);
    Assert.Equal("Sprint brief", result.Value.Title);
    Assert.Equal(1, result.Value.Version);
}
```

Add tests proving list scope is forwarded and inaccessible get errors are preserved.

- [ ] **Step 2: Run tests and verify RED**

```bash
dotnet test tests/AjaiaDocs.UnitTests/AjaiaDocs.UnitTests.csproj --filter "FullyQualifiedName~CreateDocumentHandlerTests|FullyQualifiedName~ListAndGetDocumentHandlerTests"
```

Expected: compilation fails because the application contracts and handlers are missing.

- [ ] **Step 3: Implement minimal DTOs, validators, and handlers**

```csharp
public sealed record CreateDocumentCommand(string? Title);

public sealed class CreateDocumentHandler(
    IDocumentRepository repository,
    IValidator<CreateDocumentCommand> validator,
    TimeProvider timeProvider)
{
    public Task<Result<DocumentDto>> HandleAsync(Guid actorId,
        CreateDocumentCommand command, CancellationToken ct);
}
```

Use `Guid.CreateVersion7()`, default a missing title to `Untitled document`, use `DocumentContentDefaults.EmptyLexical`, and map scope strings only through `DocumentScope`.

- [ ] **Step 4: Run application tests and verify GREEN**

```bash
dotnet test tests/AjaiaDocs.UnitTests/AjaiaDocs.UnitTests.csproj --filter "FullyQualifiedName~Application"
```

Expected: handler tests pass and substitutes are only used at repository boundaries.

- [ ] **Step 5: Commit the application read slice**

```bash
git add src/AjaiaDocs.Application tests/AjaiaDocs.UnitTests/Application
git commit -m "feat(api): define document creation and read use cases"
```

---

### Task 3: PostgreSQL Migrations, Demo Users, and Read Persistence

**Files:**

- Create: `src/AjaiaDocs.Infrastructure/Data/AjaiaDbConnectionFactory.cs`
- Create: `src/AjaiaDocs.Infrastructure/Data/Migrations/001_CreateSchema.sql`
- Create: `src/AjaiaDocs.Infrastructure/Data/Migrations/002_SeedDemoUsers.sql`
- Create: `src/AjaiaDocs.Infrastructure/Data/Migrations/SchemaMigrationRunner.cs`
- Create: `src/AjaiaDocs.Infrastructure/Data/Repositories/DocumentRepository.cs`
- Create: `src/AjaiaDocs.Infrastructure/Data/Repositories/UserRepository.cs`
- Create: `src/AjaiaDocs.Infrastructure/DependencyInjection.cs`
- Modify: `src/AjaiaDocs.Infrastructure/AjaiaDocs.Infrastructure.csproj`
- Create: `tests/AjaiaDocs.IntegrationTests/Infrastructure/PostgresFixture.cs`
- Create: `tests/AjaiaDocs.IntegrationTests/Infrastructure/DemoUsers.cs`
- Create: `tests/AjaiaDocs.IntegrationTests/Infrastructure/MigrationTests.cs`
- Create: `tests/AjaiaDocs.IntegrationTests/Infrastructure/DocumentRepositoryReadTests.cs`

**Interfaces:**

- Implements: `IDocumentRepository.CreateAsync`, `ListAsync`, and `GetAsync` from Task 2.
- Produces: stable demo IDs `00000000-0000-0000-0000-000000000001`, `00000000-0000-0000-0000-000000000002`, and `00000000-0000-0000-0000-000000000003` for API/browser tests.

```csharp
public static class DemoUsers
{
    public static readonly Guid AminaId = Guid.Parse("00000000-0000-0000-0000-000000000001");
    public static readonly Guid ChidiId = Guid.Parse("00000000-0000-0000-0000-000000000002");
    public static readonly Guid TayoId = Guid.Parse("00000000-0000-0000-0000-000000000003");
}
```

`PostgresFixture.ResetAsync()` truncates `document_shares` and `documents` with identity restart after each test while retaining the three seeded users. Put integration tests in one xUnit collection bound to the shared fixture so parallel tests never mutate the same database concurrently.

- [ ] **Step 1: Write failing migration and read tests**

```csharp
[Fact]
public async Task Migrations_are_idempotent_and_seed_exactly_three_users()
{
    await _fixture.Migrator.MigrateAsync(CancellationToken.None);
    await _fixture.Migrator.MigrateAsync(CancellationToken.None);

    await using var connection = await _fixture.OpenConnectionAsync();
    var count = await connection.ExecuteScalarAsync<int>(
        "select count(*) from app_users where is_seeded = true");
    Assert.Equal(3, count);
}

[Fact]
public async Task Shared_scope_returns_only_documents_shared_with_actor()
{
    var rows = await _fixture.Documents.ListAsync(DemoUsers.ChidiId,
        DocumentScope.Shared, CancellationToken.None);
    Assert.True(rows.IsSuccess);
    Assert.All(rows.Value, row => Assert.False(row.IsOwner));
}
```

- [ ] **Step 2: Run tests and verify RED**

```bash
dotnet test tests/AjaiaDocs.IntegrationTests/AjaiaDocs.IntegrationTests.csproj --filter "FullyQualifiedName~MigrationTests|FullyQualifiedName~DocumentRepositoryReadTests"
```

Expected: compilation fails because the fixture, migrator, and repositories do not exist.

- [ ] **Step 3: Implement the schema and migration runner**

`001_CreateSchema.sql` must create `schema_migrations`, `app_users`, `documents`, and `document_shares`, plus these constraints/indexes:

```sql
CREATE TABLE documents (
    id uuid PRIMARY KEY,
    owner_id uuid NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
    title varchar(120) NOT NULL CHECK (length(btrim(title)) BETWEEN 1 AND 120),
    content_format varchar(20) NOT NULL
        CHECK (content_format IN ('lexical', 'markdown', 'plainText')),
    content text NOT NULL,
    plain_text text NOT NULL,
    version integer NOT NULL DEFAULT 1 CHECK (version > 0),
    created_at timestamptz NOT NULL,
    updated_at timestamptz NOT NULL
);

CREATE TABLE document_shares (
    document_id uuid NOT NULL REFERENCES documents(id) ON DELETE CASCADE,
    user_id uuid NOT NULL REFERENCES app_users(id) ON DELETE CASCADE,
    shared_by_user_id uuid NOT NULL REFERENCES app_users(id),
    created_at timestamptz NOT NULL,
    PRIMARY KEY (document_id, user_id)
);

CREATE INDEX ix_documents_owner_updated ON documents(owner_id, updated_at DESC);
CREATE INDEX ix_document_shares_user_document ON document_shares(user_id, document_id);
```

Add a `prevent_owner_document_share()` trigger that raises SQLSTATE `23514` when `NEW.user_id` equals the document owner. Seed Amina Okafor, Chidi Okeke, and Tayo Bello with the stable IDs and approved cobalt/mint/amber avatar colors using `ON CONFLICT (id) DO NOTHING`.

`SchemaMigrationRunner` must acquire `pg_advisory_lock(hashtext('ajaia-docs-migrations'))`, apply embedded SQL files transactionally in filename order, record each version, and release the lock in `finally`.

Embed both migrations from `AjaiaDocs.Infrastructure.csproj`:

```xml
<ItemGroup>
  <EmbeddedResource Include="Data/Migrations/*.sql" />
</ItemGroup>
```

- [ ] **Step 4: Implement Dapper create/list/get queries**

Use a single projection that joins the owner and computes access:

```sql
WHERE d.owner_id = @ActorId
   OR EXISTS (
       SELECT 1 FROM document_shares access_share
       WHERE access_share.document_id = d.id
         AND access_share.user_id = @ActorId)
```

Apply `Owned` as `d.owner_id = @ActorId` and `Shared` as an explicit share plus `d.owner_id <> @ActorId`. Order by `updated_at DESC, id`.

- [ ] **Step 5: Run persistence tests and verify GREEN**

```bash
dotnet test tests/AjaiaDocs.IntegrationTests/AjaiaDocs.IntegrationTests.csproj --filter "FullyQualifiedName~Infrastructure"
```

Expected: migrations are idempotent, seeded users are stable, and inaccessible reads return `not_found`.

- [ ] **Step 6: Commit persistence foundation**

```bash
git add src/AjaiaDocs.Infrastructure tests/AjaiaDocs.IntegrationTests/Infrastructure
git commit -m "feat(data): persist documents and seeded reviewers"
```

---

### Task 4: Versioned Content, Rename, and Delete Use Cases

**Files:**

- Create: `src/AjaiaDocs.Application/Features/Documents/UpdateContent/UpdateDocumentContentCommand.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/UpdateContent/UpdateDocumentContentValidator.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/UpdateContent/UpdateDocumentContentHandler.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/Rename/RenameDocumentCommand.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/Rename/RenameDocumentValidator.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/Rename/RenameDocumentHandler.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/Delete/DeleteDocumentCommand.cs`
- Create: `src/AjaiaDocs.Application/Features/Documents/Delete/DeleteDocumentHandler.cs`
- Modify: `src/AjaiaDocs.Infrastructure/Data/Repositories/DocumentRepository.cs`
- Test: `tests/AjaiaDocs.UnitTests/Application/DocumentWriteHandlerTests.cs`
- Test: `tests/AjaiaDocs.IntegrationTests/Infrastructure/DocumentRepositoryWriteTests.cs`

**Interfaces:**

```csharp
public sealed record UpdateDocumentContentCommand(string ContentFormat,
    string Content, string PlainText, int ExpectedVersion);
public sealed record RenameDocumentCommand(string Title, int ExpectedVersion);
public sealed record DeleteDocumentCommand(Guid DocumentId);
```

- [ ] **Step 1: Write failing unit and repository tests**

```csharp
[Fact]
public async Task Stale_content_update_returns_conflict_without_overwrite()
{
    var first = await _fixture.Documents.UpdateContentAsync(DemoUsers.AminaId,
        _documentId, DocumentContentDefaults.EmptyLexical, "first", ContentFormat.Lexical, 1,
        CancellationToken.None);
    var stale = await _fixture.Documents.UpdateContentAsync(DemoUsers.AminaId,
        _documentId, DocumentContentDefaults.EmptyLexical, "stale", ContentFormat.Lexical, 1,
        CancellationToken.None);

    Assert.True(first.IsSuccess);
    Assert.False(stale.IsSuccess);
    Assert.Equal("conflict", stale.Error.Code);
    Assert.Equal("first", (await _fixture.Documents.GetAsync(
        DemoUsers.AminaId, _documentId, CancellationToken.None)).Value.PlainText);
}
```

Add tests for collaborator content success, collaborator rename/delete `owner_required`, owner rename version increment, and cascading shares on delete.
Also mutate the lexical payload to missing `root.type`, missing `root.children`, and malformed JSON; each must fail with `invalid_editor_state` before the repository is called.

- [ ] **Step 2: Run tests and verify RED**

```bash
dotnet test tests/AjaiaDocs.UnitTests/AjaiaDocs.UnitTests.csproj --filter "FullyQualifiedName~DocumentWriteHandlerTests"
dotnet test tests/AjaiaDocs.IntegrationTests/AjaiaDocs.IntegrationTests.csproj --filter "FullyQualifiedName~DocumentRepositoryWriteTests"
```

Expected: write handler and SQL behavior are missing.

- [ ] **Step 3: Implement handlers and atomic SQL**

The update validator parses JSON and requires `root.type === "root"`, positive `root.version`, and an array-valued `root.children`. Content SQL must include owner-or-share access plus version:

```sql
UPDATE documents d
SET content = @Content,
    content_format = @ContentFormat,
    plain_text = @PlainText,
    version = version + 1,
    updated_at = @UpdatedAt
WHERE d.id = @DocumentId
  AND d.version = @ExpectedVersion
  AND (d.owner_id = @ActorId OR EXISTS (
      SELECT 1 FROM document_shares s
      WHERE s.document_id = d.id AND s.user_id = @ActorId))
RETURNING d.*;
```

Rename uses owner plus expected version. Delete uses owner only and returns `owner_required` for a known collaborator. Resolve zero-row writes with a follow-up access/version query that returns `not_found`, `owner_required`, or `conflict` without exposing an inaccessible ID.

- [ ] **Step 4: Run write tests and verify GREEN**

```bash
dotnet test tests/AjaiaDocs.UnitTests/AjaiaDocs.UnitTests.csproj --filter "FullyQualifiedName~DocumentWriteHandlerTests"
dotnet test tests/AjaiaDocs.IntegrationTests/AjaiaDocs.IntegrationTests.csproj --filter "FullyQualifiedName~DocumentRepositoryWriteTests"
```

Expected: writes enforce capabilities and stale updates preserve stored content.

- [ ] **Step 5: Commit the write slice**

```bash
git add src/AjaiaDocs.Application/Features/Documents src/AjaiaDocs.Infrastructure/Data/Repositories/DocumentRepository.cs tests/AjaiaDocs.UnitTests/Application/DocumentWriteHandlerTests.cs tests/AjaiaDocs.IntegrationTests/Infrastructure/DocumentRepositoryWriteTests.cs
git commit -m "feat(documents): save edits with optimistic concurrency"
```

---

### Task 5: Sharing and Strict Text Import

**Files:**

- Create: `src/AjaiaDocs.Application/Common/Interfaces/IDocumentShareRepository.cs`
- Modify: `src/AjaiaDocs.Application/Common/Interfaces/IUserRepository.cs`
- Create: `src/AjaiaDocs.Application/Features/Sharing/ShareDtos.cs`
- Create: `src/AjaiaDocs.Application/Features/Sharing/GetShareCandidates/GetShareCandidatesHandler.cs`
- Create: `src/AjaiaDocs.Application/Features/Sharing/ListShares/ListDocumentSharesHandler.cs`
- Create: `src/AjaiaDocs.Application/Features/Sharing/GrantShare/GrantDocumentShareHandler.cs`
- Create: `src/AjaiaDocs.Application/Features/Sharing/RevokeShare/RevokeDocumentShareHandler.cs`
- Create: `src/AjaiaDocs.Application/Features/Import/ImportedText.cs`
- Create: `src/AjaiaDocs.Application/Features/Import/StrictTextImportParser.cs`
- Create: `src/AjaiaDocs.Application/Features/Import/ImportDocumentHandler.cs`
- Create: `src/AjaiaDocs.Infrastructure/Data/Repositories/DocumentShareRepository.cs`
- Modify: `src/AjaiaDocs.Infrastructure/Data/Repositories/UserRepository.cs`
- Test: `tests/AjaiaDocs.UnitTests/Application/StrictTextImportParserTests.cs`
- Test: `tests/AjaiaDocs.UnitTests/Application/SharingHandlerTests.cs`
- Test: `tests/AjaiaDocs.IntegrationTests/Infrastructure/DocumentShareRepositoryTests.cs`

**Interfaces:**

```csharp
public sealed record ShareCandidateDto(Guid Id, string DisplayName, string Email,
    string AvatarColor);
public sealed record DocumentShareDto(Guid DocumentId, Guid UserId,
    string DisplayName, string Email, string AvatarColor,
    DateTimeOffset CreatedAt);

public interface IDocumentShareRepository
{
    Task<Result<IReadOnlyList<DocumentShareDto>>> ListAsync(Guid actorId,
        Guid documentId, CancellationToken ct);
    Task<Result<DocumentShareDto>> GrantAsync(Guid actorId, Guid documentId,
        Guid targetUserId, DateTimeOffset now, CancellationToken ct);
    Task<Result<bool>> RevokeAsync(Guid actorId, Guid documentId,
        Guid targetUserId, CancellationToken ct);
}

public sealed record ImportedText(string Title, ContentFormat Format,
    string Content, string PlainText);

public sealed class ImportDocumentHandler(StrictTextImportParser parser,
    IDocumentRepository documents, TimeProvider timeProvider)
{
    public Task<Result<DocumentDto>> HandleAsync(Guid actorId, string fileName,
        ReadOnlyMemory<byte> bytes, CancellationToken ct);
}
```

- [ ] **Step 1: Write failing import and sharing tests**

```csharp
[Fact]
public void Invalid_utf8_is_rejected()
{
    var bytes = new byte[] { 0xC3, 0x28 };
    var result = StrictTextImportParser.Parse("broken.md", bytes);
    Assert.False(result.IsSuccess);
    Assert.Equal("invalid_utf8", result.Error.Code);
}

[Theory]
[InlineData("notes.txt", ContentFormat.PlainText)]
[InlineData("NOTES.MD", ContentFormat.Markdown)]
public void Supported_file_is_persistable(string fileName, ContentFormat format)
{
    var result = StrictTextImportParser.Parse(fileName,
        Encoding.UTF8.GetBytes("# Shared plan"));
    Assert.True(result.IsSuccess);
    Assert.Equal(format, result.Value.Format);
    Assert.Equal("notes", result.Value.Title, ignoreCase: true);
}
```

Add the exact 1 MiB boundary, 1 MiB + 1 byte, whitespace-only file, unknown user, duplicate grant, owner self-share trigger, owner-only list/revoke, and cascade tests.

- [ ] **Step 2: Run tests and verify RED**

```bash
dotnet test tests/AjaiaDocs.UnitTests/AjaiaDocs.UnitTests.csproj --filter "FullyQualifiedName~StrictTextImportParserTests|FullyQualifiedName~SharingHandlerTests"
dotnet test tests/AjaiaDocs.IntegrationTests/AjaiaDocs.IntegrationTests.csproj --filter "FullyQualifiedName~DocumentShareRepositoryTests"
```

Expected: parser, handlers, and share repository are missing.

- [ ] **Step 3: Implement strict parser and sharing**

Decode with:

```csharp
var utf8 = new UTF8Encoding(encoderShouldEmitUTF8Identifier: false,
    throwOnInvalidBytes: true);
var content = utf8.GetString(bytes.Span);
```

Treat MIME as advisory, accept extensions case-insensitively, derive title from `Path.GetFileNameWithoutExtension`, trim to 120 characters, and fall back to `Untitled document`. Map PostgreSQL unique violation `23505` to `duplicate_share` and trigger check violation `23514` to `self_share`.

- [ ] **Step 4: Run sharing/import tests and verify GREEN**

```bash
dotnet test tests/AjaiaDocs.UnitTests/AjaiaDocs.UnitTests.csproj --filter "FullyQualifiedName~StrictTextImportParserTests|FullyQualifiedName~SharingHandlerTests"
dotnet test tests/AjaiaDocs.IntegrationTests/AjaiaDocs.IntegrationTests.csproj --filter "FullyQualifiedName~DocumentShareRepositoryTests"
```

Expected: all specified validation and owner-capability cases pass.

- [ ] **Step 5: Commit sharing and import**

```bash
git add src/AjaiaDocs.Application src/AjaiaDocs.Infrastructure/Data/Repositories tests/AjaiaDocs.UnitTests/Application tests/AjaiaDocs.IntegrationTests/Infrastructure/DocumentShareRepositoryTests.cs
git commit -m "feat(sharing): grant access and import text documents"
```

---

### Task 6: Cookie Session, Antiforgery, HTTP Modules, and API Journey

**Files:**

- Create: `src/AjaiaDocs.Api/Common/CurrentActor.cs`
- Create: `src/AjaiaDocs.Api/Contracts/ApiRequests.cs`
- Create: `src/AjaiaDocs.Api/Common/ProblemResponse.cs`
- Create: `src/AjaiaDocs.Api/Common/ResultHttpMapper.cs`
- Create: `src/AjaiaDocs.Api/Middleware/GlobalExceptionHandler.cs`
- Create: `src/AjaiaDocs.Api/Security/AntiforgeryEndpointFilter.cs`
- Create: `src/AjaiaDocs.Api/Modules/Session/SessionModule.cs`
- Create: `src/AjaiaDocs.Api/Modules/Documents/DocumentModule.cs`
- Create: `src/AjaiaDocs.Api/Modules/Sharing/SharingModule.cs`
- Create: `src/AjaiaDocs.Api/Modules/Health/HealthModule.cs`
- Modify: `src/AjaiaDocs.Api/Program.cs`
- Create: `src/AjaiaDocs.Api/appsettings.json`
- Create: `src/AjaiaDocs.Api/appsettings.Development.json`
- Create: `tests/AjaiaDocs.IntegrationTests/Api/AjaiaDocsWebApplicationFactory.cs`
- Create: `tests/AjaiaDocs.IntegrationTests/Api/AntiforgeryClient.cs`
- Create: `tests/AjaiaDocs.IntegrationTests/Api/SessionEndpointsTests.cs`
- Create: `tests/AjaiaDocs.IntegrationTests/Api/DocumentJourneyTests.cs`
- Create: `tests/AjaiaDocs.IntegrationTests/Api/ImportEndpointsTests.cs`

**Interfaces:**

- Exposes every route exactly as specified in the design document.
- Produces frontend JSON using camelCase and errors shaped as `{ code, detail, errors? }` with `application/problem+json`.

```csharp
public sealed record StartSessionRequest(Guid UserId);
public sealed record CreateDocumentRequest(string? Title);
public sealed record UpdateDocumentContentRequest(string ContentFormat,
    string Content, string PlainText, int ExpectedVersion);
public sealed record RenameDocumentRequest(string Title, int ExpectedVersion);
public sealed record GrantShareRequest(Guid UserId);
public sealed record ProblemResponse(string Code, string Detail,
    IReadOnlyDictionary<string, string[]>? Errors = null);
```

- [ ] **Step 1: Write failing session/security/journey tests**

```csharp
[Fact]
public async Task Owner_can_share_and_collaborator_can_edit_but_not_rename()
{
    var owner = await _factory.CreateAuthenticatedClientAsync(DemoUsers.AminaId);
    var created = await owner.PostJsonWithAntiforgeryAsync<DocumentDto>(
        "/api/documents", new { title = "Launch brief" });
    await owner.PostJsonWithAntiforgeryAsync<DocumentShareDto>(
        $"/api/documents/{created.Id}/shares", new { userId = DemoUsers.ChidiId });

    var collaborator = await _factory.CreateAuthenticatedClientAsync(DemoUsers.ChidiId);
    var shared = await collaborator.GetFromJsonAsync<List<DocumentListItemDto>>(
        "/api/documents?scope=shared");
    Assert.Contains(shared!, item => item.Id == created.Id && !item.IsOwner);

    var edited = await collaborator.PutJsonWithAntiforgeryAsync<DocumentDto>(
        $"/api/documents/{created.Id}/content",
        new { contentFormat = "lexical", content = DocumentContentDefaults.EmptyLexical,
            plainText = "Edited", expectedVersion = created.Version });
    Assert.Equal(created.Version + 1, edited.Version);

    var rename = await collaborator.PutWithAntiforgeryAsync(
        $"/api/documents/{created.Id}/title",
        new { title = "Forbidden", expectedVersion = edited.Version });
    Assert.Equal(HttpStatusCode.Forbidden, rename.StatusCode);
    Assert.Equal("owner_required", (await rename.Content.ReadFromJsonAsync<ProblemResponse>())!.Code);
}
```

Add tests for anonymous `401`, invalid/missing antiforgery rejection, unknown document `404`, stale `409`, session logout, `.txt`/`.md`, extension/size/UTF-8 errors, and content persistence before import response.

- [ ] **Step 2: Run API tests and verify RED**

```bash
dotnet test tests/AjaiaDocs.IntegrationTests/AjaiaDocs.IntegrationTests.csproj --filter "FullyQualifiedName~Api"
```

Expected: HTTP modules and test host are missing.

- [ ] **Step 3: Implement cookie and antiforgery configuration**

```csharp
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "AjaiaDocs.Session";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = builder.Environment.IsDevelopment()
            ? CookieSecurePolicy.SameAsRequest
            : CookieSecurePolicy.Always;
        options.ExpireTimeSpan = TimeSpan.FromHours(8);
        options.SlidingExpiration = true;
        options.Events.OnRedirectToLogin = context =>
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        };
    });

builder.Services.AddAntiforgery(options =>
{
    options.HeaderName = "X-XSRF-TOKEN";
    options.Cookie.Name = "AjaiaDocs.Antiforgery";
    options.Cookie.SameSite = SameSiteMode.Strict;
});
```

`GET /api/session/antiforgery` calls `IAntiforgery.GetAndStoreTokens` and returns `{ token }`. POST/PUT/DELETE routes validate through `AntiforgeryEndpointFilter`; the login POST also requires a token.

- [ ] **Step 4: Implement thin Carter modules and stable mappings**

Map validation to `400`, unauthenticated to `401`, `owner_required` to `403`, `not_found` to `404`, `conflict`/`duplicate_share` to `409`, and unexpected failure to `500`. Limit multipart body and copied bytes to 1 MiB before invoking `ImportDocumentHandler`.

Map the SPA only after `/api` and `/health`:

```csharp
app.MapCarter();
app.MapFallbackToFile("index.html");

public partial class Program;
```

- [ ] **Step 5: Run API integration tests and verify GREEN**

```bash
dotnet test tests/AjaiaDocs.IntegrationTests/AjaiaDocs.IntegrationTests.csproj --filter "FullyQualifiedName~Api"
```

Expected: the full owner-to-collaborator journey passes against real PostgreSQL.

- [ ] **Step 6: Run complete backend verification**

```bash
dotnet restore AjaiaDocs.sln
dotnet build AjaiaDocs.sln --configuration Release --no-restore
dotnet test AjaiaDocs.sln --configuration Release --no-build
```

Expected: build exit 0, zero failed tests, and zero warnings.

- [ ] **Step 7: Commit the HTTP surface**

```bash
git add src/AjaiaDocs.Api tests/AjaiaDocs.IntegrationTests/Api
git commit -m "feat(api): expose secure document collaboration routes"
```

---

### Task 7: React Foundation, Design Tokens, Demo Login, and Route Guard

**Files:**

- Create: `package.json`
- Create: `pnpm-workspace.yaml`
- Create: `web/package.json`
- Create: `web/vite.config.ts`
- Create: `web/tsconfig.json`
- Create: `web/tsconfig.app.json`
- Create: `web/tailwind.config.ts`
- Create: `web/postcss.config.js`
- Create: `web/eslint.config.js`
- Create: `web/src/test-setup.ts`
- Create: `web/src/index.css`
- Create: `web/src/main.tsx`
- Create: `web/src/App.tsx`
- Create: `web/src/app/store.ts`
- Create: `web/src/app/hooks.ts`
- Create: `web/src/app/router.tsx`
- Create: `web/src/test/renderWithApp.tsx`
- Create: `web/src/mocks/fixtures.ts`
- Create: `web/src/mocks/handlers.ts`
- Create: `web/src/mocks/server.ts`
- Create: `web/src/types/api.ts`
- Create: `web/src/store/api/ajaiaApi.ts`
- Create: `web/src/features/auth/sessionSlice.ts`
- Create: `web/src/features/auth/demoUsers.ts`
- Create: `web/src/features/auth/DemoLoginPage.tsx`
- Create: `web/src/features/auth/RequireSession.tsx`
- Create: `web/src/components/layout/AppShell.tsx`
- Create: `web/src/components/ui/Button.tsx`
- Create: `web/src/components/ui/Card.tsx`
- Create: `web/src/components/ui/OwnershipEdge.tsx`
- Test: `web/src/features/auth/DemoLoginPage.test.tsx`
- Test: `web/src/features/auth/RequireSession.test.tsx`
- Test: `web/src/components/layout/AppShell.test.tsx`

**Interfaces:**

```ts
export type User = {
  id: string;
  displayName: string;
  email: string;
  avatarColor: string;
};

export type ProblemDetails = {
  code?: string;
  detail: string;
  errors?: Record<string, string[]>;
};

export const demoUsers: User[] = [
  { id: "00000000-0000-0000-0000-000000000001", displayName: "Amina Okafor",
    email: "amina@example.test", avatarColor: "#365CF5" },
  { id: "00000000-0000-0000-0000-000000000002", displayName: "Chidi Okeke",
    email: "chidi@example.test", avatarColor: "#25A77A" },
  { id: "00000000-0000-0000-0000-000000000003", displayName: "Tayo Bello",
    email: "tayo@example.test", avatarColor: "#C77A15" },
];

export type RenderWithAppOptions = {
  initialEntry?: string;
  routePath?: string;
  extraRoutes?: React.ReactNode;
};

export function renderWithApp(
  ui: React.ReactElement,
  options?: RenderWithAppOptions,
): ReturnType<typeof render>;
```

- [ ] **Step 1: Generate behavior-free Vite/package scaffolding**

```bash
corepack enable
pnpm create vite web --template react-ts
pnpm --dir web add react@18.3.1 react-dom@18.3.1 react-router-dom@6.30.1 @reduxjs/toolkit@2.11.2 react-redux@9.2.0 clsx@2.1.1 tailwind-merge@3.5.0 lucide-react@0.539.0 sonner@2.0.7 @fontsource-variable/manrope @fontsource-variable/literata
pnpm --dir web add -D vite@7.3.5 typescript@5.9.3 vitest@3.2.4 jsdom@29.1.1 @testing-library/react@16.3.2 @testing-library/jest-dom@6.9.1 @testing-library/user-event@14.6.1 msw @vitejs/plugin-react-swc@4.3.1 tailwindcss@3.4.17 postcss@8.5.18 autoprefixer@10.4.27 eslint@9.17.0 prettier@3.8.1
```

Root `package.json` is private, pins `pnpm@10.33.4`, and forwards `test`, `typecheck`, `lint`, `build`, and `test:e2e` to `web`. Vite proxies `/api` and `/health` to `http://localhost:5080` during development.

- [ ] **Step 2: Write failing login and guard tests**

```tsx
it("creates a reviewer session and enters the document library", async () => {
  const user = userEvent.setup();
  renderWithApp(<DemoLoginPage />, {
    initialEntry: "/login",
    routePath: "/login",
    extraRoutes: <Route path="/documents" element={<h1>Your documents</h1>} />,
  });

  expect(screen.getByText("Demo access for reviewers")).toBeInTheDocument();
  await user.click(screen.getByRole("button", { name: /continue as amina okafor/i }));

  expect(await screen.findByRole("heading", { name: /your documents/i })).toBeInTheDocument();
});
```

Guard tests assert skeleton while loading, redirect on `401`, and `AppShell` for a valid session.

- [ ] **Step 3: Run tests and verify RED**

```bash
pnpm --dir web test -- DemoLoginPage.test.tsx RequireSession.test.tsx AppShell.test.tsx
```

Expected: feature components and API store do not exist.

- [ ] **Step 4: Implement tokens, RTK Query, routes, and auth UI**

Define only semantic variables:

```css
:root {
  --midnight-ink: #17233c;
  --cool-paper: #f7f9fc;
  --action-cobalt: #365cf5;
  --shared-mint: #25a77a;
  --warning-amber: #c77a15;
  --mist-border: #dce3ef;
  --surface: #ffffff;
  --danger: #b42318;
}
```

Configure `fetchBaseQuery({ baseUrl: "/api", credentials: "include", timeout: 30000 })`. `getAntiforgery` stores the token in `sessionSlice`; `prepareHeaders` adds `X-XSRF-TOKEN` for mutations. Routes are `/login`, `/documents`, `/documents/new`, and `/documents/:documentId`.

`DemoLoginPage` renders the exact `demoUsers` constants while the API independently verifies the selected ID exists with `is_seeded = true`. `renderWithApp` constructs a real Redux store and `MemoryRouter`; MSW starts from `test-setup.ts` with `onUnhandledRequest: "error"`. Fixtures mirror the complete API DTOs. `AppShell` includes a **Switch user** action that ends the cookie session and returns to `/login`.

- [ ] **Step 5: Run auth tests and verify GREEN**

```bash
pnpm --dir web test -- DemoLoginPage.test.tsx RequireSession.test.tsx AppShell.test.tsx
```

Expected: tests pass with no unhandled MSW requests.

- [ ] **Step 6: Commit frontend foundation**

```bash
git add package.json pnpm-workspace.yaml pnpm-lock.yaml web
git commit -m "feat(web): add reviewer login and application shell"
```

---

### Task 8: Document Library, Creation, and Import Experience

**Files:**

- Create: `web/src/features/documents/DocumentLibraryPage.tsx`
- Create: `web/src/features/documents/DocumentCard.tsx`
- Create: `web/src/features/documents/ImportDocumentDialog.tsx`
- Create: `web/src/features/documents/DeleteDocumentDialog.tsx`
- Create: `web/src/features/documents/documentValidation.ts`
- Create: `web/src/components/ui/Dialog.tsx`
- Create: `web/src/components/ui/EmptyState.tsx`
- Create: `web/src/components/ui/Skeleton.tsx`
- Modify: `web/src/store/api/ajaiaApi.ts`
- Modify: `web/src/app/router.tsx`
- Test: `web/src/features/documents/DocumentLibraryPage.test.tsx`
- Test: `web/src/features/documents/DocumentCard.test.tsx`
- Test: `web/src/features/documents/ImportDocumentDialog.test.tsx`
- Test: `web/src/features/documents/documentValidation.test.ts`

**Interfaces:**

```ts
export type DocumentSummary = {
  id: string;
  ownerId: string;
  owner: User;
  title: string;
  plainText: string;
  contentFormat: "lexical" | "markdown" | "plainText";
  version: number;
  updatedAt: string;
  isOwner: boolean;
};

export type DocumentDetail = DocumentSummary & {
  content: string;
  createdAt: string;
  canEdit: boolean;
  canRename: boolean;
  canShare: boolean;
  canDelete: boolean;
};

export type DocumentCardProps = {
  document: DocumentSummary;
  onOpen: (id: string) => void;
};
```

- [ ] **Step 1: Write failing library/import tests**

```tsx
it("labels owned and shared cards in text and color-independent markup", async () => {
  renderWithApp(<DocumentLibraryPage />);
  expect(await screen.findByText("Owned")).toBeInTheDocument();
  await userEvent.click(screen.getByRole("tab", { name: "Shared with me" }));
  expect(await screen.findByText("Shared")).toBeInTheDocument();
});

it("rejects files larger than one MiB before upload", async () => {
  const file = new File([new Uint8Array(1024 * 1024 + 1)], "large.md",
    { type: "text/markdown" });
  expect(validateImportFile(file)).toEqual(
    "Choose a .txt or .md file no larger than 1 MB.");
});
```

Add valid `.txt`/`.md`, uppercase extension, empty file, unsupported extension, structured server error, create-and-navigate, loading, empty, and retry states.

- [ ] **Step 2: Run tests and verify RED**

```bash
pnpm --dir web test -- DocumentLibraryPage.test.tsx DocumentCard.test.tsx ImportDocumentDialog.test.tsx documentValidation.test.ts
```

Expected: library/import components are missing.

- [ ] **Step 3: Implement RTK endpoints and library UI**

Add `getDocuments`, `createDocument`, `importDocument`, and `deleteDocument` with `Documents`/`Document` tags. Use cobalt ownership edge plus `Owned`, mint edge plus `Shared`, filename/type/size guidance beside the file input, and active `All`, `Owned by me`, `Shared with me` tabs.

- [ ] **Step 4: Run tests and verify GREEN**

```bash
pnpm --dir web test -- DocumentLibraryPage.test.tsx DocumentCard.test.tsx ImportDocumentDialog.test.tsx documentValidation.test.ts
```

Expected: all library/import flows pass and every failure offers a next action.

- [ ] **Step 5: Commit library/import UI**

```bash
git add web/src/features/documents web/src/components/ui web/src/store/api/ajaiaApi.ts web/src/app/router.tsx
git commit -m "feat(web): create and import documents from the library"
```

---

### Task 9: Lexical Serialization, Editor, and Formatting Toolbar

**Files:**

- Create: `web/src/features/editor/lexicalSerialization.ts`
- Create: `web/src/features/editor/LexicalDocumentEditor.tsx`
- Create: `web/src/features/editor/DocumentToolbar.tsx`
- Create: `web/src/features/editor/InitialContentPlugin.tsx`
- Create: `web/src/features/editor/SaveStatus.tsx`
- Create: `web/src/features/editor/test/renderEditor.tsx`
- Test: `web/src/features/editor/lexicalSerialization.test.tsx`
- Test: `web/src/features/editor/LexicalDocumentEditor.test.tsx`
- Test: `web/src/features/editor/DocumentToolbar.test.tsx`
- Test: `web/src/features/editor/SaveStatus.test.tsx`
- Modify: `web/package.json`

**Interfaces:**

```ts
export type EditorChange = { content: string; plainText: string };

export type LexicalDocumentEditorProps = {
  initialContent: string;
  contentFormat: "lexical" | "markdown" | "plainText";
  onChange: (change: EditorChange) => void;
};
```

- [ ] **Step 1: Install editor dependencies**

```bash
pnpm --dir web add lexical@0.41.0 @lexical/react@0.41.0 @lexical/rich-text@0.41.0 @lexical/list@0.41.0 @lexical/markdown@0.41.0 @lexical/selection@0.41.0 @lexical/utils@0.41.0
```

- [ ] **Step 2: Write failing serialization and toolbar tests**

```tsx
it("preserves underline, heading, and numbered-list structure after reload", async () => {
  const first = renderEditor("", "plainText");
  await first.user.click(screen.getByRole("button", { name: "Heading 1" }));
  await first.user.click(screen.getByRole("button", { name: "Underline" }));
  await first.user.click(screen.getByRole("button", { name: "Numbered list" }));
  await first.user.type(screen.getByRole("textbox", { name: "Document content" }),
    "Release plan");
  const serialized = first.latestChange().content;
  first.unmount();

  renderEditor(serialized, "lexical");
  expect(screen.getByRole("heading", { level: 1, name: "Release plan" }))
    .toBeInTheDocument();
  expect(screen.getByRole("list").tagName).toBe("OL");
  expect(screen.getByText("Release plan")).toHaveStyle("text-decoration: underline");
});
```

Add plain text import, Markdown heading/list import, no initial false-dirty event, bold/italic/underline active state, H1/H2, bullets/numbers, undo/redo, accessible names, and visible focus class assertions.

- [ ] **Step 3: Run editor tests and verify RED**

```bash
pnpm --dir web test -- lexicalSerialization.test.tsx LexicalDocumentEditor.test.tsx DocumentToolbar.test.tsx SaveStatus.test.tsx
```

Expected: editor components and serialization utilities are missing.

- [ ] **Step 4: Implement Lexical editor and toolbar**

Use `LexicalComposer`, `RichTextPlugin`, `ContentEditable`, `HistoryPlugin`, `ListPlugin`, and `OnChangePlugin`. Import Markdown with `$convertFromMarkdownString(markdown, TRANSFORMERS)`, split plain text into paragraphs, and parse lexical state with `editor.parseEditorState`. Serialize with `JSON.stringify(editorState.toJSON())` and extract plain text inside `editorState.read(() => $getRoot().getTextContent())`.

`renderEditor(initialContent, contentFormat)` is a test-only harness that renders the real editor, returns the Testing Library `user`, exposes the last real `onChange` payload through `latestChange()`, and returns `unmount`. It never mocks Lexical.

Use `FORMAT_TEXT_COMMAND` for bold/italic/underline, `$setBlocksType` with `$createHeadingNode` for H1/H2, list insert/remove commands, and undo/redo commands. Each toggle has `aria-label`, `aria-pressed`, tooltip text, and `focus-visible:ring-2`.

- [ ] **Step 5: Run editor tests and verify GREEN**

```bash
pnpm --dir web test -- lexicalSerialization.test.tsx LexicalDocumentEditor.test.tsx DocumentToolbar.test.tsx SaveStatus.test.tsx
```

Expected: required formatting survives serialization and imports open as editable content.

- [ ] **Step 6: Commit the editor surface**

```bash
git add web/package.json pnpm-lock.yaml web/src/features/editor
git commit -m "feat(editor): preserve rich document formatting"
```

---

### Task 10: Serialized Autosave and Editor Page Integration

**Files:**

- Create: `web/src/features/editor/saveCoordinator.ts`
- Create: `web/src/features/editor/DocumentEditorPage.tsx`
- Create: `web/src/features/editor/useUnsavedChangesWarning.ts`
- Create: `web/src/test/deferred.ts`
- Modify: `web/src/features/editor/SaveStatus.tsx`
- Modify: `web/src/store/api/ajaiaApi.ts`
- Modify: `web/src/app/router.tsx`
- Test: `web/src/features/editor/saveCoordinator.test.ts`
- Test: `web/src/features/editor/DocumentEditorPage.test.tsx`
- Test: `web/src/features/editor/useUnsavedChangesWarning.test.ts`

**Interfaces:**

```ts
export type SaveState = "saved" | "saving" | "changes-not-saved" | "conflict";

export type SaveIntent =
  | { kind: "content"; content: string; plainText: string; contentFormat: "lexical" }
  | { kind: "title"; title: string };

export type SaveResponse = { version: number; updatedAt: string };

export type DocumentSaveCoordinatorOptions = {
  initialVersion: number;
  debounceMs?: number;
  save: (intent: SaveIntent & { expectedVersion: number }) => Promise<SaveResponse>;
  onStateChange: (state: SaveState) => void;
  onVersionChange: (response: SaveResponse) => void;
  onConflict: (error: ProblemDetails) => void;
};

export class DocumentSaveCoordinator {
  constructor(options: DocumentSaveCoordinatorOptions);
  setContent(content: string, plainText: string): void;
  setTitle(title: string): void;
  flush(): Promise<void>;
  retry(): void;
  dispose(): void;
  getState(): SaveState;
  hasUnsavedChanges(): boolean;
}

export function deferred<T>() {
  let resolve!: (value: T) => void;
  let reject!: (reason?: unknown) => void;
  const promise = new Promise<T>((res, rej) => {
    resolve = res;
    reject = rej;
  });
  return { promise, resolve, reject };
}
```

- [ ] **Step 1: Write failing coordinator tests with fake timers**

```ts
it("serializes title and content writes and advances only from acknowledgements", async () => {
  const first = deferred<SaveResponse>();
  const second = deferred<SaveResponse>();
  const save = vi.fn()
    .mockReturnValueOnce(first.promise)
    .mockReturnValueOnce(second.promise);
  const coordinator = new DocumentSaveCoordinator({
    initialVersion: 3,
    debounceMs: 700,
    save,
    onStateChange: vi.fn(),
    onVersionChange: vi.fn(),
    onConflict: vi.fn(),
  });

  coordinator.setContent("one", "one");
  await vi.advanceTimersByTimeAsync(700);
  coordinator.setTitle("Latest title");
  expect(save).toHaveBeenCalledTimes(1);

  first.resolve({ version: 4, updatedAt: "2026-08-15T12:00:00Z" });
  await first.promise;
  expect(save).toHaveBeenLastCalledWith(expect.objectContaining({
    kind: "title", expectedVersion: 4,
  }));

  second.resolve({ version: 5, updatedAt: "2026-08-15T12:00:01Z" });
  await second.promise;
  expect(coordinator.getState()).toBe("saved");
});
```

Add debounce, content coalescing, network retry, `409` pause/local preservation, disposal, `flush`, and beforeunload tests.

- [ ] **Step 2: Run coordinator tests and verify RED**

```bash
pnpm --dir web test -- saveCoordinator.test.ts DocumentEditorPage.test.tsx useUnsavedChangesWarning.test.ts
```

Expected: coordinator and editor page are missing.

- [ ] **Step 3: Implement coordinator and editor integration**

Use a 700 ms default, one in-flight promise, separate latest pending title/content slots, and one monotonically updated acknowledged version. Coalesce repeated writes of the same kind; alternate pending title and content so neither starves. On network error retain pending state and expose `retry`. On a problem with `code === "conflict"`, retain local content, block retries, and expose `Reload saved version`.

The page loads `DocumentDto`, hides owner-only actions for collaborators, saves title on blur/Enter, queues editor changes, and refetches/reinitializes only when the user chooses reload after conflict.

- [ ] **Step 4: Run save/editor tests and verify GREEN**

```bash
pnpm --dir web test -- saveCoordinator.test.ts DocumentEditorPage.test.tsx useUnsavedChangesWarning.test.ts
```

Expected: title/content never race, failure does not clear dirty state, and collaborator editing remains enabled.

- [ ] **Step 5: Commit autosave/editor integration**

```bash
git add web/src/features/editor web/src/store/api/ajaiaApi.ts web/src/app/router.tsx
git commit -m "feat(editor): autosave edits without silent overwrite"
```

---

### Task 11: Sharing UI, Owner Actions, and Responsive Accessibility

**Files:**

- Create: `web/src/features/sharing/ShareDocumentDialog.tsx`
- Create: `web/src/features/sharing/sharePresentation.ts`
- Create: `web/src/components/layout/MobileNavDrawer.tsx`
- Modify: `web/src/features/editor/DocumentEditorPage.tsx`
- Modify: `web/src/features/documents/DeleteDocumentDialog.tsx`
- Modify: `web/src/components/layout/AppShell.tsx`
- Modify: `web/src/index.css`
- Modify: `web/src/store/api/ajaiaApi.ts`
- Test: `web/src/features/sharing/ShareDocumentDialog.test.tsx`
- Test: `web/src/features/sharing/sharePresentation.test.ts`
- Test: `web/src/features/editor/OwnerActions.test.tsx`
- Test: `web/src/components/layout/MobileNavDrawer.test.tsx`

**Interfaces:**

```ts
export type ShareDocumentDialogProps = {
  documentId: string;
  open: boolean;
  onOpenChange: (open: boolean) => void;
};

export type DocumentShare = {
  documentId: string;
  userId: string;
  displayName: string;
  email: string;
  avatarColor: string;
  createdAt: string;
};
```

- [ ] **Step 1: Install Radix dialog and write failing tests**

```bash
pnpm --dir web add @radix-ui/react-dialog@1.1.15 @radix-ui/react-alert-dialog@1.1.14
```

```tsx
it("grants and revokes a seeded collaborator with focus restoration", async () => {
  const user = userEvent.setup();
  renderWithApp(<ShareDocumentDialog documentId="doc-1" open onOpenChange={vi.fn()} />);
  await user.click(await screen.findByRole("button", { name: /share with chidi/i }));
  expect(await screen.findByText("Chidi Okeke has access")).toBeInTheDocument();
  await user.click(screen.getByRole("button", { name: /remove chidi okeke/i }));
  expect(await screen.findByText("Access removed")).toBeInTheDocument();
});
```

Add duplicate/self/unknown errors, collaborator owner-control absence, focus trap/restore, Escape, mobile drawer, text labels beside ownership colors, reduced motion, and no horizontal overflow unit checks.

- [ ] **Step 2: Run tests and verify RED**

```bash
pnpm --dir web test -- ShareDocumentDialog.test.tsx sharePresentation.test.ts OwnerActions.test.tsx MobileNavDrawer.test.tsx
```

Expected: sharing and responsive components are missing.

- [ ] **Step 3: Implement share endpoints and accessible UI**

Add `getShareCandidates`, `getShares`, `grantShare`, and `revokeShare` with `Shares`, `ShareCandidates`, and `Documents` invalidation. Use Radix focus management, explicit success/error copy, semantic ownership edges, toolbar wrapping, a 44 px mobile hit target, and `@media (prefers-reduced-motion: reduce)`.

- [ ] **Step 4: Run all frontend tests and checks**

```bash
pnpm --dir web test
pnpm --dir web typecheck
pnpm --dir web lint
pnpm --dir web build
```

Expected: all commands exit 0 with no warnings or unhandled requests.

- [ ] **Step 5: Commit sharing and polish**

```bash
git add web/src/features/sharing web/src/features/editor/DocumentEditorPage.tsx web/src/features/documents/DeleteDocumentDialog.tsx web/src/components/layout web/src/index.css web/src/store/api/ajaiaApi.ts web/package.json pnpm-lock.yaml
git commit -m "feat(web): complete accessible sharing and responsive flows"
```

---

### Task 12: Docker, Chrome Journey, Screenshots, and CI

**Files:**

- Create: `Dockerfile`
- Create: `.dockerignore`
- Create: `docker-compose.yml`
- Create: `render.yaml`
- Create: `web/playwright.config.ts`
- Create: `web/e2e/ajaia-docs.spec.ts`
- Create: `web/e2e/visual.spec.ts`
- Create: `web/e2e/fixtures/reviewer-brief.md`
- Create: `.github/workflows/ci.yml`
- Create after Chrome capture: `docs/screenshots/desktop-library.png`
- Create after Chrome capture: `docs/screenshots/desktop-editor.png`
- Create after Chrome capture: `docs/screenshots/mobile-library.png`
- Create after Chrome capture: `docs/screenshots/mobile-editor.png`

**Interfaces:**

- Produces: one container on port `8080`, Postgres on local port `54329`, `/health`, and screenshots used by submission docs.
- E2E base URL: `E2E_BASE_URL` or `http://127.0.0.1:8080`.

```ts
import { defineConfig, devices } from "@playwright/test";

export default defineConfig({
  testDir: "./e2e",
  use: { baseURL: process.env.E2E_BASE_URL ?? "http://127.0.0.1:8080" },
  projects: [{
    name: "chrome",
    use: { ...devices["Desktop Chrome"], channel: "chrome" },
  }],
});
```

- [ ] **Step 1: Write the Chrome journey and import fixture**

```ts
test("owner creates, formats, shares, and collaborator safely edits", async ({ page }) => {
  await page.goto("/login");
  await page.getByRole("button", { name: /continue as amina okafor/i }).click();
  await page.getByRole("button", { name: /new document/i }).click();
  await page.getByRole("textbox", { name: /document title/i }).fill("Launch brief");
  await page.getByRole("textbox", { name: /document content/i }).fill("Release plan");
  await page.getByRole("button", { name: /bold/i }).click();
  await expect(page.getByText("Saved")).toBeVisible();
  await page.reload();
  await expect(page.getByText("Release plan")).toBeVisible();
  await page.getByRole("button", { name: /^share$/i }).click();
  await page.getByRole("button", { name: /share with chidi/i }).click();
  await page.getByRole("button", { name: /switch user/i }).click();
  await page.getByRole("button", { name: /continue as chidi okeke/i }).click();
  await page.getByRole("tab", { name: /shared with me/i }).click();
  await page.getByRole("link", { name: /launch brief/i }).click();
  await expect(page.getByRole("button", { name: /^share$/i })).toHaveCount(0);
});
```

Extend the journey to bold, italic, underline, H1/H2, bullets, numbers, collaborator edit, refresh persistence, console-error failure, and owner-control absence.

Add a second Chrome test that uploads `web/e2e/fixtures/reviewer-brief.md`, asserts the filename-derived title and imported heading/list content, waits for a normalized `Saved` state after an edit, refreshes, and proves the normalized formatting persists.

- [ ] **Step 2: Install the Google Chrome browser used by the release journey**

```bash
pnpm --dir web exec playwright install --with-deps chrome
```

Expected: Playwright reports the installed Chrome channel without changing product behavior.

- [ ] **Step 3: Implement the deployment files**

Use `node:22-alpine`, `mcr.microsoft.com/dotnet/sdk:10.0`, and `mcr.microsoft.com/dotnet/aspnet:10.0` stages. Copy `web/dist` to `/app/publish/wwwroot` after `dotnet publish`, then copy the complete publish directory into the runtime work directory. Run as a non-root user and listen on Render's `PORT` through Program configuration.

`docker-compose.yml` uses PostgreSQL 16, database/user `ajaia_docs`, development password `ajaia_docs_dev`, port `54329`, a health check, and an API dependency on healthy Postgres.

`render.yaml` declares one free Docker web service, one free PostgreSQL database, `ConnectionStrings__Postgres` from the database, `ASPNETCORE_ENVIRONMENT=Production`, and `/health`.

- [ ] **Step 4: Start the full stack and run Chrome GREEN**

```bash
docker compose up -d --build
curl --fail http://127.0.0.1:8080/health
pnpm --dir web test:e2e --project=chrome
```

Expected: the product journey passes in the installed Google Chrome channel.

- [ ] **Step 5: Capture and inspect visual evidence**

```bash
pnpm --dir web exec playwright test e2e/visual.spec.ts --project=chrome
```

`visual.spec.ts` captures 1440×1000 library/editor and 390×844 library/editor images into `docs/screenshots`. Inspect all four images, rerun after any visual fix, assert `document.documentElement.scrollWidth <= window.innerWidth`, verify visible keyboard focus, and fail the test on browser console errors.

- [ ] **Step 6: Add and run CI**

CI runs:

```bash
dotnet restore AjaiaDocs.sln
dotnet build AjaiaDocs.sln --configuration Release --no-restore
dotnet test AjaiaDocs.sln --configuration Release --no-build
pnpm install --frozen-lockfile
pnpm --dir web test
pnpm --dir web typecheck
pnpm --dir web lint
pnpm --dir web build
docker compose up -d --build
pnpm --dir web exec playwright install --with-deps chrome
pnpm --dir web test:e2e --project=chrome
```

Retain Playwright traces and screenshots on failure.

- [ ] **Step 7: Commit deployment and evidence**

```bash
git add Dockerfile .dockerignore docker-compose.yml render.yaml web/playwright.config.ts web/e2e .github/workflows/ci.yml docs/screenshots
git commit -m "test: verify the complete collaboration journey in Chrome"
```

---

### Task 13: Documentation, Live Deployment, Review, and Submission Package

**Files:**

- Create: `README.md`
- Create: `ARCHITECTURE.md`
- Create: `AI_WORKFLOW.md`
- Create: `SUBMISSION.md`
- Create: `WALKTHROUGH_SCRIPT.md`
- Create only after the candidate supplies the final URL: `WALKTHROUGH_VIDEO_URL.txt`
- Create after final commit: `/Users/user/RiderProjects/vicomeg/ajaia-docs-submission.zip`

**Interfaces:**

- Consumes: observed commands, actual test counts, actual Render URL, actual screenshots, and actual limitation state.
- Produces: evaluation-ready written artifacts and the human recording/upload handoff.

- [ ] **Step 1: Run the complete local release gate**

```bash
dotnet build AjaiaDocs.sln --configuration Release
dotnet test AjaiaDocs.sln --configuration Release --no-build
pnpm --dir web test
pnpm --dir web typecheck
pnpm --dir web lint
pnpm --dir web build
docker compose up -d --build
curl --fail http://127.0.0.1:8080/health
pnpm --dir web test:e2e --project=chrome
git diff --check
git status --short
```

Keep the fresh command outputs available for the documentation step; do not summarize them from memory.

- [ ] **Step 2: Request code review and resolve findings**

Run a requirements review against the spec, then a code-quality/security/accessibility review. Every accepted bug receives a failing regression test before its fix. Rerun the affected suite and the complete release gate after the final fix.

- [ ] **Step 3: Deploy the Render Blueprint and capture the observed URL**

Connect `https://github.com/vicosoft4real/ajaia-docs` to Render, apply `render.yaml`, wait for `/health`, and capture the exact public URL as `AJAIA_DEPLOY_URL`. If Render authorization is unavailable in the agent environment, pause only this deployment substep and request the user's dashboard authorization; local verification continues.

Run the same Chrome journey against the deployment:

```bash
E2E_BASE_URL="$AJAIA_DEPLOY_URL" pnpm --dir web test:e2e --project=chrome
```

- [ ] **Step 4: Write documentation using only observed facts**

`README.md` must include prerequisites, `docker compose up --build`, separate developer commands, all test commands, the three seeded identities, `.txt`/`.md` and size limits, the observed live URL, Render cold-start/database-expiry disclosure, and the cookie re-login caveat.

`ARCHITECTURE.md` must explain layer direction, one-origin deployment, schema, auth/data flow, expected-version SQL, scope cuts, and next work.

`AI_WORKFLOW.md` must name Codex, Superpowers, frontend-design, parallel agents, the work AI accelerated, the generated recommendations changed or rejected, and the exact automated/Chrome verification performed.

`SUBMISSION.md` must enumerate source, documentation, screenshots, the observed live URL, demo identities, working behavior, incomplete human handoffs, exact verified test counts, and the next 2–4 hours. It must never claim an unobserved result.

`WALKTHROUGH_SCRIPT.md` must fit 3–5 minutes and cover login, create/import, formatting, save/refresh, share/switch/edit, scope cuts, architecture, and AI verification.

- [ ] **Step 5: Commit, push, open the PR, and merge only after checks pass**

```bash
git add README.md ARCHITECTURE.md AI_WORKFLOW.md SUBMISSION.md WALKTHROUGH_SCRIPT.md
git commit -m "docs: prepare the Ajaia Docs reviewer submission"
git push origin HEAD
gh pr create --title "Build Ajaia Docs collaborative editor" --body "Closes #1"
gh pr checks --watch
gh pr merge --squash --delete-branch
```

The PR body closes issue #1 only when GitHub merges the implementation. Do not close the issue manually before the merge.

- [ ] **Step 6: Package the merged source**

```bash
git fetch origin main
git archive --format=zip --output=/Users/user/RiderProjects/vicomeg/ajaia-docs-submission.zip origin/main
```

Verify the archive contains README, architecture/AI/submission notes, source, migrations, tests, Docker/Render files, and screenshots.

- [ ] **Step 7: Complete the human-owned handoff**

The candidate records the prepared walkthrough and supplies the unlisted Loom/YouTube URL. Create `WALKTHROUGH_VIDEO_URL.txt` with that URL as its only line, commit it, push it, and rebuild the archive from the resulting merged commit. The candidate uploads `ajaia-docs-submission.zip`, screenshots, and the video-link file to one Google Drive folder.

---

## Plan Self-Review Checklist

- [x] Every acceptance criterion in the spec maps to Tasks 1–13.
- [x] Every custom production behavior has an explicit failing-test step before implementation.
- [x] Repository, DTO, route, and frontend type names are consistent across tasks.
- [x] Owner/collaborator capabilities are tested in Core, SQL, HTTP, UI, and Chrome.
- [x] Import limits and UTF-8 behavior are tested at parser, HTTP, UI, and Chrome boundaries.
- [x] Title/content writes share one acknowledged version from API through save coordinator.
- [x] No step claims deployment, test, screenshot, or video success before evidence exists.
- [x] The tracking issue remains open through implementation and closes through the merged PR.
