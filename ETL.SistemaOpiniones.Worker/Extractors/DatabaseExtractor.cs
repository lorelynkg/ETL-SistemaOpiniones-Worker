using ETL.SistemaOpiniones.Worker.Configuration;
using ETL.SistemaOpiniones.Worker.Interfaces;
using ETL.SistemaOpiniones.Worker.Models;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Options;

namespace ETL.SistemaOpiniones.Worker.Extractors
{
    public class DatabaseExtractor : IExtractor
    {
        private readonly DatabaseOptions _options;
        private readonly ILogger<DatabaseExtractor> _logger;

        public string NombreFuente => "Opiniones SQL Server";

        public DatabaseExtractor(
            IOptions<ExtractionOptions> options,
            ILogger<DatabaseExtractor> logger)
        {
            _options = options.Value.Database;
            _logger = logger;
        }

        public async Task<List<OpinionExtraida>> ExtractAsync(
            CancellationToken cancellationToken)
        {
            var resultados = new List<OpinionExtraida>();

            var connectionString =
     Environment.GetEnvironmentVariable("ConnectionStrings__SistemaOpiniones")
     ?? _options.ConnectionString;

            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new InvalidOperationException(
                    "No se configuró la cadena de conexión de SistemaOpiniones.");
            }

            await using var connection =
                new SqlConnection(connectionString);

            await connection.OpenAsync(cancellationToken);

            string query = @"
                SELECT
                    O.IdOpinion,
                    O.IdCliente,
                    O.IdProducto,
                    O.Fecha,
                    O.Comentario,
                    O.Clasificacion,
                    O.PuntajeSatisfaccion,
                    F.TipoFuente
                FROM Opiniones O
                INNER JOIN FuenteDatos F
                    ON O.IdFuente = F.IdFuente;";

            await using var command =
                new SqlCommand(query, connection);

            await using var reader =
                await command.ExecuteReaderAsync(cancellationToken);

            while (await reader.ReadAsync(cancellationToken))
            {
                var opinion = new OpinionExtraida
                {
                    IdOpinionOrigen =
                        reader["IdOpinion"].ToString(),

                    IdCliente =
                        reader["IdCliente"].ToString(),

                    IdProducto =
                        reader["IdProducto"].ToString(),

                    Fecha =
                        Convert.ToDateTime(reader["Fecha"]),

                    Comentario =
                        reader["Comentario"]?.ToString() ?? "",

                    Clasificacion =
                        reader["Clasificacion"]?.ToString(),

                    PuntajeSatisfaccion =
                        reader["PuntajeSatisfaccion"] == DBNull.Value
                        ? null
                        : Convert.ToDecimal(
                            reader["PuntajeSatisfaccion"]),

                    TipoFuente =
                        reader["TipoFuente"]?.ToString() ?? ""
                };

                resultados.Add(opinion);
            }

            _logger.LogInformation(
                "Se extrajeron {Cantidad} opiniones de SQL Server.",
                resultados.Count);

            return resultados;
        }
    }
}