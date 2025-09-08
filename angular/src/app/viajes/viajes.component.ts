import { Component, OnInit } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { FormBuilder, FormGroup } from '@angular/forms';
import { NgbDateAdapter, NgbDateNativeAdapter } from '@ng-bootstrap/ng-bootstrap';
import { MedioDeTransporte as MedioEnum } from '@proxy/domain/shared/enums/medio-de-transporte.enum';
// Importá del proxy generado por ABP (rutas reales según tu CLI)
import { ViajeService } from '@proxy/application/servicios';
import { ViajeDto, GetViajesDto } from '@proxy/application/contracts/dtos';

@Component({
  selector: 'app-viajes',
  templateUrl: './viajes.component.html',
  providers: [ListService, { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }],
})
export class ViajesComponent implements OnInit {
  readonly MedioEnum = MedioEnum;
  viajes = { items: [], totalCount: 0 } as PagedResultDto<ViajeDto>;

  filtroForm: FormGroup;
 private readonly asEnum: any = MedioEnum || {};;

  constructor(
    public readonly list: ListService,
    private viajeService: ViajeService,
    private fb: FormBuilder
  ) {}

  ngOnInit(): void {

    // Formulario de filtro (por ahora: rango de salida + sorting)

    this.filtroForm = this.fb.group({
      fechaDesde: [null],
      fechaHasta: [null],
      sorting: ['FechaSalida DESC'],
    });

    //Carga de datos

    const stream = (query) => {
      //Armamos el dto q necesita getviajes
      const input: GetViajesDto = this.toGetInput(query);
      return this.viajeService.getList(input);
    };

    this.list.hookToQuery(stream).subscribe(res => (this.viajes = res));
  }

  private toGetInput(query: any): GetViajesDto {
    const { fechaDesde, fechaHasta, sorting } = this.filtroForm.value;

    return {
      skipCount: query.skipCount,
      maxResultCount: query.maxResultCount,
      sorting: sorting,
      fechaSalidaDesde: fechaDesde ? new Date(fechaDesde).toISOString() : undefined,
      fechaSalidaHasta: fechaHasta ? new Date(fechaHasta).toISOString() : undefined,
    } as GetViajesDto;
  }

  aplicarFiltros() {
    this.list.get(); // vuelve a consultar con los filtros actuales
  }

  limpiarFiltros() {
    this.filtroForm.reset({ sorting: 'FechaSalida DESC' });
    this.list.get();
  }

  onSort(event: any) {
    const sort = event?.sorts?.[0];
    if (!sort) return;
    const dir = (sort.dir || 'asc').toUpperCase(); // 'ASC' | 'DESC'
    this.filtroForm.patchValue({ sorting: `${sort.prop} ${dir}` });
    this.list.get();
  }

  //Probar el enum 
  enumLabel(value: unknown): string {
    if (value === null || value === undefined) return '-';
    const name = typeof value === 'number' ? this.asEnum[value as any] : value; // Avion | "Avion"
    const labels: Record<string, string> = {
      Avion: 'Avión',
      Tren: 'Tren',
      Auto: 'Auto',
      Autobus: 'Autobús',
    };
    const key = String(name);
    return labels[key] ?? key ?? '-';
  }
}