import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

/**
 * Modulo Core con tutti i principali servizi e componenti di TB
 */
import { TbCoreModule, TB_SERVICES } from './core/core.module';
export * from './core/core.module';

/**
 * Metodo da richiamare in app.module per lettura parametri configurazione
 */
export { loadConfig } from './core/services/info.service';

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
  // TbCoreModule,
  TbSharedModule,
  TbMenuModule,
  TbDashboardModule,
  TbTestModule,
  TbSettingsModule
];


/** 
 * Models & Enums
 */
export { ComponentInfo } from './shared/models/component-info.model';
export { ContextMenuItem } from './shared/models/context-menu-item.model';
export { ControlTypes } from './shared/models/control-types.enum';
export { CommandEventArgs } from './shared/models/eventargs.model';
export { LoginCompact } from './shared/models/login-compact.model';
export { LoginSession } from './shared/models/login-session.model';
export { MessageDlgArgs, MessageDlgResult, DiagnosticData, Message, DiagnosticDlgResult, DiagnosticType } from './shared/models/message-dialog.model';
export { OperationResult } from './shared/models/operation-result.model';
export { StateButton } from './shared/models/state-button.model';
export { ViewModeType } from './shared/models/view-mode-type.model';
export { SocketConnectionStatus } from './shared/models/websocket-connection.enum';

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

const TB_HOME_COMPONENTS = [HomeComponent, HomeSidenavComponent, StandaloneReportComponent, StandaloneDocumentComponent];

import { LayoutModule } from '@progress/kendo-angular-layout';

@NgModule({
  imports: [CommonModule, TB_MODULES, LayoutModule],
  declarations: [TB_HOME_COMPONENTS],
  exports: [TB_MODULES, TB_HOME_COMPONENTS]
})
export class TaskbuilderCoreModule { }