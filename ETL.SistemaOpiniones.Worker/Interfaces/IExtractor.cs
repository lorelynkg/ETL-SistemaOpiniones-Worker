using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ETL.SistemaOpiniones.Worker.Models;

namespace ETL.SistemaOpiniones.Worker.Interfaces
{
    public interface IExtractor
    {
        string NombreFuente { get; }

        Task<List<OpinionExtraida>> ExtractAsync(
            CancellationToken cancellationToken);
    }
}