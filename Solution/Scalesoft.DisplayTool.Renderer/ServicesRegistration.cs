using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Scalesoft.DisplayTool.Extensions.Localization.CdaCodeLists;
using Scalesoft.DisplayTool.Renderer.Clients.Converter;
using Scalesoft.DisplayTool.Renderer.Clients.FhirValidator;
using Scalesoft.DisplayTool.Renderer.DocumentRenderers;
using Scalesoft.DisplayTool.Renderer.DocumentRenderers.Tools;
using Scalesoft.DisplayTool.Renderer.ModelBasedValidationWSService;
using Scalesoft.DisplayTool.Renderer.Renderers;
using Scalesoft.DisplayTool.Renderer.UrlUtils;
using Scalesoft.DisplayTool.Renderer.Utils.Language;
using Scalesoft.DisplayTool.Renderer.Validators;
using Scalesoft.DisplayTool.Renderer.Validators.Cda;
using Scalesoft.DisplayTool.Renderer.Validators.Dasta;
using Scalesoft.DisplayTool.Renderer.Validators.Fhir;
using Scalesoft.DisplayTool.Renderer.Validators.Signature;
using Scalesoft.DisplayTool.Shared.Configuration;
using Scalesoft.DisplayTool.Shared.Translation;
using Scalesoft.EZCAII.Client;

namespace Scalesoft.DisplayTool.Renderer;

public static class ServicesRegistration
{
    public static IServiceProvider CreateServiceProvider(
        ILoggerFactory loggerFactory,
        PdfRendererOptions? pdfRendererOptions,
        ExternalServicesConfiguration externalServicesConfiguration,
        KnownOidsConfiguration? knownOidsConfiguration
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
        services.AddSingleton(knownOidsConfiguration ?? new KnownOidsConfiguration());

        // Termx translator is pretty slow, disabled for development purposes
        //services.RegisterTermxTranslator(externalServicesConfiguration.TranslationSource);
        services.AddSingleton<ICodeTranslator, EpsosTranslator>();

        // InMemoryTranslationsStorage can be used for better performance at the cost of higher ram usage 
        var translationsStorage = new LiteDbTranslationsStorage("translations.db");
        // var translationsStorage = new InMemoryTranslationsStorage();
        var parser = new EpsosParser(knownOidsConfiguration);
        parser.LoadIntoStorage(translationsStorage);

        services.AddSingleton<ITranslationsStorage>(translationsStorage);

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

        switch (externalServicesConfiguration.DocumentSignature.PdfSigningProvider)
        {
            case SignatureProvider.None:
            {
                services.AddScoped<IPdfSignatureManager, NullSignatureManager>();
            }
                break;
            case SignatureProvider.EZCAII:
            {
                if (externalServicesConfiguration.DocumentSignature.EZCAIIConfiguration == null)
                {
                    throw new InvalidOperationException(
                        "EZCAII signature provider was configured, but configuration is not defined");
                }

                var baseUrl =
                    UrlUtil.PreprocessBaseUrl(externalServicesConfiguration.DocumentSignature.EZCAIIConfiguration
                        .BaseUrl);

                services.AddHttpClient<SignDocumentClient>(c => { c.BaseAddress = new Uri(baseUrl); });
                services.AddScoped<IPdfSignatureManager, EZCIIPdfSignatureManager>();
            }
                break;
            case SignatureProvider.PoCSigningAuthority:
            case SignatureProvider.PoCSigningAuthorityEncapsulated:
            {
                throw new InvalidOperationException(
                    "PoC Signing Authority PDF signing provider was selected, this is unsupported.");
            }
            default:
                throw new ArgumentOutOfRangeException();
        }

        switch (externalServicesConfiguration.DocumentSignature.FhirDocumentProvider)
        {
            case SignatureProvider.None:
            {
                services.AddScoped<IFhirDocumentSignatureManager, NullSignatureManager>();
            }
                break;
            case SignatureProvider.EZCAII:
            {
                throw new InvalidOperationException(
                    "EZCAII FHIR document signing provider was selected, this is unsupported.");
            }
            case SignatureProvider.PoCSigningAuthority:
            case SignatureProvider.PoCSigningAuthorityEncapsulated:
            {
                if (externalServicesConfiguration.DocumentSignature.PoCSigningAuthorityConfiguration == null)
                {
                    throw new InvalidOperationException(
                        "PoC Signing Authority FHIR document signing provider was configured, but configuration is not defined");
                }

                var baseUrl = UrlUtil.PreprocessBaseUrl(externalServicesConfiguration.DocumentSignature
                    .PoCSigningAuthorityConfiguration.BaseUrl);

                services.AddHttpClient<DocSignAuthority.Client.Client>(c => { c.BaseAddress = new Uri(baseUrl); });
                if (externalServicesConfiguration.DocumentSignature.FhirDocumentProvider ==
                    SignatureProvider.PoCSigningAuthority)
                {
                    services
                        .AddScoped<IFhirDocumentSignatureManager,
                            PoCSigningAuthorityDocumentIntegratedSignatureManager>();
                }
                else
                {
                    services
                        .AddScoped<IFhirDocumentSignatureManager,
                            PoCSigningAuthorityDocumentEncapsulatedSignatureManager>();
                }
            }
                break;
            default:
                throw new InvalidOperationException(
                    $"Unknown signature validation provider: {externalServicesConfiguration.DocumentSignature.FhirDocumentProvider}");
        }

        return services.BuildServiceProvider();
    }
}