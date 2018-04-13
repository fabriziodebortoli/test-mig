import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TbKendoModule } from './kendo/kendo.module';
export * from './kendo/kendo.module';

import { NgxGalleryModule } from 'ngx-gallery';

@NgModule({
  imports: [
    CommonModule,
    TbKendoModule,
    NgxGalleryModule
  ],
  exports: [
    TbKendoModule,
    NgxGalleryModule
  ]
})
export class TbLibsModule { }
