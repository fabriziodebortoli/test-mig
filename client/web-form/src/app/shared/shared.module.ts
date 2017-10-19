import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TaskbuilderCoreModule } from '@taskbuilder/core';
import { TbIconsModule } from '@taskbuilder/icons';
import { ERPModule } from '@taskbuilder/erp';

@NgModule({
    imports: [
        CommonModule,
        TaskbuilderCoreModule,
        TbIconsModule,
        ERPModule
    ],
    exports: [TaskbuilderCoreModule, TbIconsModule, ERPModule]
})
export class SharedModule { }
