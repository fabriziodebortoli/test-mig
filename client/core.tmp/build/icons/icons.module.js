import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TbIconComponent } from './tb-icon.component';
import { M4IconComponent } from './m4-icon.component';
export { TbIconComponent } from './tb-icon.component';
export { M4IconComponent } from './m4-icon.component';
export class TbIconsModule {
}
TbIconsModule.decorators = [
    { type: NgModule, args: [{
                imports: [CommonModule],
                declarations: [TbIconComponent, M4IconComponent],
                exports: [TbIconComponent, M4IconComponent]
            },] },
];
/**
 * @nocollapse
 */
TbIconsModule.ctorParameters = () => [];
function TbIconsModule_tsickle_Closure_declarations() {
    /** @type {?} */
    TbIconsModule.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TbIconsModule.ctorParameters;
}
