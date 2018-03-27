import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TbSharedModule } from '@taskbuilder/core';

import { WidgetContainerComponent } from './widget-container.component';
import { WidgetComponent } from './widget.component';
import { WidgetsService } from './widgets.service';
import { WidgetStatsComponent } from './widget-stats.component';
import { WidgetGridComponent } from './widget-grid.component';
import { WidgetChartComponent } from './widget-chart.component';
import { WidgetClockComponent } from "./widget-clock.component";

@NgModule({
  imports: [
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
