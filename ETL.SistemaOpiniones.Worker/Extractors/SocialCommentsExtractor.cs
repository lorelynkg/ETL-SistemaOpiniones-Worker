using CsvHelper;
using CsvHelper.Configuration;
using ETL.SistemaOpiniones.Worker.Configuration;
using ETL.SistemaOpiniones.Worker.Interfaces;
using ETL.SistemaOpiniones.Worker.Models;
using Microsoft.Extensions.Options;
using System.Globalization;

namespace ETL.SistemaOpiniones.Worker.Extractors
{
    public class SocialCommentsExtractor : IExtractor
    {
        private readonly CsvOptions _options;
        private readonly ILogger<SocialCommentsExtractor> _logger;

        public string NombreFuente => "Comentarios de redes sociales CSV";

        public SocialCommentsExtractor(
            IOptions<ExtractionOptions> options,
            ILogger<SocialCommentsExtractor> logger)
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
                "social_comments.csv");

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

                var opinion = new OpinionExtraida
                {
                    IdOpinionOrigen = Obtener("IdComment"),
                    IdCliente = Obtener("IdCliente"),
                    IdProducto = Obtener("IdProducto"),
                    Fecha = fecha,
                    Comentario = Obtener("Comentario"),
                    TipoFuente = Obtener("Fuente"),
                    Canal = Obtener("Fuente")
                };

                resultados.Add(opinion);
            }

            _logger.LogInformation(
                "Se extrajeron {Cantidad} comentarios sociales.",
                resultados.Count);

            await Task.CompletedTask;

            return resultados;
        }
    }
}