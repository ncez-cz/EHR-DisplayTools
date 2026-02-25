using DisplayTool.DocSignAuthority.Service.Canonicalizers;
using DisplayTool.DocSignAuthority.Service.FhirManipulation;
using DisplayTool.DocSignAuthority.Service.Managers;
using DisplayTool.DocSignAuthority.Service.Models;
using DisplayTool.DocSignAuthority.Service.Profiles;
using Microsoft.OpenApi;
using Newtonsoft.Json.Converters;
using NLog.Extensions.Logging;

namespace DisplayTool.DocSignAuthority.Service;

internal class Program
{
    private static ConfigurationManager? m_config;

    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        m_config = builder.Configuration;

// Add services to the container.
        builder.Services.AddControllersWithViews().AddNewtonsoftJson(options =>
        {
            var converter = new StringEnumConverter();
            options.SerializerSettings.Converters.Add(converter);
        });
        ConfigureServices(builder.Services);
        Configure(m_config, builder.Services);

        var app = builder.Build();

// Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapControllerRoute(
            name: "default",
            pattern: "{controller=Signature}/{action=Index}/{id?}");

        app.UseSwagger();
        app.UseSwaggerUI(options => { options.SwaggerEndpoint("v1/swagger.json", "My API V1"); });

        app.Run();
    }

    private static void ConfigureServices(IServiceCollection services)
    {
        services.AddScoped<ICanonicalizer, FhirXmlDocumentCanonicalizer>();
        services.AddScoped<ICanonicalizer, FhirJsonDocumentCanonicalizer>();
        services.AddScoped<Canonicalizer>();
        services.AddScoped<IDocumentFormatManipulator, XmlDocumentFormatManipulator>();
        services.AddScoped<IDocumentFormatManipulator, JsonDocumentFormatManipulator>();
        services.AddScoped<DocumentFormatManipulator>();
        services.AddScoped<DocumentSignManager>();
        services.AddAutoMapper(cfg => { }, typeof(DocumentTypeProfile).Assembly);
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo { Title = "My API", Version = "v1" });
        });
        services.AddSwaggerGenNewtonsoftSupport();
        services.AddNLog();
    }

    private static void Configure(ConfigurationManager configuration, IServiceCollection services)
    {
        services.Configure<CertificateConfiguration>(configuration.GetSection("SignatureCertificate"));
        services.Configure<SigningConfiguration>(configuration.GetSection("Signing"));
    }
}