import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TaskbuilderCoreModule } from '@taskbuilder/core';
import { TbIconsModule } from '@taskbuilder/icons';

@NgModule({
  imports: [
    CommonModule,
    TaskbuilderCoreModule,
    TbIconsModule
  ],
  exports: [TaskbuilderCoreModule]
})
export class SharedModule { }
