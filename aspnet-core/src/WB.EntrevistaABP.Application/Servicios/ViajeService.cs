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
        public async Task<ViajeDto> CreateAsync(CrearViajeDto input)
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







        [Authorize(EntrevistaABPPermissions.Viajes.Default)]
        public async Task<PagedResultDto<ViajeDto>> GetListAsync(GetViajesDto input)
        {
            var query = (await _viajeRepo.WithDetailsAsync(
                v => v.Coordinador,
                v => v.Pasajeros));

            
            query = query
                .WhereIf(input.FechaSalidaDesde.HasValue, v => v.FechaSalida >= input.FechaSalidaDesde!.Value)
                .WhereIf(input.FechaSalidaHasta.HasValue, v => v.FechaSalida <= input.FechaSalidaHasta!.Value);

            // Si es "client", solo viajes donde él es pasajero
            if (!CurrentUser.IsInRole("admin") && CurrentUser.IsInRole("client") && CurrentUser.Id.HasValue)
            {
                var userId = CurrentUser.Id.Value;
                query = query.Where(v => v.Pasajeros.Any(p => p.UserId == userId));
            }

            //creada la query listamos y paginamos 

            
            var total = await AsyncExecuter.CountAsync(query);

           
            var sorting = string.IsNullOrWhiteSpace(input.Sorting) ? "FechaSalida DESC" : input.Sorting;

            var pageQuery = query.OrderBy(sorting).PageBy(input.SkipCount, input.MaxResultCount);

            // Paginación + orden
            var list = await AsyncExecuter.ToListAsync(pageQuery);

            
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
                throw new BusinessException("Viaje.NoSePuedeEliminarConPasajeros");


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






        [Authorize(EntrevistaABPPermissions.Viajes.ManagePassengers)]
        public async Task<ViajeDto> AsignarPasajeroAsync(Guid viajeId, AsignarPasajeroDto input)
        {
            // Validaciones
            var usaDni = input.DniExistente.HasValue;
            var usaNuevo = input.PasajeroNuevo != null;


            if ((usaDni && usaNuevo) || (!usaDni && !usaNuevo))
                throw new BusinessException("DebeIndicarPasajeroExistenteOPasajeroNuevo");

            // Traer el viaje con detalles 
            var qViajes = await _viajeRepo.WithDetailsAsync(v => v.Coordinador, v => v.Pasajeros);
            var viaje = await AsyncExecuter.FirstOrDefaultAsync(qViajes.Where(v => v.Id == viajeId));
            if (viaje == null)
                throw new EntityNotFoundException(typeof(Viaje), viajeId);

            //  Resolver pasajero (existente o crear)
            Pasajero pasajero;

            if (usaDni)
            {
                pasajero = await _pasajeroRepo.FirstOrDefaultAsync(p => p.DNI == input.DniExistente!.Value);
                if (pasajero == null)
                    throw new BusinessException("PasajeroNoEncontradoPorDni");
            }
            else
            {
                // Cargar nuevo (si no existe por DNI lo creamos)
                var nuevo = input.PasajeroNuevo!;
                // VERIFICAMOS SI EXISTE POR DNI
                pasajero = await _pasajeroRepo.FirstOrDefaultAsync(p => p.DNI == nuevo.DNI);
                if (pasajero == null)
                {
                    // IdentityUser con userName = DNI

                    var userName = nuevo.DNI.ToString();
                    var user = await _userManager.FindByNameAsync(userName);
                    if (user == null)
                    {
                        var email = $"{userName}@demo.local";
                        user = new IdentityUser(_guid.Create(), userName, email, CurrentTenant.Id);
                        (await _userManager.CreateAsync(user, "1q2w3E*")).CheckErrors();
                        (await _userManager.AddToRoleAsync(user, "client")).CheckErrors();
                    }

                    pasajero = new Pasajero(_guid.Create())
                    {
                        Nombre = nuevo.Nombre,
                        Apellido = nuevo.Apellido,
                        DNI = nuevo.DNI,
                        FechaNacimiento = nuevo.FechaNacimiento,
                        UserId = user.Id
                    };

                    await _pasajeroRepo.InsertAsync(pasajero, autoSave: true);
                }
            }

            // Validaciones de N:N
            if (viaje.Pasajeros.Any(p => p.Id == pasajero.Id))
                throw new BusinessException("PasajeroYaAsignado");

            if (viaje.CoordinadorId == pasajero.Id)
                throw new BusinessException("ElCoordinadorYaEstaAsignadoComoTal");

            // Asignar y guardar
            viaje.Pasajeros.Add(pasajero);
            await _viajeRepo.UpdateAsync(viaje, autoSave: true);

            // Devolver con detalles
            var qRefresco = await _viajeRepo.WithDetailsAsync(v => v.Coordinador, v => v.Pasajeros);
            var actualizado = await AsyncExecuter.FirstOrDefaultAsync(qRefresco.Where(v => v.Id == viajeId));
            return ObjectMapper.Map<Viaje, ViajeDto>(actualizado ?? viaje);
        }






        [Authorize(EntrevistaABPPermissions.Viajes.ManagePassengers)]
        public async Task<ViajeDto> CambiarCoordinadorAsync(Guid viajeId, CambiarCoordinadorDto input)
        {
            var usaDni = input.DniExistente.HasValue;
            var usaNuevo = input.PasajeroNuevo != null;
            if ((usaDni && usaNuevo) || (!usaDni && !usaNuevo))
                throw new BusinessException("DebeIndicarCoordinadorExistenteONuevo");

            var q = await _viajeRepo.WithDetailsAsync(v => v.Coordinador, v => v.Pasajeros);
            var viaje = await AsyncExecuter.FirstOrDefaultAsync(q.Where(v => v.Id == viajeId))
                        ?? throw new EntityNotFoundException(typeof(Viaje), viajeId);

            Pasajero nuevoCoor;

            if (usaDni)
            {
                nuevoCoor = await _pasajeroRepo.FirstOrDefaultAsync(p => p.DNI == input.DniExistente!.Value)
                             ?? throw new BusinessException("CoordinadorNoEncontradoPorDni");
            }
            else
            {
                var dto = input.PasajeroNuevo!;
                nuevoCoor = await _pasajeroRepo.FirstOrDefaultAsync(p => p.DNI == dto.DNI);
                if (nuevoCoor == null)
                {
                    var userName = dto.DNI.ToString();
                    var user = await _userManager.FindByNameAsync(userName);
                    if (user == null)
                    {
                        var email = $"{userName}@demo.local";
                        user = new IdentityUser(_guid.Create(), userName, email, CurrentTenant.Id);
                        (await _userManager.CreateAsync(user, "1q2w3E*")).CheckErrors();
                        (await _userManager.AddToRoleAsync(user, "client")).CheckErrors();
                    }

                    nuevoCoor = new Pasajero(_guid.Create())
                    {
                        Nombre = dto.Nombre,
                        Apellido = dto.Apellido,
                        DNI = dto.DNI,
                        FechaNacimiento = dto.FechaNacimiento,
                        UserId = user.Id
                    };
                    await _pasajeroRepo.InsertAsync(nuevoCoor, autoSave: true);
                }
            }

            var estaba = viaje.Pasajeros.FirstOrDefault(p => p.Id == nuevoCoor.Id);
            if (estaba != null) viaje.Pasajeros.Remove(estaba);

            viaje.CoordinadorId = nuevoCoor.Id;
            viaje.Coordinador = nuevoCoor;

            await _viajeRepo.UpdateAsync(viaje, autoSave: true);

            var qRefresco = await _viajeRepo.WithDetailsAsync(v => v.Coordinador, v => v.Pasajeros);
            var actualizado = await AsyncExecuter.FirstOrDefaultAsync(qRefresco.Where(v => v.Id == viajeId));
            return ObjectMapper.Map<Viaje, ViajeDto>(actualizado ?? viaje);
        }











    }
}