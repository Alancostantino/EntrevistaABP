
using System.Threading.Tasks;
using WB.EntrevistaABP.Application.Contracts.Dtos;

namespace WB.EntrevistaABP.Application.Contracts.Interfaces
{
    public interface IViajeServicio
    {
        Task<ViajeDto> CrearAsync(CrearViajeDto input);
    }
}