import { Pipe, PipeTransform } from '@angular/core';

@Pipe({
  name: 'enumLabel'
})
export class EnumLabelPipe implements PipeTransform {

  transform(value: unknown, enumType?:any,  map?: Record<string, string>,
    fallback: string = '-'
  ): string {
    if (value === null || value === undefined) return fallback;

    // Si viene número y tengo enumType, resuelvo el nombre del enum
    let key = typeof value === 'number' && enumType && enumType[value] !== undefined
      ? String(enumType[value])
      : String(value);

    // Si hay mapa (acentos, etc), lo aplico
    if (map && map[key]) return map[key];

    // Fallback: formateo mínimamente el texto
    return key.replace(/_/g, ' ').replace(/([a-z])([A-Z])/g, '$1 $2') || fallback;  
     
     
  }

}
