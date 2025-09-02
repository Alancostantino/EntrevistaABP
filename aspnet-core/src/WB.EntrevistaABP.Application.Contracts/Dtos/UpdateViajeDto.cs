using System;
using WB.EntrevistaABP.Domain.Shared.Enums;

namespace WB.EntrevistaABP.Application.Contracts.Dtos
{
    public class UpdateViajeDto
    {
        public Guid Id { get; set; }
        public DateTime FechaSalida { get; set; }
        public DateTime FechaLlegada { get; set; }
        public string Origen { get; set; } = "";
        public string Destino { get; set; } = "";
        public MedioDeTransporte MedioDeTransporte { get; set; }
    }
}