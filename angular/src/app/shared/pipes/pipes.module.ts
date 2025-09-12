import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { EnumLabelPipe } from './enum-label.pipe';

@NgModule({
  declarations: [EnumLabelPipe],
  imports: [CommonModule],
  exports: [EnumLabelPipe],
})
export class PipesModule {}