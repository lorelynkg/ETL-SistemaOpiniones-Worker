using ETL.SistemaOpiniones.Worker.Configuration;
using ETL.SistemaOpiniones.Worker.Interfaces;
using ETL.SistemaOpiniones.Worker.Models;
using Microsoft.Extensions.Options;
using System.Diagnostics;
using System.Text.Json;

namespace ETL.SistemaOpiniones.Worker
{
    public class Worker : BackgroundService
    {
        private readonly IEnumerable<IExtractor> _extractores;
        private readonly ILogger<Worker> _logger;
        private readonly ExtractionOptions _options;

        public Worker(
            IEnumerable<IExtractor> extractores,
            ILogger<Worker> logger,
            IOptions<ExtractionOptions> options)
        {
            _extractores = extractores;
            _logger = logger;
            _options = options.Value;
        }

        protected override async Task ExecuteAsync(
            CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "Iniciando proceso de extracción ETL.");

            Directory.CreateDirectory(_options.OutputDirectory);

            var tareas = _extractores.Select(
                extractor => EjecutarExtractorAsync(
                    extractor, stoppingToken));

            await Task.WhenAll(tareas);

            _logger.LogInformation(
                "Proceso de extracción ETL finalizado.");
        }

        private async Task EjecutarExtractorAsync(
            IExtractor extractor,
            CancellationToken cancellationToken)
        {
            var cronometro = Stopwatch.StartNew();

            try
            {
                _logger.LogInformation(
                    "Iniciando extracción: {Fuente}",
                    extractor.NombreFuente);

                List<OpinionExtraida> datos =
                    await extractor.ExtractAsync(cancellationToken);

                string nombreArchivo =
                    $"{DateTime.Now:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}.json";

                string ruta = Path.Combine(
                    _options.OutputDirectory,
                    nombreArchivo);

                string contenido = JsonSerializer.Serialize(
                    datos,
                    new JsonSerializerOptions
                    {
                        WriteIndented = true
                    });

                await File.WriteAllTextAsync(
                    ruta, contenido, cancellationToken);

                cronometro.Stop();

                _logger.LogInformation(
                    "Extracción completada: {Fuente}. " +
                    "Registros: {Cantidad}. Tiempo: {Tiempo} ms.",
                    extractor.NombreFuente,
                    datos.Count,
                    cronometro.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Error durante la extracción de {Fuente}.",
                    extractor.NombreFuente);
            }
        }
    }
}