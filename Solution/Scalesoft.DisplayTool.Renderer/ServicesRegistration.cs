using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Renderer.Clients.Converter;
using Scalesoft.DisplayTool.Renderer.Clients.FhirValidator;
using Scalesoft.DisplayTool.Renderer.DocumentRenderers;
using Scalesoft.DisplayTool.Renderer.DocumentRenderers.Tools;
using Scalesoft.DisplayTool.Renderer.ModelBasedValidationWSService;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Validators;
using Scalesoft.DisplayTool.Renderer.Validators.Cda;
using Scalesoft.DisplayTool.Renderer.Validators.Dasta;
using Scalesoft.DisplayTool.Renderer.Validators.Fhir;
using Scalesoft.DisplayTool.Renderer.Validators.Signature;
using Scalesoft.DisplayTool.Shared.Configuration;
using Scalesoft.DisplayTool.TermxTranslator;
using Scalesoft.EZCAII.Client;

namespace Scalesoft.DisplayTool.Renderer;

public static class ServicesRegistration
{
    public static IServiceProvider CreateServiceProvider(
        ILoggerFactory loggerFactory,
        PdfRendererOptions? pdfRendererOptions,
        ExternalServicesConfiguration externalServicesConfiguration
    )
    {
        var services = new ServiceCollection();

        services.AddSingleton(externalServicesConfiguration);


        services.TryAdd(ServiceDescriptor.Singleton(typeof(ILogger<>), typeof(Logger<>)));
        services.AddSingleton(loggerFactory);

        services.AddSingleton<DastaFhirDocumentConverterClient>();

        services.AddScoped<ISpecificDocumentRenderer, CdaDocumentRenderer>();
        services.AddScoped<ISpecificDocumentRenderer, FhirJsonDocumentRenderer>();
        services.AddScoped<ISpecificDocumentRenderer, FhirXmlDocumentRenderer>();
        services.AddScoped<ISpecificDocumentRenderer, PdfDocumentRenderer>();
        services.AddScoped<ISpecificDocumentRenderer, DastaDocumentRenderer>();
        services.AddScoped<FhirXmlDocumentRenderer>();

        services.AddSingleton<IWidgetRenderer, RazorWidgetRenderer>();

        services.AddScoped<Language>();

        services.RegisterTermxTranslator(externalServicesConfiguration.TranslationSource);
        // services.AddSingleton<ICodeTranslator, EpsosTranslator>();

        services.AddSingleton(pdfRendererOptions ?? new PdfRendererOptions());
        services.AddSingleton<HtmlToPdfConverter>();

        if (!string.IsNullOrEmpty(externalServicesConfiguration.DocumentValidation.CdaBaseUrl))
        {
            services.AddSingleton(
                new ModelBasedValidationWSClient(
                    ModelBasedValidationWSClient.EndpointConfiguration.ModelBasedValidationWSPort,
                    externalServicesConfiguration.DocumentValidation.CdaBaseUrl
                )
            );
            services.AddSingleton<IDocumentValidator, CdaExternalValidator>();
        }
        else
        {
            services.AddSingleton<IDocumentValidator, CdaInternalXmlValidator>();
        }

        if (!string.IsNullOrEmpty(externalServicesConfiguration.DocumentValidation.FhirBaseUrl))
        {
            services.AddHttpClient<FhirValidatorClient>(c =>
                {
                    c.BaseAddress = new Uri(externalServicesConfiguration.DocumentValidation.FhirBaseUrl);
                    c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
                }
            );

            services.AddSingleton<IDocumentValidator, FhirExternalJsonValidator>();
            services.AddSingleton<IDocumentValidator, FhirExternalXmlValidator>();
        }
        else
        {
            services.AddSingleton<IDocumentValidator, FhirInternalJsonValidator>();
            services.AddSingleton<IDocumentValidator, FhirInternalXmlValidator>();
        }

        // Only internal validator is available for dasta:
        services.AddSingleton<IDocumentValidator, DastaInternalXmlValidator>();

        services.AddSingleton<DocumentValidatorProvider>();

        services.AddHttpClient<DastaFhirDocumentConverterClient>(c =>
            {
                c.BaseAddress = new Uri(externalServicesConfiguration.DocumentConverter.BaseUrl);
                c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/xml"));
            }
        );

        if (!string.IsNullOrEmpty(externalServicesConfiguration.DocumentSignatureValidation.EZCAIIBaseUrl))
        {
            var baseUrl = externalServicesConfiguration.DocumentSignatureValidation.EZCAIIBaseUrl;
            if (!string.IsNullOrEmpty(baseUrl) && !baseUrl.EndsWith("/"))
            {
                baseUrl += '/';
            }

            services.AddHttpClient<SignDocumentClient>(c => { c.BaseAddress = new Uri(baseUrl); });
            services.AddHttpClient<ValidateDocumentClient>(c => { c.BaseAddress = new Uri(baseUrl); });
            services.AddScoped<IDocumentSignatureValidationManager, DocumentSignatureValidationManager>();
        }
        else
        {
            services.AddScoped<IDocumentSignatureValidationManager, NullDocumentSignatureValidationManager>();
        }

        return services.BuildServiceProvider();
    }
}