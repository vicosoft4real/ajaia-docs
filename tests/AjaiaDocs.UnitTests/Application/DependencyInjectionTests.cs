using AjaiaDocs.Application;
using AjaiaDocs.Application.Common.Interfaces;
using AjaiaDocs.Application.Features.Documents.CreateDocument;
using AjaiaDocs.Application.Features.Documents.GetDocument;
using AjaiaDocs.Application.Features.Documents.ListDocuments;
using AjaiaDocs.Application.Features.Import;
using AjaiaDocs.Application.Features.Sharing.GetShareCandidates;
using AjaiaDocs.Application.Features.Sharing.GrantShare;
using AjaiaDocs.Application.Features.Sharing.ListShares;
using AjaiaDocs.Application.Features.Sharing.RevokeShare;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;

namespace AjaiaDocs.UnitTests.Application;

public sealed class DependencyInjectionTests
{
    [Fact]
    public void AddAjaiaDocsApplication_registers_document_handlers_validator_and_default_time_provider()
    {
        var services = new ServiceCollection();
        services.AddSingleton(Substitute.For<IDocumentRepository>());
        services.AddSingleton(Substitute.For<IDocumentShareRepository>());
        services.AddSingleton(Substitute.For<IUserRepository>());

        services.AddAjaiaDocsApplication();

        using var provider = services.BuildServiceProvider();
        Assert.IsType<CreateDocumentHandler>(provider.GetRequiredService<CreateDocumentHandler>());
        Assert.IsType<ListDocumentsHandler>(provider.GetRequiredService<ListDocumentsHandler>());
        Assert.IsType<GetDocumentHandler>(provider.GetRequiredService<GetDocumentHandler>());
        Assert.IsType<StrictTextImportParser>(
            provider.GetRequiredService<StrictTextImportParser>());
        Assert.IsType<ImportDocumentHandler>(provider.GetRequiredService<ImportDocumentHandler>());
        Assert.IsType<GetShareCandidatesHandler>(
            provider.GetRequiredService<GetShareCandidatesHandler>());
        Assert.IsType<ListDocumentSharesHandler>(
            provider.GetRequiredService<ListDocumentSharesHandler>());
        Assert.IsType<GrantDocumentShareHandler>(
            provider.GetRequiredService<GrantDocumentShareHandler>());
        Assert.IsType<RevokeDocumentShareHandler>(
            provider.GetRequiredService<RevokeDocumentShareHandler>());
        Assert.IsType<CreateDocumentValidator>(
            provider.GetRequiredService<IValidator<CreateDocumentCommand>>());
        Assert.Same(TimeProvider.System, provider.GetRequiredService<TimeProvider>());
    }

    [Fact]
    public void AddAjaiaDocsApplication_preserves_a_caller_provided_time_provider()
    {
        var timeProvider = new FakeTimeProvider();
        var services = new ServiceCollection();
        services.AddSingleton<TimeProvider>(timeProvider);

        services.AddAjaiaDocsApplication();

        using var provider = services.BuildServiceProvider();
        Assert.Same(timeProvider, provider.GetRequiredService<TimeProvider>());
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => DateTimeOffset.UnixEpoch;
    }
}
