import { mapEnumToOptions } from '@abp/ng.core';

export enum MedioDeTransporte {
  Avion = 0,
  Tren = 1,
  Auto = 2,
  Autobus = 3,
}

export const medioDeTransporteOptions = mapEnumToOptions(MedioDeTransporte);
