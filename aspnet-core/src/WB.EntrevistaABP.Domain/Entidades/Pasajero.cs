using System;
using System.Collections.Generic;
using Volo.Abp.Identity;
using Volo.Abp.Domain.Entities;

namespace WB.EntrevistaABP.Domain.Entidades
{
    public class Pasajero : Entity<Guid>
    {
        protected Pasajero() { }
        public Pasajero(Guid id) : base(id) { }

        public string Nombre { get; set; } = "";
        public string Apellido { get; set; } = "";
        public int DNI { get; set; }
        public DateTime FechaNacimiento { get; set; }

        // Relacion 1:1 IdentityUser
        public Guid UserId { get; set; }
        public IdentityUser User { get; set; } = null!;

        //Relacion N:N con Viaje
        public ICollection<Viaje> Viajes { get; set; } = new List<Viaje>();

        

    }
}