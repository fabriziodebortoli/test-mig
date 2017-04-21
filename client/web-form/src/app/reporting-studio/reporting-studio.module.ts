import { ReportLinkComponent } from './../shared/report-objects/link/link.component';
import { ReportRectComponent } from './../shared/report-objects/rect/rect.component';
import { ReportImageComponent } from './../shared/report-objects/image/image.component';
import { ReportTableComponent } from './../shared/report-objects/table/table.component';
import { ReportFieldrectComponent } from './../shared/report-objects/fieldrect/fieldrect.component';
import { ReportTextrectComponent } from './../shared/report-objects/textrect/textrect.component';
import { GridModule } from '@progress/kendo-angular-grid';

import { SharedModule } from './../shared/shared.module';
import { RouterModule } from '@angular/router';

import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';

import { ReportingStudioHostComponent } from './reporting-studio-host.component';
import { ReportingStudioComponent, ReportingStudioFactoryComponent } from './reporting-studio.component';



const KENDO_UI_MODULES = [
  GridModule,
  /*InputsModule,
  DateInputsModule,
  DialogModule,
  DropDownsModule,
  LayoutModule,
  PopupModule,
  ButtonsModule*/
];

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    KENDO_UI_MODULES,
    MaterialModule.forRoot(),
    RouterModule.forChild([
      { path: 'reportingstudio/:ns/:params', component: ReportingStudioFactoryComponent },
      { path: 'reportingstudio/', component: ReportingStudioFactoryComponent },
    ])
  ],
  declarations: [
    ReportingStudioComponent,
    ReportingStudioHostComponent,
    ReportingStudioFactoryComponent,
    ReportTextrectComponent,
    ReportFieldrectComponent,
    ReportTableComponent,
    ReportImageComponent,
    ReportRectComponent,
    ReportLinkComponent,
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
