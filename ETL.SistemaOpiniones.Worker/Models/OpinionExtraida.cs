namespace ETL.SistemaOpiniones.Worker.Models
{
    public class OpinionExtraida
    {
        public string? IdOpinionOrigen { get; set; }

        public string? IdCliente { get; set; }

        public string? IdProducto { get; set; }

        public DateTime Fecha { get; set; }

        public string Comentario { get; set; } = string.Empty;

        public string? Clasificacion { get; set; }

        public decimal? PuntajeSatisfaccion { get; set; }

        public string TipoFuente { get; set; } = string.Empty;

        public string? Canal { get; set; }
    }
}