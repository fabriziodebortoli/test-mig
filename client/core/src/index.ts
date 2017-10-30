import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

import { TbIconsModule } from '@taskbuilder/icons';

/**
 * Modulo Core con tutti i principali servizi e componenti di TB
 */
import { TbCoreModule, TB_SERVICES } from './core/core.module';
export * from './core/core.module';

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
 * Modulo Settings
 */
import { TbSettingsModule } from './settings/settings.module';
export * from './settings/settings.module';

/**
 * Modulo Test
 */
import { TbTestModule } from './test/test.module';
export * from './test/test.module';

const TB_MODULES = [
  TbSharedModule,
  TbMenuModule,
  TbIconsModule,
  TbDashboardModule,
  TbTestModule,
  TbSettingsModule
];

/** 
 * HOME 
 */
import { StandaloneDocumentComponent } from './home/standalone.document/standalone.document.component';
import { StandaloneReportComponent } from './home/standalone.report/standalone.report.component';
import { HomeSidenavComponent } from './home/home-sidenav/home-sidenav.component';
import { HomeComponent } from './home/home.component';

export { StandaloneDocumentComponent } from './home/standalone.document/standalone.document.component';
export { StandaloneReportComponent } from './home/standalone.report/standalone.report.component';
export { HomeSidenavComponent } from './home/home-sidenav/home-sidenav.component';
export { HomeComponent } from './home/home.component';

const TB_HOME_COMPONENTS = [
  HomeComponent, HomeSidenavComponent,
  StandaloneDocumentComponent,
  StandaloneReportComponent
];

@NgModule({
  imports: [CommonModule, TB_MODULES],
  declarations: [TB_HOME_COMPONENTS],
  exports: [TB_MODULES, TB_HOME_COMPONENTS]
})
export class TaskbuilderCoreModule { }
