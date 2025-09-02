using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Volo.Abp;
using Volo.Abp.Application.Dtos;
using Volo.Abp.Application.Services;
using Volo.Abp.Domain.Repositories;
using Volo.Abp.Guids;
using Volo.Abp.Identity;
using WB.EntrevistaABP.Application.Contracts.Dtos;
using WB.EntrevistaABP.Application.Contracts.Interfaces;
using WB.EntrevistaABP.Domain.Entidades;
using WB.EntrevistaABP.Permissions;
using System.Linq.Dynamic.Core;
using Volo.Abp.Domain.Entities;

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
                //Coordinador existente por Id
                coordinador = await _pasajeroRepo.GetAsync(input.CoordinadorId!.Value);
            }
            else
            {
                // Coordinador nuevo 
                var nuevo = input.CoordinadorNuevo!;

                // Si ya hay un Dni igual.
                coordinador = await _pasajeroRepo.FirstOrDefaultAsync(p => p.DNI == nuevo.DNI);

                if (coordinador == null)
                {
                    //¿Ya existe IdentityUser con userName = DNI?
                    var userName = nuevo.DNI.ToString();
                    var user = await _userManager.FindByNameAsync(userName);
                    if (user == null)
                    {
                        var email = $"{userName}@demo.local";
                        user = new IdentityUser(_guid.Create(), userName, email, CurrentTenant.Id);

                        (await _userManager.CreateAsync(user, "1q2w3E*")).CheckErrors();
                        (await _userManager.AddToRoleAsync(user, "client")).CheckErrors();//Asigna el rol al usuario
                    }

                    //  Crear PASAJERO vinculado al user
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

            // CREAR EL VIAJE

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

            await _viajeRepo.InsertAsync(viaje, autoSave: true);


            // MAPEAR A DTO Y DEVOLVER

            return ObjectMapper.Map<Viaje, ViajeDto>(viaje);
        }


        public async Task<PagedResultDto<ViajeDto>> GetListAsync(GetViajesDto input)
        {
            var query = await _viajeRepo.GetQueryableAsync();

            // Filtro por rango de fecha de salida
            query = query
                .WhereIf(input.FechaSalidaDesde.HasValue, v => v.FechaSalida >= input.FechaSalidaDesde!.Value)
                .WhereIf(input.FechaSalidaHasta.HasValue, v => v.FechaSalida <= input.FechaSalidaHasta!.Value);

            // Si es "client", solo viajes donde él es pasajero
            if (!CurrentUser.IsInRole("admin") && CurrentUser.IsInRole("client") && CurrentUser.Id.HasValue)
            {
                var userId = CurrentUser.Id.Value;
                query = query.Where(v => v.Pasajeros.Any(p => p.UserId == userId));
            }

            // Total antes de paginar
            var total = await AsyncExecuter.CountAsync(query);

            // Orden 
            var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "FechaSalida DESC" : input.Sorting;

            var pageQuery = query.OrderBy(sorting).PageBy(input.SkipCount, input.MaxResultCount);

            // Paginación + orden
            var list = await AsyncExecuter.ToListAsync(pageQuery);

            // Map a DTO
            var items = list.Select(v => ObjectMapper.Map<Viaje, ViajeDto>(v)).ToList();

            return new PagedResultDto<ViajeDto>(total, items);
        }


        [Authorize(EntrevistaABPPermissions.Viajes.Delete)]
        public async Task DeleteAsync(Guid id)
        {
            //  Verificar existencia
            var viaje = await _viajeRepo.FindAsync(id);
            if (viaje == null)
                throw new EntityNotFoundException(typeof(Viaje), id);

            //  Nos fijamos si teiene pasajeros 

            var tienePasajeros = await AsyncExecuter.AnyAsync(
                (await _viajeRepo.GetQueryableAsync())
                    .Where(v => v.Id == id)
                    .SelectMany(v => v.Pasajeros)
            );

            if (tienePasajeros)
                throw new BusinessException("Viaje.NoSePuedeEliminarConPasajeros")
                      .WithData("Reason", "El viaje tiene pasajeros asignados. Quite los pasajeros antes de eliminar.");

            // 3) Borrar 
            await _viajeRepo.DeleteAsync(id);
        }

        [Authorize(EntrevistaABPPermissions.Viajes.Update)] 
        public async Task<ViajeDto> UpdateAsync(UpdateViajeDto input)
        {
            // Validar entradas  
            input.Origen = input.Origen?.Trim() ?? "";
            input.Destino = input.Destino?.Trim() ?? "";

            if (input.FechaLlegada <= input.FechaSalida)
                throw new BusinessException("FechaLlegadaDebeSerMayorQueSalida");

            if (string.Equals(input.Origen, input.Destino, StringComparison.OrdinalIgnoreCase))
                throw new BusinessException("OrigenYDestinoNoPuedenSerIguales");

            //  Traer el viaje
            var viaje = await _viajeRepo.FindAsync(input.Id);
            if (viaje == null)
                throw new EntityNotFoundException(typeof(Viaje), input.Id);

            // Aplicar cambios
            viaje.FechaSalida = input.FechaSalida;
            viaje.FechaLlegada = input.FechaLlegada;
            viaje.Origen = input.Origen;
            viaje.Destino = input.Destino;
            viaje.MedioDeTransporte = input.MedioDeTransporte;

            await _viajeRepo.UpdateAsync(viaje);

        
            return ObjectMapper.Map<Viaje, ViajeDto>(viaje);
        }


    }
}