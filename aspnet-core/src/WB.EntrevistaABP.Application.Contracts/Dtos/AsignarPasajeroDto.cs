using System;

namespace WB.EntrevistaABP.Application.Contracts.Dtos
{
    public class AsignarPasajeroDto
    {
        public Guid? PasajeroId { get; set; }

        public PasajeroDto? PasajeroNuevo { get; set; }
    }
}