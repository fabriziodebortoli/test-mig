import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

import { RsTestComponent } from './rs-test.component';
export * from './rs-test.component';

@NgModule({
  imports: [
    CommonModule
  ],
  declarations: [
    RsTestComponent
  ],
  exports: [
    RsTestComponent
  ]
})
export class ReportingStudioModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: ReportingStudioModule,
      // providers: [SampleService]
    };
  }
}
