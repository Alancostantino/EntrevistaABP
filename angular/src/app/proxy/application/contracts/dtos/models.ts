import type { MedioDeTransporte } from '../../../domain/shared/enums/medio-de-transporte.enum';
import type { PagedAndSortedResultRequestDto } from '@abp/ng.core';

export interface AsignarPasajeroDto {
  dniExistente?: number;
  pasajeroNuevo: PasajeroDto;
}

export interface CambiarCoordinadorDto {
  dniExistente?: number;
  pasajeroNuevo: PasajeroDto;
}

export interface CrearViajeDto {
  fechaSalida: string;
  fechaLlegada: string;
  origen: string;
  destino: string;
  medioDeTransporte: MedioDeTransporte;
  coordinadorId?: string;
  coordinadorNuevo: PasajeroDto;
}

export interface GetViajesDto extends PagedAndSortedResultRequestDto {
  fechaSalidaDesde?: string;
  fechaSalidaHasta?: string;
}

export interface PasajeroDto {
  id?: string;
  nombre?: string;
  apellido?: string;
  dni: number;
  fechaNacimiento?: string;
}

export interface UpdateViajeDto {
  id: string;
  fechaSalida: string;
  fechaLlegada: string;
  origen: string;
  destino: string;
  medioDeTransporte: MedioDeTransporte;
}

export interface ViajeDto {
  id?: string;
  fechaSalida?: string;
  fechaLlegada?: string;
  origen?: string;
  destino?: string;
  medioDeTransporte: MedioDeTransporte;
  coordinador: PasajeroDto;
  pasajeros: PasajeroDto[];
}
