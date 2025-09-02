
using System;
using System.Threading.Tasks;
using Volo.Abp.Application.Dtos;
using WB.EntrevistaABP.Application.Contracts.Dtos;

namespace WB.EntrevistaABP.Application.Contracts.Interfaces
{
    public interface IViajeServicio
    {
        Task<ViajeDto> CrearAsync(CrearViajeDto input);
        Task<PagedResultDto<ViajeDto>> GetListAsync(GetViajesDto input);
        Task DeleteAsync(Guid id);
        Task<ViajeDto> UpdateAsync(UpdateViajeDto input);
    }
}