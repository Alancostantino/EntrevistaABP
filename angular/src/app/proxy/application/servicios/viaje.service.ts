import { RestService, Rest } from '@abp/ng.core';
import type { PagedResultDto } from '@abp/ng.core';
import { Injectable } from '@angular/core';
import type { AsignarPasajeroDto, CambiarCoordinadorDto, CrearViajeDto, UpdateViajeDto, ViajeDto } from '../contracts/dtos/models';
import type { GetViajesDto } from '../contracts/interfaces/models';

@Injectable({
  providedIn: 'root',
})
export class ViajeService {
  apiName = 'Default';
  

  asignarPasajero = (viajeId: string, input: AsignarPasajeroDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ViajeDto>({
      method: 'POST',
      url: `/api/app/viaje/asignar-pasajero/${viajeId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  cambiarCoordinador = (viajeId: string, input: CambiarCoordinadorDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ViajeDto>({
      method: 'POST',
      url: `/api/app/viaje/cambiar-coordinador/${viajeId}`,
      body: input,
    },
    { apiName: this.apiName,...config });
  

  create = (input: CrearViajeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ViajeDto>({
      method: 'POST',
      url: '/api/app/viaje',
      body: input,
    },
    { apiName: this.apiName,...config });
  

  delete = (id: string, config?: Partial<Rest.Config>) =>
    this.restService.request<any, void>({
      method: 'DELETE',
      url: `/api/app/viaje/${id}`,
    },
    { apiName: this.apiName,...config });
  

  getList = (input: GetViajesDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, PagedResultDto<ViajeDto>>({
      method: 'GET',
      url: '/api/app/viaje',
      params: { fechaSalidaDesde: input.fechaSalidaDesde, fechaSalidaHasta: input.fechaSalidaHasta, sorting: input.sorting, skipCount: input.skipCount, maxResultCount: input.maxResultCount },
    },
    { apiName: this.apiName,...config });
  

  update = (input: UpdateViajeDto, config?: Partial<Rest.Config>) =>
    this.restService.request<any, ViajeDto>({
      method: 'PUT',
      url: '/api/app/viaje',
      body: input,
    },
    { apiName: this.apiName,...config });

  constructor(private restService: RestService) {}
}
