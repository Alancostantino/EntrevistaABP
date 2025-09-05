import type { PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface GetViajesDto extends PagedAndSortedResultRequestDto {
  fechaSalidaDesde?: string;
  fechaSalidaHasta?: string;
}
