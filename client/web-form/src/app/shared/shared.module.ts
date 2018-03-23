import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TaskbuilderCoreModule } from '@taskbuilder/core';
import { TbIconsModule } from '@taskbuilder/icons';
import { TbLibsModule } from '@taskbuilder/libs';
import { ERPModule } from '@taskbuilder/erp';

@NgModule({
    imports: [
        CommonModule,
        TaskbuilderCoreModule,
        TbIconsModule,
        TbLibsModule,
        ERPModule
    ],
    exports: [TaskbuilderCoreModule, TbIconsModule, TbLibsModule, ERPModule]
})
export class SharedModule { }
