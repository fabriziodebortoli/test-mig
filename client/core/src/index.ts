import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MdButtonModule } from '@angular/material';

import { SampleComponent } from './sample.component';

export * from './sample.component';

@NgModule({
  imports: [
    CommonModule,
    MdButtonModule
  ],
  declarations: [
    SampleComponent
  ],
  exports: [
    SampleComponent
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
