using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ETL.SistemaOpiniones.Worker.Configuration
{
    public class ExtractionOptions
    {
        public string OutputDirectory { get; set; } = "staging";

        public CsvOptions Csv { get; set; } = new();

        public DatabaseOptions Database { get; set; } = new();

        public ApiOptions Api { get; set; } = new();
    }

    public class CsvOptions
    {
        public string FilePath { get; set; } = "datos/encuestas.csv";

        public string Delimiter { get; set; } = ",";
    }

    public class DatabaseOptions
    {
        public string ConnectionString { get; set; } = string.Empty;

        public string Query { get; set; } = string.Empty;
    }

    public class ApiOptions
    {
        public string BaseUrl { get; set; } = string.Empty;

        public string Endpoint { get; set; } = string.Empty;

        public int TimeoutSeconds { get; set; } = 30;

        public string Token { get; set; } = string.Empty;
    }
}