using System.Text.Json.Serialization;
using Scalesoft.DisplayTool.Renderer;
using Scalesoft.DisplayTool.Shared.Configuration;

var builder = WebApplication.CreateBuilder(args);


// Add services to the container.
builder.Services
    .AddControllers()
    .AddJsonOptions(options =>
        {
            var converter = new JsonStringEnumConverter();
            options.JsonSerializerOptions.Converters.Add(converter);
        }
    );

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<ExternalServicesConfiguration>(builder.Configuration.GetSection("ExternalServices"));

builder.Services.Configure<TranslatorConfiguration>(builder.Configuration.GetSection("Translator"));

builder.Services.AddSingleton(serviceProvider => new DocumentRenderer(
        new DocumentRendererOptions
        {
            LoggerFactory = serviceProvider.GetService<ILoggerFactory>(),
            PdfRenderer = builder.Configuration.GetSection("PdfRenderer").Get<PdfRendererOptions>(),
            ExternalServicesConfiguration =
                builder.Configuration.GetSection("ExternalServices").Get<ExternalServicesConfiguration>() ??
                throw new InvalidOperationException("Missing ExternalServices configuration section"),
            TranslatorConfiguration = builder.Configuration.GetSection("Translator").Get<TranslatorConfiguration>(),
        }
    )
);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseStatusCodePages(); // absolutely basic error pages

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.MapControllers();

app.Run();