using System;
using Volo.Abp.Application.Dtos;
using WB.EntrevistaABP.Domain.Shared.Enums;

namespace WB.EntrevistaABP.Application.Contracts.Interfaces
{
    public class GetViajesDto : PagedAndSortedResultRequestDto
    {
        public DateTime? FechaSalidaDesde { get; set; }
        public DateTime? FechaSalidaHasta { get; set; }
     
    }
}