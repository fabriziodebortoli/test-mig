import { WidgetsModule } from './widgets/widgets.module';
import { WidgetsService } from './widgets/widgets.service';
import { MaterialModule } from '@angular/material';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

import { SharedModule } from '../shared/shared.module';
import { MenuModule } from '../menu/menu.module';

import { GridModule } from '@progress/kendo-angular-grid';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { PopupModule } from '@progress/kendo-angular-popup';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { ChartsModule } from '@progress/kendo-angular-charts';

const KENDO_UI_MODULES = [
  GridModule,
  InputsModule,
  DateInputsModule,
  DialogModule,
  DropDownsModule,
  LayoutModule,
  PopupModule,
  ButtonsModule,
  ChartsModule
];

import { DashboardComponent } from './dashboard/dashboard.component';

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    MenuModule.forRoot(),
    MaterialModule,
    WidgetsModule,
    KENDO_UI_MODULES
  ],
  exports: [
    DashboardComponent
  ],
  providers: [WidgetsService],
  declarations: [DashboardComponent]
})
export class DashboardModule {
   static forRoot(): ModuleWithProviders {
    return {
      ngModule: DashboardModule,
      providers: [WidgetsService]
    };
  }
 }
