using System;
using System.ComponentModel.DataAnnotations;
using WB.EntrevistaABP.Domain.Shared.Enums;
namespace WB.EntrevistaABP.Application.Contracts.Dtos

{
    public class CrearViajeDto
    {
        [Required] public DateTime FechaSalida { get; set; }
        [Required] public DateTime FechaLlegada { get; set; }

        [Required, StringLength(128)] public string Origen { get; set; } = string.Empty;
        [Required, StringLength(128)] public string Destino { get; set; } = string.Empty;

        [Required] public MedioDeTransporte MedioDeTransporte { get; set; }

        // Coordinador: o Id existente o datos para crearlo (PasajeroDto)
        public Guid? CoordinadorId { get; set; }
        public PasajeroDto? CoordinadorNuevo { get; set; }

    }
}