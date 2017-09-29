import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { M4IconComponent } from './m4-icon.component';

export * from './m4-icon.component';

@NgModule({
  imports: [CommonModule],
  declarations: [M4IconComponent],
  exports: [M4IconComponent]
})
export class TbIconsModule { }
