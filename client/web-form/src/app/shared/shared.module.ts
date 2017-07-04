import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TaskbuilderCoreModule } from '@taskbuilder/core';

@NgModule({
    imports: [
        CommonModule,
        TaskbuilderCoreModule
    ],
    exports: [TaskbuilderCoreModule]
})
export class SharedModule { }