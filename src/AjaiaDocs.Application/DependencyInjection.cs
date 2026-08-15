using AjaiaDocs.Application.Features.Documents.CreateDocument;
using AjaiaDocs.Application.Features.Documents.GetDocument;
using AjaiaDocs.Application.Features.Documents.ListDocuments;
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
        services.AddScoped<CreateDocumentHandler>();
        services.AddScoped<ListDocumentsHandler>();
        services.AddScoped<GetDocumentHandler>();

        return services;
    }
}
