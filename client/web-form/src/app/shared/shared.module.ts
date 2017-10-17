import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TaskbuilderCoreModule } from '@taskbuilder/core';
import { ERPModule } from '@taskbuilder/erp';
import { TbIconsModule } from '@taskbuilder/icons';

@NgModule({
    imports: [
        CommonModule,
        TaskbuilderCoreModule,
        ERPModule,
        TbIconsModule
    ],
    exports: [TaskbuilderCoreModule, ERPModule, TbIconsModule]
})
export class SharedModule { }
