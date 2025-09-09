import { Component, OnInit, TemplateRef } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import {
  NgbDateAdapter,
  NgbDateNativeAdapter
} from '@ng-bootstrap/ng-bootstrap';
import { MedioDeTransporte as MedioEnum } from '@proxy/domain/shared/enums/medio-de-transporte.enum';
// Importá del proxy generado por ABP (rutas reales según tu CLI)
import { ViajeService } from '@proxy/application/servicios';
import { ViajeDto, GetViajesDto } from '@proxy/application/contracts/dtos';
import { ToasterService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs/operators';




@Component({
  selector: 'app-viajes',
  templateUrl: './viajes.component.html',
  providers: [ListService, { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }],
})
export class ViajesComponent implements OnInit {
  viajes = { items: [], totalCount: 0 } as PagedResultDto<ViajeDto>; //Iterable de tabla

  readonly MedioEnum = MedioEnum;
  private readonly asEnum: any = MedioEnum || {};
  mediosOpts: Array<{ label: string; value: number }> = [];

   isCrearOpen = false;
   saving = false;

  crearForm!: FormGroup; //Form del modal creear
  filtroForm: FormGroup; //Form de filtros

  constructor(
    public readonly list: ListService,
    private viajeService: ViajeService,
    private fb: FormBuilder,
    private toaster: ToasterService
  ) {}

  ngOnInit(): void {
    // Formulario de filtro (rango de salida + sorting)

    this.filtroForm = this.fb.group({
      fechaDesde: [null],
      fechaHasta: [null],
      sorting: ['FechaSalida DESC'],
    });

    //Opciones del enum para usar "select"

    this.mediosOpts = Object.keys(this.asEnum)
      .filter(k => !isNaN(Number(k))) // solo claves numéricas (0,1,2,3)
      .map(k => {
        const nombre = this.asEnum[k]; // Avion, Tren, ...
        return { label: this.enumLabel(nombre), value: Number(k) };
      });

    //Form del modal Crear Viaje

    this.crearForm = this.fb.group({
      fechaSalida: [null, Validators.required],
      fechaLlegada: [null, Validators.required],
      origen: ['', Validators.required],
      destino: ['', Validators.required],
      medioDeTransporte: [null, Validators.required],
      coordinadorNuevo: this.fb.group({
        nombre: ['', Validators.required],
        apellido: ['', Validators.required],
        dni: [null, Validators.required],
        fechaNacimiento: [null, Validators.required],
      }),
    });

    //Carga de datos

    const stream = query => {
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
    const dir = (sort.dir || 'asc').toUpperCase(); // ASC  DESC
    this.filtroForm.patchValue({ sorting: `${sort.prop} ${dir}` });
    this.list.get();
  }

  //Probar el enum
  enumLabel(value: unknown): string {
    if (value === null || value === undefined) return '-';
    const name = typeof value === 'number' ? this.asEnum[value as any] : value;
    const labels: Record<string, string> = {
      Avion: 'Avión',
      Tren: 'Tren',
      Auto: 'Auto',
      Autobus: 'Autobús',
    };
    const key = String(name);
    return labels[key] ?? key ?? '-';
  }

  //CREAR MODAL
  openCrearModal() {
    this.isCrearOpen = true;
  }

  onCrear() {
    if (this.crearForm.invalid) {
      this.crearForm.markAllAsTouched();
      return;
    }

    const v = this.crearForm.value;
    const toIso = (x: string | null) => (x ? new Date(x).toISOString() : null);

    const dto = {
      fechaSalida: toIso(v.fechaSalida)!,
      fechaLlegada: toIso(v.fechaLlegada)!,
      origen: v.origen,
      destino: v.destino,
      medioDeTransporte: v.medioDeTransporte,
      coordinadorNuevo: {
        nombre: v.coordinadorNuevo?.nombre,
        apellido: v.coordinadorNuevo?.apellido,
        dni: Number(v.coordinadorNuevo?.dni),
        fechaNacimiento: toIso(v.coordinadorNuevo?.fechaNacimiento)!,
      },
    };

    this.saving = true;
    this.viajeService.create(dto)
      .pipe(finalize(() => (this.saving = false)))
      .subscribe({
        next: () => {
          this.toaster.success('Viaje creado correctamente.');
          this.isCrearOpen = false;
          this.crearForm.reset();
          this.list.get();
        },
        error: e => {
          const code = e?.error?.error?.code || '';
          const msg =
            e?.error?.error?.message ||
            'No se pudo crear el viaje. Revisá los datos e intentá nuevamente.';
          const map: Record<string, string> = {
            FechaLlegadaDebeSerMayorQueSalida:
              'La fecha de llegada debe ser mayor que la de salida.',
            OrigenYDestinoNoPuedenSerIguales:
              'Origen y destino no pueden ser iguales.',
            DebeIndicarCoordinadorExistenteOCoordinadorNuevo:
              'Debés indicar un coordinador (existente o nuevo).',
          };
          this.toaster.error(map[code] ?? msg);
          console.error('Create error', e);
        },
      });
  }

  
}
