using System;
using System.Collections.Generic;
using Volo.Abp.Domain.Entities;
using WB.EntrevistaABP.Domain.Shared.Enums;

namespace WB.EntrevistaABP.Domain.Entidades
{
    public class Viaje : Entity<Guid>
    {
        protected Viaje() { }
        public Viaje(Guid id) : base(id) { }
        public DateTime FechaSalida { get; set; }
        public DateTime FechaLlegada { get; set; }
        public string Origen { get; set; } = "";
        public string Destino { get; set; } = "";
        public MedioDeTransporte MedioDeTransporte { get; set; } 

        //Relacion N:N con Pasajeros
        public ICollection<Pasajero> Pasajeros { get; set; } = new List<Pasajero>();

        // Coordinador especial 1:1
        public Guid CoordinadorId { get; set; }
        public Pasajero Coordinador { get; set; } = null!;
    }
}