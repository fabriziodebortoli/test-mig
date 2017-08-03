import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TbSharedModule } from './../../shared/shared.module';

import { WidgetContainerComponent } from './widget-container.component';
import { WidgetComponent } from './widget.component';
import { WidgetsService } from './widgets.service';
import { WidgetStatsComponent } from './widget-stats.component';
import { WidgetGridComponent } from './widget-grid.component';
import { WidgetChartComponent } from './widget-chart.component';
import { WidgetClockComponent } from "./widget-clock.component";

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


@NgModule({
  imports: [
    KENDO_UI_MODULES,
    CommonModule,
    TbSharedModule
  ],
  declarations: [
    WidgetContainerComponent,
    WidgetComponent,
    WidgetStatsComponent,
    WidgetGridComponent,
    WidgetChartComponent,
    WidgetClockComponent
  ],
  exports: [
    WidgetContainerComponent,
    WidgetComponent,
    WidgetStatsComponent,
    WidgetGridComponent,
    WidgetChartComponent,
    WidgetClockComponent
  ]
})
export class WidgetsModule { }
