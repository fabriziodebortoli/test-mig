import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MdButtonModule } from '@angular/material';

import { SampleComponent } from './sample.component';

import { TbIconsModule } from './icons/icons.module';

export * from './sample.component';

@NgModule({
  imports: [
    CommonModule,
    MdButtonModule,
    TbIconsModule
  ],
  declarations: [
    SampleComponent
  ],
  exports: [
    SampleComponent, TbIconsModule
  ]
})
export class TaskbuilderCoreModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: TaskbuilderCoreModule,
      // providers: [SampleService]
    };
  }
}
