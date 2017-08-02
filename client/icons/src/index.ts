import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TbIconComponent } from './tb-icon.component';
import { M4IconComponent } from './m4-icon.component';

export * from './tb-icon.component';
export * from './m4-icon.component';

@NgModule({
  imports: [CommonModule],
  declarations: [TbIconComponent, M4IconComponent],
  exports: [TbIconComponent, M4IconComponent]
})
export class TbIconsModule { }
