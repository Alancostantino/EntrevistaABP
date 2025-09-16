import { Component, OnInit } from '@angular/core';
import { ListService, PagedResultDto, PermissionService } from '@abp/ng.core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { NgbDateAdapter, NgbDateNativeAdapter } from '@ng-bootstrap/ng-bootstrap';
import { medioDeTransporteOptions, MedioDeTransporte as MedioEnum } from '@proxy/domain/shared/enums/medio-de-transporte.enum';
import { ViajeService } from '@proxy/application/servicios';
import { ViajeDto, GetViajesDto } from '@proxy/application/contracts/dtos';
import { ToasterService, ConfirmationService } from '@abp/ng.theme.shared';
import { finalize } from 'rxjs/operators';
import { SelectionType } from '@swimlane/ngx-datatable';

@Component({
  selector: 'app-viajes',
  templateUrl: './viajes.component.html',
  providers: [ListService, { provide: NgbDateAdapter, useClass: NgbDateNativeAdapter }],
})
export class ViajesComponent implements OnInit {

  viajes = { items: [], totalCount: 0 } as PagedResultDto<ViajeDto>;
  canManage = false;

  medioLabels: Record<string, string> = {
    Avion: 'Avión',
    Tren: 'Tren',
    Auto: 'Auto',
    Autobus: 'Autobús',
  };

  readonly MedioEnum = MedioEnum;

  mediosOpts: Array<{ label: string; value: number }> = [];

  // Modales
  isModalOpen = false; // crear/editar viaje
  isManageOpen = false; // asignar pasajero / cambiar coordinador
  isViewOpen = false; // ver pasajeros del viaje

  // Estados de modal
  formMode: 'create' | 'edit' = 'create';
  manageMode: 'passenger' | 'coordinator' = 'passenger';

  // IDs y selección
  viajeId: string | null = null;
  manageViajeId: string | null = null;
  selectedViaje: ViajeDto | null = null;

  saving = false;

  // Formularios
  crearForm!: FormGroup;
  filtroForm!: FormGroup;
  manageForm!: FormGroup;

  constructor(
    public readonly list: ListService,
    private viajeService: ViajeService,
    private fb: FormBuilder,
    private toaster: ToasterService,
    private confirmation: ConfirmationService,
    private perms: PermissionService
  ) {}

  ngOnInit(): void {
    this.canManage =
      this.perms.getGrantedPolicy('EntrevistaABP.Viajes.Update') ||
      this.perms.getGrantedPolicy('EntrevistaABP.Viajes.Delete');

    // Filtros
    this.filtroForm = this.fb.group({
      fechaDesde: [null],
      fechaHasta: [null],
      sorting: ['FechaSalida DESC'],
    });

    // Form Crear/Editar
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

    // Form Asignar pasajero / Cambiar coordinador
    this.manageForm = this.fb.group({
      modo: ['existente'],
      // existente:
      dniExistente: [null],
      // nuevo:
      nombre: [''],
      apellido: [''],
      dni: [null],
      fechaNacimiento: [null],
    });

    //Validar modo de uso 

    this.validarModo('existente');
    
    this.manageForm
      .get('modo')!
      .valueChanges.subscribe((m: 'existente' | 'nuevo') => this.validarModo(m));

    // Opciones enum para el <select>
    this.mediosOpts = medioDeTransporteOptions.map(o => ({
      value: o.value,
      label: this.medioLabels[o.key] ?? o.key,
    }));
    
    // Carga de datos
    const stream = (query: any) => {
      const input: GetViajesDto = this.toGetInput(query);
      return this.viajeService.getList(input);
    };
    this.list.hookToQuery(stream).subscribe(res => (this.viajes = res));
  }

  //crea el DTO
  private toGetInput(query: any): GetViajesDto {
    const { fechaDesde, fechaHasta, sorting } = this.filtroForm.value;
    return {
      skipCount: query.skipCount,
      maxResultCount: query.maxResultCount,
      sorting,
      fechaSalidaDesde: fechaDesde ? new Date(fechaDesde).toISOString() : undefined,
      fechaSalidaHasta: fechaHasta ? new Date(fechaHasta).toISOString() : undefined,
    } as GetViajesDto;
  }

  aplicarFiltros() {
    this.list.get();
  }
  limpiarFiltros() {
    this.filtroForm.reset({ sorting: 'FechaSalida DESC' });
    this.list.get();
  }

  onSort(event: any) {
    const sort = event?.sorts?.[0];
    if (!sort) return;
    const dir = (sort.dir || 'asc').toUpperCase();
    this.filtroForm.patchValue({ sorting: `${sort.prop} ${dir}` });
    this.list.get();
  }

//Setear correctamente el dateTime

  private toInputLocal(iso?: string | null): string | null {
    if (!iso) return null;
    const d = new Date(iso);
    const pad = (n: number) => String(n).padStart(2, '0');
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(
      d.getHours()
    )}:${pad(d.getMinutes())}`;
  }
  private fromInputDateTime(s?: string | null): string | null {
    return s ? `${s}:00` : null;
  }
  private fromDateOnly(s?: string | null): string | null {
    return s ? `${s}T00:00:00` : null;
  }

  // Crear / Editar
  openCrearModal() {
    this.formMode = 'create';
    this.viajeId = null;
    this.crearForm.reset();
    (this.crearForm.get('coordinadorNuevo') as FormGroup).enable({ emitEvent: false });
    this.isModalOpen = true;
  }
  openEdit(row: ViajeDto) {
    this.formMode = 'edit';
    this.viajeId = row.id as any;
    (this.crearForm.get('coordinadorNuevo') as FormGroup).disable({ emitEvent: false });
    this.crearForm.patchValue({
      fechaSalida: this.toInputLocal(row.fechaSalida),
      fechaLlegada: this.toInputLocal(row.fechaLlegada),
      origen: row.origen,
      destino: row.destino,
      medioDeTransporte: row.medioDeTransporte,
    });
    this.isModalOpen = true;
  }

  // Gestionar pasajeros / coordinador
  
  openAsignarPasajero(row: ViajeDto) {
    this.manageMode = 'passenger';
    this.manageViajeId = row.id as any;
    this.manageForm.reset({
      modo: 'existente',
      dniExistente: null,
      nombre: '',
      apellido: '',
      dni: null,
      fechaNacimiento: null,
    });
    this.validarModo('existente');
    this.isManageOpen = true;
  }

  openCambiarCoordinador(row: ViajeDto) {
    this.manageMode = 'coordinator';
    this.manageViajeId = row.id as any;
    this.manageForm.reset({
      modo: 'existente',
      dniExistente: null,
      nombre: '',
      apellido: '',
      dni: null,
      fechaNacimiento: null,
    });
    this.validarModo('existente');
    this.isManageOpen = true;
  }

  guardarGestion() {
    if (this.manageForm.invalid || !this.manageViajeId) {
      this.manageForm.markAllAsTouched();
      return;
    }
    const v = this.manageForm.value;

    let payload: any = {};

    if (this.manageMode === 'passenger') {
      payload =
        v.modo === 'existente'
          ? { dniExistente: Number(v.dniExistente) }
          : {
              pasajeroNuevo: {
                nombre: v.nombre,
                apellido: v.apellido,
                dni: Number(v.dni),
                fechaNacimiento: this.fromDateOnly(v.fechaNacimiento)!,
              },
            };
    } else {
      payload =
        v.modo === 'existente'
          ? { dniExistente: Number(v.dniExistente) }
          : {
              pasajeroNuevo: {
                nombre: v.nombre,
                apellido: v.apellido,
                dni: Number(v.dni),
                fechaNacimiento: this.fromDateOnly(v.fechaNacimiento)!,
              },
            };
    }

    this.saving = true;
    const req$ =
      this.manageMode === 'passenger'
        ? this.viajeService.asignarPasajero(this.manageViajeId, payload as any)
        : this.viajeService.cambiarCoordinador(this.manageViajeId, payload as any);

    req$.pipe(finalize(() => (this.saving = false))).subscribe({
      next: () => {
        this.toaster.success(
          this.manageMode === 'passenger' ? 'Pasajero asignado.' : 'Coordinador cambiado.'
        );
        this.isManageOpen = false;
        this.manageForm.reset({ modo: 'existente' });
        this.list.get();
      },
      error: e => this.handleError(e, 'No se pudo completar la operación.'),
    });
  }

  // Ver pasajeros
  openVerPasajeros(row: ViajeDto) {
    this.selectedViaje = row;
    this.isViewOpen = true;
  }

  openAgregarDesdeVista() {
    if (!this.selectedViaje) return;
    this.isViewOpen = false;
    this.openAsignarPasajero(this.selectedViaje);
  }

  // Guardar viaje (create/update)
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
      const dto = {
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

  confirmDelete(row: ViajeDto) {
    this.confirmation
      .warn('¿Seguro que querés eliminar este viaje?', 'Confirmación', { hideCancelBtn: false })
      .subscribe(status => {
        if (status === 'confirm') {
          this.saving = true;
          this.viajeService
            .delete(row.id!)
            .pipe(finalize(() => (this.saving = false)))
            .subscribe({
              next: () => {
                this.toaster.success('Viaje eliminado.');
                this.list.get();
              },
              error: e => this.handleError(e, 'No se pudo eliminar el viaje.'),
            });
        }
      });
  }

  private handleError(e: any, fallback: string) {
    const code = e?.error?.error?.code || '';
    const msg = e?.error?.error?.message || fallback;
    const map: Record<string, string> = {
      FechaLlegadaDebeSerMayorQueSalida: 'La fecha de llegada debe ser mayor que la de salida.',
      OrigenYDestinoNoPuedenSerIguales: 'Origen y destino no pueden ser iguales.',
      'Viaje.NoSePuedeEliminarConPasajeros':
        'No se puede eliminar un viaje con pasajeros asignados.',
      PasajeroYaAsignado: 'El pasajero ya está asignado a este viaje.',
      ElCoordinadorYaEstaAsignadoComoTal: 'Ese pasajero ya es el coordinador.',
      DebeIndicarPasajeroExistenteOPasajeroNuevo: 'Indicá pasajero existente o cargá sus datos.',
      DebeIndicarCoordinadorExistenteONuevo: 'Indicá coordinador existente o cargá sus datos.',
    };
    this.toaster.error(map[code] ?? msg);
    console.error('Error', e);
  }

  //Variables para Seleccionar viaje 

  selected: ViajeDto[] = [];
  SelectionType = SelectionType;
  lastClickId: string | null = null;

  onActivate(e: any) {
    if (e.type === 'click') {
      this.lastClickId = e.row?.id ?? null;
    }
  }

  onSelect(e: any) {
    const row = e.selected?.[0] as ViajeDto | undefined;

    // si clickeaste la misma fila que ya estaba seleccionada . deseleccionar
    if (
      row &&
      this.selectedViaje &&
      row.id === this.selectedViaje.id &&
      this.lastClickId === row.id
    ) {
      this.selected = [];
      this.selectedViaje = null;
      return;
    }

    this.selected = e.selected;
    this.selectedViaje = row ?? null;
  }

  rowClass = (row: ViajeDto) => ({
    'is-selected': this.selectedViaje?.id === row.id,
  });


  //BOTONES UI
  VerPasajeros() {
    if (this.selectedViaje) this.openVerPasajeros(this.selectedViaje);
  }
  SeleccionarPasajeros() {
    if (this.selectedViaje) this.openAsignarPasajero(this.selectedViaje);
  }
  CambiarCoordinador() {
    if (this.selectedViaje) this.openCambiarCoordinador(this.selectedViaje);
  }

  //VALIDAR MODO DE CREAR PASAJERO/COORDINADOR

  private validarModo(modo: 'existente' | 'nuevo') {
    const dniExistente = this.manageForm.get('dniExistente')!;
    const nombre = this.manageForm.get('nombre')!;
    const apellido = this.manageForm.get('apellido')!;
    const dni = this.manageForm.get('dni')!;
    const fechaNacimiento = this.manageForm.get('fechaNacimiento')!;

    if (modo === 'existente') {
      dniExistente.setValidators([Validators.required]);
      nombre.clearValidators();
      apellido.clearValidators();
      dni.clearValidators();
      fechaNacimiento.clearValidators();
    } else {
      dniExistente.clearValidators();
      nombre.setValidators([Validators.required]);
      apellido.setValidators([Validators.required]);
      dni.setValidators([Validators.required]);
      fechaNacimiento.setValidators([Validators.required]);
    }
    [dniExistente, nombre, apellido, dni, fechaNacimiento].forEach(c =>
      c.updateValueAndValidity({ emitEvent: false })
    );
  }
}
