using ETL.SistemaOpiniones.Worker;
using ETL.SistemaOpiniones.Worker.Configuration;
using ETL.SistemaOpiniones.Worker.Extractors;
using ETL.SistemaOpiniones.Worker.Interfaces;

var builder = Host.CreateApplicationBuilder(args);

// Configuración general
builder.Services.Configure<ExtractionOptions>(
    builder.Configuration.GetSection("Extraction"));

// Extractor de encuestas CSV
builder.Services.AddTransient<IExtractor, CsvExtractor>();

// Extractor de opiniones SQL Server
builder.Services.AddTransient<IExtractor, DatabaseExtractor>();

// Extractor de comentarios de redes sociales CSV
builder.Services.AddTransient<IExtractor, SocialCommentsExtractor>();

// Extractor de reseñas web CSV
builder.Services.AddTransient<IExtractor, WebReviewsExtractor>();

// API REST: se mantiene registrado para su futura conexión
builder.Services.AddHttpClient<ApiExtractor>();
builder.Services.AddTransient<IExtractor>(serviceProvider =>
    serviceProvider.GetRequiredService<ApiExtractor>());

// Servicio principal
builder.Services.AddHostedService<Worker>();

var host = builder.Build();

host.Run();