import { ReportingStudioConnectionComponent } from './reporting-studio-connection.component';
import { rsRouting } from './reporting-studio.routing';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';

import { ReportingStudioHostComponent } from './reporting-studio-host.component';
import { ReportingStudioComponent } from './reporting-studio.component';
import { ReportObjectWrapperComponent } from './report-object-wrapper/report-object-wrapper.component';
import { ReportObjectDirective } from './report-object-wrapper/report-object.directive';

import {
  ReportObjectRectangleComponent, ReportObjectImageComponent,
  ReportObjectFileComponent, ReportObjectTableComponent, ReportObjectTextComponent
} from './report-object-wrapper/report-objects';

@NgModule({
  imports: [
    CommonModule,
    MaterialModule.forRoot(),
    rsRouting
  ],
  declarations: [
    ReportingStudioComponent, ReportingStudioHostComponent, ReportingStudioConnectionComponent,
    ReportObjectWrapperComponent,
    ReportObjectDirective,
    ReportObjectRectangleComponent, ReportObjectImageComponent, ReportObjectTextComponent, ReportObjectFileComponent, ReportObjectTableComponent
  ]
})
export class ReportingStudioModule {

  static forRoot(): ModuleWithProviders {
    return { ngModule: ReportingStudioModule };
  }
}
