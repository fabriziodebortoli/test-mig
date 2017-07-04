import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TbSharedModule } from './../shared/shared.module';
import { TbMenuModule } from './../menu/menu.module';

import { WidgetsModule } from './widgets/widgets.module';
import { WidgetsService } from './widgets/widgets.service';

import { DashboardComponent } from './dashboard/dashboard.component';

@NgModule({
  imports: [
    CommonModule,
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
