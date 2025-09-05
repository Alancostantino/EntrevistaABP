import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { ViajesRoutingModule } from './viajes-routing.module';
import { ViajesComponent } from './viajes.component';
import { SharedModule } from '../shared/shared.module';


@NgModule({
  declarations: [
    ViajesComponent
  ],
  imports: [
    CommonModule,
    ViajesRoutingModule,SharedModule
  ]
})
export class ViajesModule { }
