using System;

namespace WB.EntrevistaABP.Application.Contracts.Dtos
{
    public class CambiarCoordinadorDto
    {
         public int? DniExistente { get; set; }      
        public PasajeroDto? PasajeroNuevo { get; set; } 

    }
}