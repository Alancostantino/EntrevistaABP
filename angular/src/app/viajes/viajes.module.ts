import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { NgbDatepickerModule } from '@ng-bootstrap/ng-bootstrap'; 
import { ViajesRoutingModule } from './viajes-routing.module';
import { ViajesComponent } from './viajes.component';
import { SharedModule } from '../shared/shared.module';


@NgModule({
  declarations: [
    ViajesComponent
  ],
  imports: [
    CommonModule,
    ViajesRoutingModule,SharedModule,NgbDatepickerModule

  ]
})
export class ViajesModule { }
