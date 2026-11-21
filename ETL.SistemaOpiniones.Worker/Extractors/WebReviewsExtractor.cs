using CsvHelper;
using ETL.SistemaOpiniones.Worker.Configuration;
using ETL.SistemaOpiniones.Worker.Interfaces;
using ETL.SistemaOpiniones.Worker.Models;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace ETL.SistemaOpiniones.Worker.Extractors
{
    public class WebReviewsExtractor : IExtractor
    {
        private readonly CsvOptions _options;
        private readonly ILogger<WebReviewsExtractor> _logger;

        public string NombreFuente => "Reseñas web CSV";

        public WebReviewsExtractor(
            IOptions<ExtractionOptions> options,
            ILogger<WebReviewsExtractor> logger)
        {
            _options = options.Value.Csv;
            _logger = logger;
        }

        public async Task<List<OpinionExtraida>> ExtractAsync(
            CancellationToken cancellationToken)
        {
            var resultados = new List<OpinionExtraida>();

            string ruta = Path.Combine(
                Path.GetDirectoryName(_options.FilePath) ?? "datos",
                "web_reviews.csv");

            using var reader = new StreamReader(ruta);

            using var csv = new CsvReader(
                reader,
                CultureInfo.InvariantCulture);

            var registros = csv.GetRecords<dynamic>();

            foreach (var registro in registros)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var campos = (IDictionary<string, object>)registro;

                string Obtener(string nombre)
                {
                    var campo = campos.FirstOrDefault(
                        x => x.Key.Equals(
                            nombre,
                            StringComparison.OrdinalIgnoreCase));

                    return campo.Value?.ToString() ?? "";
                }

                DateTime.TryParse(
                    Obtener("Fecha"),
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out DateTime fecha);

                decimal.TryParse(
                    Obtener("Rating"),
                    NumberStyles.Any,
                    CultureInfo.InvariantCulture,
                    out decimal rating);

                var opinion = new OpinionExtraida
                {
                    IdOpinionOrigen = Obtener("IdReview"),
                    IdCliente = Obtener("IdCliente"),
                    IdProducto = Obtener("IdProducto"),
                    Fecha = fecha,
                    Comentario = Obtener("Comentario"),
                    PuntajeSatisfaccion = rating,
                    TipoFuente = "Web",
                    Canal = "Web"
                };

                resultados.Add(opinion);
            }

            _logger.LogInformation(
                "Se extrajeron {Cantidad} reseñas web.",
                resultados.Count);

            await Task.CompletedTask;

            return resultados;
        }
    }
}