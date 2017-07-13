import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Modulo Core con tutti i principali servizi e componenti di TB
 */
import { TbCoreModule, TB_SERVICES } from './core/core.module';
export * from './core/core.module';
// export * from './core';

/**
 * Modulo Icon Font
 */
import { TbIconsModule } from './icons/icons.module';
export * from './icons/icons.module';

/**
 * Modulo Shared
 */
import { TbSharedModule } from './shared/shared.module';
export * from './shared/shared.module';

/**
 * Modulo Menu
 */
import { TbMenuModule } from './menu/menu.module';
export * from './menu/menu.module';

/**
 * Modulo Dashboard
 */
import { TbDashboardModule } from './dashboard/dashboard.module';
export * from './dashboard/dashboard.module';

/**
 * Modulo Test
 */
import { TbTestModule } from './test/test.module';
export * from './test/test.module';

const TB_MODULES = [
  // TbCoreModule,
  TbSharedModule,
  TbIconsModule,
  TbMenuModule,
  TbDashboardModule,
  TbTestModule
];

export * from './shared/models';
export { SocketConnectionStatus } from './shared';

import { HomeComponent, HomeSidenavComponent, StandaloneReportComponent, StandaloneDocumentComponent } from './home';
const TB_HOME_COMPONENTS = [HomeComponent, HomeSidenavComponent, StandaloneReportComponent, StandaloneDocumentComponent];
export * from './home';

import { LayoutModule } from '@progress/kendo-angular-layout';

@NgModule({
  imports: [CommonModule, TB_MODULES, LayoutModule],
  declarations: [TB_HOME_COMPONENTS],
  exports: [TB_MODULES, TB_HOME_COMPONENTS]
})
export class TaskbuilderCoreModule { }