using CsvHelper;
using CsvHelper.Configuration;
using ETL.SistemaOpiniones.Worker.Configuration;
using ETL.SistemaOpiniones.Worker.Interfaces;
using ETL.SistemaOpiniones.Worker.Models;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Text;

namespace ETL.SistemaOpiniones.Worker.Extractors
{
    public class CsvExtractor : IExtractor
    {
        private readonly CsvOptions _options;
        private readonly ILogger<CsvExtractor> _logger;

        public string NombreFuente => "Encuestas internas CSV";

        public CsvExtractor(
            IOptions<ExtractionOptions> options,
            ILogger<CsvExtractor> logger)
        {
            _options = options.Value.Csv;
            _logger = logger;
        }

        public async Task<List<OpinionExtraida>> ExtractAsync(
            CancellationToken cancellationToken)
        {
            var resultados = new List<OpinionExtraida>();

            if (!File.Exists(_options.FilePath))
            {
                throw new FileNotFoundException(
                    "No se encontró el archivo CSV.",
                    _options.FilePath);
            }

            using var reader = new StreamReader(_options.FilePath);

            var config = new CsvConfiguration(
                CultureInfo.InvariantCulture)
            {
                Delimiter = _options.Delimiter,
                PrepareHeaderForMatch = args =>
                    Normalizar(args.Header)
            };

            using var csv = new CsvReader(reader, config);

            var registros = csv.GetRecords<dynamic>();

            foreach (var registro in registros)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var campos = (IDictionary<string, object>)registro;

                var opinion = new OpinionExtraida
                {
                    IdOpinionOrigen = ObtenerTexto(
                        campos, "IdOpinion"),

                    IdCliente = ObtenerTexto(
                        campos, "IdCliente"),

                    IdProducto = ObtenerTexto(
                        campos, "IdProducto"),

                    Fecha = ObtenerFecha(
                        campos, "Fecha"),

                    Comentario = ObtenerTexto(
                        campos, "Comentario"),

                    Clasificacion = ObtenerTexto(
                        campos, "Clasificacion"),

                    PuntajeSatisfaccion = ObtenerDecimal(
                        campos, "PuntajeSatisfaccion"),

                    TipoFuente = ObtenerTexto(
                        campos, "Fuente")
                };

                resultados.Add(opinion);
            }

            _logger.LogInformation(
                "Se extrajeron {Cantidad} encuestas del CSV.",
                resultados.Count);

            await Task.CompletedTask;

            return resultados;
        }

        private static string Normalizar(string texto)
        {
            string normalizado = texto.Normalize(
                NormalizationForm.FormD);

            var resultado = new StringBuilder();

            foreach (char caracter in normalizado)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(caracter)
                    != UnicodeCategory.NonSpacingMark)
                {
                    resultado.Append(caracter);
                }
            }

            return resultado.ToString()
                .Normalize(NormalizationForm.FormC)
                .Trim()
                .ToLowerInvariant();
        }

        private static string ObtenerTexto(
            IDictionary<string, object> campos,
            string nombre)
        {
            var campo = campos.FirstOrDefault(
                x => Normalizar(x.Key) == Normalizar(nombre));

            return campo.Value?.ToString() ?? "";
        }

        private static decimal? ObtenerDecimal(
            IDictionary<string, object> campos,
            string nombre)
        {
            string valor = ObtenerTexto(campos, nombre);

            return decimal.TryParse(
                valor,
                NumberStyles.Any,
                CultureInfo.InvariantCulture,
                out decimal resultado)
                ? resultado
                : null;
        }

        private static DateTime ObtenerFecha(
            IDictionary<string, object> campos,
            string nombre)
        {
            string valor = ObtenerTexto(campos, nombre);

            if (DateTime.TryParse(
                valor,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out DateTime fecha))
            {
                return fecha;
            }

            throw new FormatException(
                $"Fecha inválida en el CSV: {valor}");
        }
    }
}