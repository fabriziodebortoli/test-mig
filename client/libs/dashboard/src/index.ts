import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TbSharedModule, TbMenuModule } from '@taskbuilder/core';

// import { WidgetsModule } from './widgets/widgets.module';
// import { WidgetsService } from './widgets/widgets.service';

import { DashboardComponent } from './dashboard/dashboard.component';
export * from './dashboard/dashboard.component';

const TB_MODULES = [
  TbSharedModule,
  TbMenuModule
];

@NgModule({
  imports: [
    CommonModule,
    TB_MODULES
  ],
  declarations: [DashboardComponent],
  providers: [/*WidgetsService*/],
  exports: [DashboardComponent]
})
export class TbDashboardModule {
  // static forRoot(): ModuleWithProviders {
  //   return {
  //     ngModule: TbDashboardModule,
  //     providers: [WidgetsService]
  //   };
  // }
}
