import { ReportingStudioService } from './reporting-studio.service';
import { AskdialogService } from './report-objects/askdialog/askdialog.service';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

import { RsTestComponent } from './rs-test.component';
export * from './rs-test.component';

export * from './models';

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
       providers: [
         ReportingStudioService,
         AskdialogService
         ]
    };
  }
}
