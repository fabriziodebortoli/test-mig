import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TbSharedModule } from './../shared/shared.module';
import { TbMenuModule } from './../menu/menu.module';

import { WidgetsModule } from './widgets/widgets.module';
import { WidgetsService } from './widgets/widgets.service';

import { DialogModule } from '@progress/kendo-angular-dialog';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { PopupModule } from '@progress/kendo-angular-popup';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { GridModule } from '@progress/kendo-angular-grid';
import { ChartsModule } from '@progress/kendo-angular-charts';

const KENDO_UI_MODULES = [
  GridModule,
  ChartsModule,
  DialogModule,
  DateInputsModule,
  DropDownsModule,
  InputsModule,
  LayoutModule,
  PopupModule,
  ButtonsModule
];

import { DashboardComponent } from './dashboard/dashboard.component';

@NgModule({
  imports: [
    CommonModule,
    KENDO_UI_MODULES,
    WidgetsModule,
    TbSharedModule,
    TbMenuModule
  ],
  exports: [
    DashboardComponent
  ],
  providers: [WidgetsService],
  declarations: [DashboardComponent]
})
export class TbDashboardModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: TbDashboardModule,
      providers: [WidgetsService]
    };
  }
}
