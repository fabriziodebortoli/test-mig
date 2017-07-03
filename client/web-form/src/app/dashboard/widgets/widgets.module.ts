import { DataService } from '@taskbuilder/core';
import { SharedModule } from './../../shared/shared.module';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { MaterialModule } from '@angular/material';

import { WidgetContainerComponent } from './widget-container.component';
import { WidgetComponent } from './widget.component';
import { WidgetsService } from './widgets.service';
import { WidgetStatsComponent } from './widget-stats.component';
import { WidgetGridComponent } from './widget-grid.component';
import { WidgetChartComponent } from './widget-chart.component';

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

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    MaterialModule,
    KENDO_UI_MODULES
  ],
  declarations: [
    WidgetContainerComponent,
    WidgetComponent,
    WidgetStatsComponent,
    WidgetGridComponent,
    WidgetChartComponent
  ],
  exports: [
    WidgetContainerComponent,
    WidgetComponent,
    WidgetStatsComponent,
    WidgetGridComponent,
    WidgetChartComponent
  ]
})
export class WidgetsModule { }
