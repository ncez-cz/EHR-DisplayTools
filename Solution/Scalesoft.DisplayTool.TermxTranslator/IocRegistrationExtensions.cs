using System.Net.Http.Headers;
using Microsoft.Extensions.DependencyInjection;
using Scalesoft.DisplayTool.Shared.Configuration;
using Scalesoft.DisplayTool.Shared.Translation;

namespace Scalesoft.DisplayTool.TermxTranslator;

public static class IocRegistrationExtensions
{
    public static void RegisterTermxTranslator(this ServiceCollection services, TermxTranslatorConfiguration? configuration)
    {
        services.AddHttpClient<TermxApiClient>(c =>
        {
            c.BaseAddress = new Uri(configuration?.BaseUrl?? "https://termapitest.mzcr.cz/fhir/");
            c.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/fhir+xml"));
        });
        services.AddSingleton<ICodeTranslator, TermxCodeTranslator>();
    }
}