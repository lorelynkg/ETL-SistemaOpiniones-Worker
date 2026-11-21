# ETL.SistemaOpiniones.Worker

Proyecto de extracción de datos ETL desarrollado con .NET 8 Worker Service.

## Objetivo

Extraer opiniones de clientes desde tres fuentes:

1. Archivos CSV: encuestas internas.
2. SQL Server: reseñas web.
3. API REST: comentarios de redes sociales.

Los datos extraídos se guardan temporalmente en archivos JSON dentro
de la carpeta staging.

## Tecnologías

- .NET 8
- Worker Service
- CsvHelper
- Microsoft.Data.SqlClient
- HttpClient
- ILogger
- JSON

## Arquitectura

El proyecto utiliza una arquitectura modular basada en interfaces.

Cada fuente tiene su propio extractor:

- CsvExtractor
- DatabaseExtractor
- ApiExtractor

Todos implementan la interfaz IExtractor.

El servicio Worker coordina la ejecución paralela de los extractores.

## Configuración

Los parámetros de extracción se encuentran en appsettings.json.

Se deben configurar:

- Ruta del archivo CSV.
- Cadena de conexión SQL Server.
- Consulta SQL.
- URL de la API.
- Endpoint de la API.
- Token de autenticación, si corresponde.

No se deben publicar contraseñas ni tokens reales en GitHub.

## Ejecución

Desde Visual Studio:

1. Compilar la solución.
2. Configurar las fuentes de datos.
3. Ejecutar el proyecto.

Los archivos JSON generados se almacenan en la carpeta staging.

## Nota

La consulta SQL, las columnas del CSV y la estructura JSON de la API
son plantillas. Deben ajustarse a las fuentes reales del proyecto.