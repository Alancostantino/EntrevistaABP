using System;
using System.Collections.Generic;
using WB.EntrevistaABP.Domain.Shared.Enums;
namespace WB.EntrevistaABP.Application.Contracts.Dtos
{
    public class ViajeDto
    {
        public Guid Id { get; set; }  // Id del viaje
        public DateTime FechaSalida { get; set; }
        public DateTime FechaLlegada { get; set; }
        public string Origen { get; set; } = "";
        public string Destino { get; set; } = "";
        public MedioDeTransporte MedioDeTransporte { get; set; }

        public PasajeroDto Coordinador { get; set; } = new PasajeroDto();     // Siempre habrá un coordinador
        public List<PasajeroDto> Pasajeros { get; set; } = new(); // Nombre completo del coordinador
    }
}