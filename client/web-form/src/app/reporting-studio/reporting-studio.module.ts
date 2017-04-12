
import { SharedModule } from './../shared/shared.module';
import { RouterModule } from '@angular/router';

import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';

import { ReportingStudioHostComponent } from './reporting-studio-host.component';
import { ReportingStudioComponent, ReportingStudioFactoryComponent } from './reporting-studio.component';





@NgModule({
  imports: [
    SharedModule,
    CommonModule,
    MaterialModule.forRoot(),
    RouterModule.forChild([
      { path: 'reportingstudio/:ns/:params', component: ReportingStudioFactoryComponent },
    ])
  ],
  declarations: [
    ReportingStudioComponent,
    ReportingStudioHostComponent,
    ReportingStudioFactoryComponent
  ],
  entryComponents:
  [
    ReportingStudioComponent
  ]
})
export class ReportingStudioModule {

  static forRoot(): ModuleWithProviders {
    return { ngModule: ReportingStudioModule };
  }
}
