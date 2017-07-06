import { ReportingStudioComponent } from './reporting-studio.component';
import { AskdialogComponent } from './report-objects/askdialog/askdialog.component';
import { ReportLayoutComponent } from './report-objects/layout/layout.component';
import { ReportingStudioService } from './reporting-studio.service';
import { AskdialogService } from './report-objects/askdialog/askdialog.service';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TbSharedModule } from '@taskbuilder/core';

import { RsTestComponent } from './rs-test.component';
export * from './rs-test.component';

export * from './models';

@NgModule({
  imports: [
    CommonModule,
    TbSharedModule
  ],
  declarations: [
    RsTestComponent,
    ReportLayoutComponent,
    AskdialogComponent,
    ReportingStudioComponent
  ],
  exports: [
    RsTestComponent,
    ReportLayoutComponent,
    AskdialogComponent,
    ReportingStudioComponent
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
