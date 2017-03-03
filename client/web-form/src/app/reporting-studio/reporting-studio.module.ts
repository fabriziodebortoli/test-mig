import { SharedModule } from './../shared/shared.module';
import { RouterModule } from '@angular/router';

import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';

import { ReportingStudioHostComponent } from './reporting-studio-host.component';
import { ReportingStudioComponent, ReportingStudioFactoryComponent } from './reporting-studio.component';
import { ReportObjectWrapperComponent } from './report-object-wrapper/report-object-wrapper.component';
import { ReportObjectDirective } from './report-object-wrapper/report-object.directive';

import {
  ReportObjectRectangleComponent, ReportObjectImageComponent,
  ReportObjectFileComponent, ReportObjectTableComponent, ReportObjectTextComponent
} from './report-object-wrapper/report-objects';

@NgModule({
  imports: [
    SharedModule,
    CommonModule,
    MaterialModule.forRoot(),
    RouterModule.forChild([
      { path: 'reportingstudio/:ns', component: ReportingStudioFactoryComponent },
    ])
  ],
  declarations: [
    ReportingStudioComponent, ReportingStudioHostComponent, ReportingStudioFactoryComponent,
    ReportObjectWrapperComponent,
    ReportObjectDirective,
    ReportObjectRectangleComponent, ReportObjectImageComponent, ReportObjectTextComponent, ReportObjectFileComponent, ReportObjectTableComponent
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
