import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpModule } from '@angular/http';

import { TaskbuilderCoreModule } from '@taskbuilder/core';
import { TbIconsModule } from '@taskbuilder/icons';

@NgModule({
  imports: [
    CommonModule,
    HttpModule,
    TaskbuilderCoreModule,
    TbIconsModule
  ],
  exports: [
    TaskbuilderCoreModule,
    TbIconsModule
  ]
})
export class SharedModule { }
