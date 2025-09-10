import { Component, OnInit, TemplateRef } from '@angular/core';
import { ListService, PagedResultDto } from '@abp/ng.core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbDateAdapter, NgbDateNativeAdapter } from '@ng-bootstrap/ng-bootstrap';
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

  isModalOpen = false;
  formMode: 'create' | 'edit' = 'create';
  viajeId: string | null = null;
  saving = false;

  crearForm!: FormGroup; //Form del modal creear/editar
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

    //Form del modal Crear/Editar viaje

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

  //Convierte cualquier ISO que venga del back en string
  private toInputLocal(iso?: string | null): string | null {
  if (!iso) return null;
  const d = new Date(iso); // si viene con Z, lo convierte a tu hora local automáticamente
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}
  // Del input "YYYY-MM-DDTHH:mm" devolvemos un string sin zona
 private fromInputDateTime(s?: string | null): string | null {
  return s ? `${s}:00` : null; // <<< NO usamos toISOString, no agregamos 'Z'
}

private fromDateOnly(s?: string | null): string | null {
  return s ? `${s}T00:00:00` : null;
}
  //Abrir modal en modo CREAR
  openCrearModal() {
    this.formMode = 'create';
    this.viajeId = null;
    this.crearForm.reset();
    this.isModalOpen = true;
    (this.crearForm.get('coordinadorNuevo') as FormGroup).enable({emitEvent:false});
  }

  //ABRIR MODAL EN MODO EDITAR

  openEdit(row: ViajeDto) {
    this.formMode = 'edit';
    this.viajeId = row.id as any;
    (this.crearForm.get('coordinadorNuevo') as FormGroup).disable({emitEvent:false});
    this.crearForm.patchValue({
      fechaSalida: this.toInputLocal(row.fechaSalida),
      fechaLlegada: this.toInputLocal(row.fechaLlegada),
      origen: row.origen,
      destino: row.destino,
      medioDeTransporte: row.medioDeTransporte,
      // coordinador Nuevo no aplica en editar
    });
    this.isModalOpen = true;
  }

  guardar() {
    if (this.crearForm.invalid) {
      this.crearForm.markAllAsTouched();
      return;
    }

    const v = this.crearForm.value;
    if (this.formMode === 'create') {
      const dto = {
        fechaSalida: this.fromInputDateTime(v.fechaSalida)!,
        fechaLlegada: this.fromInputDateTime(v.fechaLlegada)!,
        origen: v.origen,
        destino: v.destino,
        medioDeTransporte: v.medioDeTransporte,
        coordinadorNuevo: {
          nombre: v.coordinadorNuevo?.nombre,
          apellido: v.coordinadorNuevo?.apellido,
          dni: Number(v.coordinadorNuevo?.dni),
          fechaNacimiento: this.fromDateOnly(v.coordinadorNuevo?.fechaNacimiento)!,
        },
      };
      this.saving = true;
      this.viajeService
        .create(dto)
        .pipe(finalize(() => (this.saving = false)))
        .subscribe({
          next: () => {
            this.toaster.success('Viaje creado correctamente.');
            this.isModalOpen = false;
            this.crearForm.reset();
            this.list.get();
          },
          error: e => this.handleError(e, 'No se pudo crear el viaje.'),
        });
    } else {
      const dto /*: UpdateViajeDto*/ = {
        id: this.viajeId!,
        fechaSalida: this.fromInputDateTime(v.fechaSalida)!,
        fechaLlegada: this.fromInputDateTime(v.fechaLlegada)!,
        origen: v.origen?.trim(),
        destino: v.destino?.trim(),
        medioDeTransporte: v.medioDeTransporte,
      };
      this.saving = true;
      this.viajeService
        .update(dto as any)
        .pipe(finalize(() => (this.saving = false)))
        .subscribe({
          next: () => {
            this.toaster.success('Viaje actualizado correctamente.');
            this.isModalOpen = false;
            this.crearForm.reset();
            this.list.get();
          },
          error: e => this.handleError(e, 'No se pudo actualizar el viaje.'),
        });
    }
  }
  private handleError(e: any, fallback: string) {
    const code = e?.error?.error?.code || '';
    const msg = e?.error?.error?.message || fallback;
    const map: Record<string, string> = {
      FechaLlegadaDebeSerMayorQueSalida: 'La fecha de llegada debe ser mayor que la de salida.',
      OrigenYDestinoNoPuedenSerIguales: 'Origen y destino no pueden ser iguales.',
    };
    this.toaster.error(map[code] ?? msg);
    console.error('Error', e);
  }
}
