using ETL.SistemaOpiniones.Worker.Configuration;
using ETL.SistemaOpiniones.Worker.Interfaces;
using ETL.SistemaOpiniones.Worker.Models;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;

namespace ETL.SistemaOpiniones.Worker.Extractors
{
    public class ApiExtractor : IExtractor
    {
        private readonly HttpClient _httpClient;
        private readonly ApiOptions _options;
        private readonly ILogger<ApiExtractor> _logger;

        public string NombreFuente => "API REST JSONPlaceholder";

        public ApiExtractor(
            HttpClient httpClient,
            IOptions<ExtractionOptions> options,
            ILogger<ApiExtractor> logger)
        {
            _httpClient = httpClient;
            _options = options.Value.Api;

            _logger = logger;

            _httpClient.BaseAddress = new Uri(_options.BaseUrl);

            _httpClient.Timeout =
                TimeSpan.FromSeconds(_options.TimeoutSeconds);
        }

        public async Task<List<OpinionExtraida>> ExtractAsync(
            CancellationToken cancellationToken)
        {
            var resultados = new List<OpinionExtraida>();

            var datos = await _httpClient.GetFromJsonAsync<List<ComentarioApi>>(
                _options.Endpoint,
                cancellationToken);

            if (datos == null)
                return resultados;

            foreach (var comentario in datos)
            {
                resultados.Add(new OpinionExtraida
                {
                    IdOpinionOrigen = comentario.Id.ToString(),

                    IdCliente = null,

                    IdProducto = null,

                    Fecha = DateTime.UtcNow,

                    Comentario = comentario.Body,

                    Clasificacion = comentario.Name,

                    PuntajeSatisfaccion = null,

                    TipoFuente = "APIRest",

                    Canal = "JSONPlaceholder"
                });
            }

            _logger.LogInformation(
                "Se extrajeron {Cantidad} comentarios desde la API REST.",
                resultados.Count);

            return resultados;
        }

        private class ComentarioApi
        {
            public int Id { get; set; }

            public int PostId { get; set; }

            public string Name { get; set; } = "";

            public string Email { get; set; } = "";

            public string Body { get; set; } = "";
        }
    }
}