import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { IconComponent } from './icon.component';
import { M4IconComponent } from './m4-icon.component';

export * from './icon.component';
export * from './m4-icon.component';

@NgModule({
    imports: [CommonModule],
    declarations: [IconComponent, M4IconComponent],
    exports: [IconComponent, M4IconComponent]
})
export class TbIconsModule {
}
