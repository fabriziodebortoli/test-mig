import { FormsModule } from '@angular/forms';

import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { PopupModule } from '@progress/kendo-angular-popup';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { GridModule } from '@progress/kendo-angular-grid';

import { ReportLinkComponent } from './report-objects/layout/link/link.component';
import { ReportRectComponent } from './report-objects/layout/rect/rect.component';
import { ReportImageComponent } from './report-objects/layout/image/image.component';
import { ReportTableComponent } from './report-objects/layout/table/table.component';
import { ReportFieldrectComponent } from './report-objects/layout/fieldrect/fieldrect.component';
import { ReportTextrectComponent } from './report-objects/layout/textrect/textrect.component';
import { AskdialogComponent } from './report-objects/askdialog/askdialog.component';

import { SharedModule } from './../shared/shared.module';
import { RouterModule } from '@angular/router';

import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';

import { ReportingStudioComponent, ReportingStudioFactoryComponent } from './reporting-studio.component';
import { LayoutService } from '@taskbuilder/core';
import { AskGroupComponent } from './report-objects/askdialog/ask-group/ask-group.component';
import { AskCheckComponent } from './report-objects/askdialog/ask-check/ask-check.component';
import { AskRadioComponent } from './report-objects/askdialog/ask-radio/ask-radio.component';
import { AskDropdownlistComponent } from './report-objects/askdialog/ask-dropdownlist/ask-dropdownlist.component';
import { AskTextComponent } from './report-objects/askdialog/ask-text/ask-text.component';
import { AskHotlinkComponent } from './report-objects/askdialog/ask-hotlink/ask-hotlink.component';
import { ReportLayoutComponent } from './report-objects/layout/layout.component';



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
    FormsModule,
    CommonModule,
    SharedModule,
    KENDO_UI_MODULES,
    MaterialModule,
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
    AskdialogComponent,
    AskGroupComponent,
    AskCheckComponent,
    AskRadioComponent,
    AskTextComponent,
    AskDropdownlistComponent,
    AskHotlinkComponent,
    ReportLayoutComponent
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
