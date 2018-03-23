import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TbKendoModule } from './kendo/kendo.module';
export * from './kendo/kendo.module';

@NgModule({
  imports: [
    CommonModule,
    TbKendoModule
  ],
  exports: [
    TbKendoModule
  ]
})
export class TbLibsModule { }
