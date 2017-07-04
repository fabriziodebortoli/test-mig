import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { GridModule } from '@progress/kendo-angular-grid';
import { ChartsModule } from '@progress/kendo-angular-charts';

import { TbSharedModule } from './../../shared/shared.module';

import { WidgetContainerComponent } from './widget-container.component';
import { WidgetComponent } from './widget.component';
import { WidgetsService } from './widgets.service';
import { WidgetStatsComponent } from './widget-stats.component';
import { WidgetGridComponent } from './widget-grid.component';
import { WidgetChartComponent } from './widget-chart.component';

@NgModule({
  imports: [
    CommonModule,
    TbSharedModule,
    GridModule,
    ChartsModule
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
