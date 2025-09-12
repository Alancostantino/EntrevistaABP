using System;

namespace WB.EntrevistaABP.Application.Contracts.Dtos
{
    public class AsignarPasajeroDto
    {
        public int? DniExistente { get; set; }

        public PasajeroDto? PasajeroNuevo { get; set; }
    }
}