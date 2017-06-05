import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TestComponent } from './test/test.component';

export * from './test/test.component';

@NgModule({
  imports: [
    CommonModule
  ],
  declarations: [
    TestComponent
  ],
  exports: [
    TestComponent
  ]
})
export class TaskbuilderModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: TaskbuilderModule,
      // providers: [SampleService]
    };
  }
}
