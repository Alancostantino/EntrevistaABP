using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Volo.Abp;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using WB.EntrevistaABP.Application.Contracts.Dtos;
using WB.EntrevistaABP.Application.Contracts.Interfaces;
using WB.EntrevistaABP.Domain.Entidades;
using WB.EntrevistaABP.Permissions;

namespace WB.EntrevistaABP.Application.Servicios
{
    [Authorize(EntrevistaABPPermissions.Viajes.Default)]
    public class ViajeService : ApplicationService, IViajeServicio
    {
        private readonly IRepository<Viaje, Guid> _viajeRepo;
        private readonly IRepository<Pasajero, Guid> _pasajeroRepo;
        private readonly IdentityUserManager _userManager;
        private readonly IGuidGenerator _guid;

        public ViajeService(
            IRepository<Viaje, Guid> viajeRepo,
            IRepository<Pasajero, Guid> pasajeroRepo,
            IdentityUserManager userManager,
            IGuidGenerator guid)
        {
            _viajeRepo = viajeRepo;
            _pasajeroRepo = pasajeroRepo;
            _userManager = userManager;
            _guid = guid;
        }

        [Authorize(EntrevistaABPPermissions.Viajes.Create)]
        public async Task<ViajeDto> CrearAsync(CrearViajeDto input)
        {

            // 1) VALIDACIONES BÁSICAS

            if (input.FechaLlegada <= input.FechaSalida)
                throw new BusinessException("FechaLlegadaDebeSerMayorQueSalida");

            if (string.Equals(input.Origen, input.Destino, StringComparison.OrdinalIgnoreCase))
                throw new BusinessException("OrigenYDestinoNoPuedenSerIguales");

            var traeId = input.CoordinadorId.HasValue;
            var traeNuevo = input.CoordinadorNuevo != null;
            if (traeId == traeNuevo) // ambos o ninguno
                throw new BusinessException("DebeIndicarCoordinadorExistenteOCoordinadorNuevo");


            // 2) RESOLVER COORDINADOR

            Pasajero coordinador;

            if (traeId)
            {
                // A) Coordinador existente por Id
                coordinador = await _pasajeroRepo.GetAsync(input.CoordinadorId!.Value);
            }
            else
            {
                // B) Coordinador nuevo (usás PasajeroDto como payload)
                var nuevo = input.CoordinadorNuevo!;

                // B.1) ¿Ya existe Pasajero por DNI?
                coordinador = await _pasajeroRepo.FirstOrDefaultAsync(p => p.DNI == nuevo.DNI);

                if (coordinador == null)
                {
                    // B.2) ¿Ya existe IdentityUser con userName = DNI?
                    var userName = nuevo.DNI.ToString();
                    var user = await _userManager.FindByNameAsync(userName);
                    if (user == null)
                    {
                        var email = $"{userName}@demo.local";
                        user = new IdentityUser(_guid.Create(), userName, email, CurrentTenant.Id);

                        (await _userManager.CreateAsync(user, "1q2w3E*")).CheckErrors();
                        (await _userManager.AddToRoleAsync(user, "client")).CheckErrors();//Asigna el rol al usuario
                    }

                    // B.3) Crear PASAJERO vinculado al user
                    coordinador = new Pasajero(_guid.Create()) // seteamos Id en el ctor
                    {
                        Nombre = nuevo.Nombre,
                        Apellido = nuevo.Apellido,
                        DNI = nuevo.DNI,
                        FechaNacimiento = nuevo.FechaNacimiento,
                        UserId = user.Id
                    };

                    await _pasajeroRepo.InsertAsync(coordinador, autoSave: true);
                }
                // Si ya existía pasajero por DNI, lo reutilizamos tal cual
            }

            // 3) CREAR EL VIAJE

            var viaje = new Viaje(_guid.Create()) // seteamos Id en el ctor
            {
                FechaSalida = input.FechaSalida,
                FechaLlegada = input.FechaLlegada,
                Origen = input.Origen,
                Destino = input.Destino,
                MedioDeTransporte = input.MedioDeTransporte,
                CoordinadorId = coordinador.Id,
                Coordinador = coordinador
            };

            // Regla del negocio: el coordinador también viaja
            if (!viaje.Pasajeros.Any(p => p.Id == coordinador.Id)) //EVITAR DUPLCIADOS.
                viaje.Pasajeros.Add(coordinador); // EF insertará en 'PasajerosViajes'

            await _viajeRepo.InsertAsync(viaje, autoSave: true);


            // 4) MAPEAR A DTO Y DEVOLVER

            return ObjectMapper.Map<Viaje, ViajeDto>(viaje);
        }
    }
}