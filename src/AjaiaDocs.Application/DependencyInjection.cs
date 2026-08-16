using AjaiaDocs.Application.Features.Documents.CreateDocument;
using AjaiaDocs.Application.Features.Documents.Delete;
using AjaiaDocs.Application.Features.Documents.GetDocument;
using AjaiaDocs.Application.Features.Documents.ListDocuments;
using AjaiaDocs.Application.Features.Documents.Rename;
using AjaiaDocs.Application.Features.Documents.UpdateContent;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AjaiaDocs.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddAjaiaDocsApplication(this IServiceCollection services)
    {
        services.TryAddSingleton(TimeProvider.System);
        services.AddScoped<IValidator<CreateDocumentCommand>, CreateDocumentValidator>();
        services.AddScoped<IValidator<UpdateDocumentContentCommand>,
            UpdateDocumentContentValidator>();
        services.AddScoped<IValidator<RenameDocumentCommand>, RenameDocumentValidator>();
        services.AddScoped<CreateDocumentHandler>();
        services.AddScoped<ListDocumentsHandler>();
        services.AddScoped<GetDocumentHandler>();
        services.AddScoped<UpdateDocumentContentHandler>();
        services.AddScoped<RenameDocumentHandler>();
        services.AddScoped<DeleteDocumentHandler>();

        return services;
    }
}
