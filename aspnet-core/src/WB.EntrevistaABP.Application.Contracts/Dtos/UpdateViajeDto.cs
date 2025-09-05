using System;
using System.ComponentModel.DataAnnotations;
using WB.EntrevistaABP.Domain.Shared.Enums;

namespace WB.EntrevistaABP.Application.Contracts.Dtos
{
    public class UpdateViajeDto
    {
        [Required] public Guid Id { get; set; }
        [Required] public DateTime FechaSalida { get; set; }
        [Required] public DateTime FechaLlegada { get; set; }
        [Required] public string Origen { get; set; } = "";
        [Required] public string Destino { get; set; } = "";
        [Required] public MedioDeTransporte MedioDeTransporte { get; set; }
    }
}