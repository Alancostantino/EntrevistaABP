using System;

namespace WB.EntrevistaABP.Application.Contracts.Dtos
{
    public class PasajeroDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Apellido { get; set; } = string.Empty;
        public int DNI { get; set; }
        public DateTime FechaNacimiento { get; set; }
    }
}