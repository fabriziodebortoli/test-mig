
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { PopupModule } from '@progress/kendo-angular-popup';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { GridModule } from '@progress/kendo-angular-grid';

import { ReportLinkComponent } from './../shared/report-objects/link/link.component';
import { ReportRectComponent } from './../shared/report-objects/rect/rect.component';
import { ReportImageComponent } from './../shared/report-objects/image/image.component';
import { ReportTableComponent } from './../shared/report-objects/table/table.component';
import { ReportFieldrectComponent } from './../shared/report-objects/fieldrect/fieldrect.component';
import { ReportTextrectComponent } from './../shared/report-objects/textrect/textrect.component';

import { SharedModule } from './../shared/shared.module';
import { RouterModule } from '@angular/router';

import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';

import { ReportingStudioComponent, ReportingStudioFactoryComponent } from './reporting-studio.component';
import { AskdialogComponent } from './report-objects/askdialog/askdialog.component';

const KENDO_UI_MODULES = [
  GridModule,
  DialogModule,
  InputsModule,
  DateInputsModule,
  DropDownsModule,
  LayoutModule,
  PopupModule,
  ButtonsModule,

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
    ReportingStudioFactoryComponent,
    ReportTextrectComponent,
    ReportFieldrectComponent,
    ReportTableComponent,
    ReportImageComponent,
    ReportRectComponent,
    ReportLinkComponent,
    AskdialogComponent
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
