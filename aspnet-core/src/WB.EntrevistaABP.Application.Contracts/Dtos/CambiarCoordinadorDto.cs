using System;

namespace WB.EntrevistaABP.Application.Contracts.Dtos
{
    public class CambiarCoordinadorDto
    {
         public Guid? PasajeroId { get; set; }       
        public PasajeroDto? PasajeroNuevo { get; set; } 

    }
}