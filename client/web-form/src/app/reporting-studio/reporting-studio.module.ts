import { RouterModule } from '@angular/router';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';

import { ReportingStudioService } from './reporting-studio.service';
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
    RouterModule.forChild([
      { path: ':namespace/:params', component: ReportingStudioComponent }
    ])
  ],
  providers: [ReportingStudioService],
  declarations: [
    ReportingStudioComponent,
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
