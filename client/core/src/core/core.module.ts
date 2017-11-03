import { RouterModule } from '@angular/router';
import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';

import 'hammerjs';

/**
 * Servizi
 * 
 * Tutti i servizi condivisi TB (http, websocket, 
 */
import { AuthService } from './services/auth.service';
import { BOService } from './services/bo.service';
import { BOClient } from './services/bo.service';
import { ComponentService } from './services/component.service';
import { ComponentInfoService } from './services/component-info.service';
import { DataService } from './services/data.service';
import { DocumentService } from './services/document.service';
import { EasystudioService } from './services/easystudio.service';
import { EnumsService } from './services/enums.service';
import { EventDataService } from './services/eventdata.service';
import { ExplorerService } from './services/explorer.service';
import { HttpService } from './services/http.service';
import { InfoService } from './services/info.service';
import { LayoutService } from './services/layout.service';
import { Logger } from './services/logger.service';
import { TaskbuilderService } from './services/taskbuilder.service';
import { SidenavService } from './services/sidenav.service';
import { TabberService } from './services/tabber.service';
import { UtilsService } from './services/utils.service';
import { WebSocketService } from './services/websocket.service';
import { TbComponentService } from './services/tbcomponent.service';
import { LocalizationService } from './services/localization.service';
import { LoadingService } from './services/loading.service';
import { DiagnosticService } from './services/diagnostic.service';
import { SettingsService } from './services/settings.service';
import { EventManagerService } from './services/event-manager.service';
import { Store } from './services/store.service';
import { ThemeService } from './services/theme.service';

export { AuthService } from './services/auth.service';
export { TbComponentService } from './services/tbcomponent.service';
export { BOService } from './services/bo.service';
export { BOClient } from './services/bo.service';
export { ComponentService } from './services/component.service';
export { ComponentInfoService } from './services/component-info.service';
export { DataService } from './services/data.service';
export { DocumentService } from './services/document.service';
export { EasystudioService } from './services/easystudio.service';
export { EnumsService } from './services/enums.service';
export { EventDataService } from './services/eventdata.service';
export { ExplorerService } from './services/explorer.service';
export { HttpService } from './services/http.service';
export { InfoService } from './services/info.service';
export { loadConfig } from './services/info.service';
export { LayoutService } from './services/layout.service';
export { Logger } from './services/logger.service';
export { TaskbuilderService } from './services/taskbuilder.service';
export { SidenavService } from './services/sidenav.service';
export { TabberService } from './services/tabber.service';
export { UtilsService } from './services/utils.service';
export { WebSocketService } from './services/websocket.service';
export { LocalizationService } from './services/localization.service';
export { LoadingService } from './services/loading.service';
export { DiagnosticService } from './services/diagnostic.service';
export { SettingsService } from './services/settings.service';
export { EventManagerService } from './services/event-manager.service';
export { Store } from './services/store.service';
export { ThemeService } from './services/theme.service';

export const TB_SERVICES = [
    TbComponentService, BOService, ComponentService, DocumentService, DataService, EasystudioService, EnumsService,
    EventDataService, ExplorerService, HttpService, InfoService, LayoutService, Logger, AuthService,
    TaskbuilderService, SidenavService, TabberService, UtilsService, WebSocketService, ThemeService,
    LocalizationService, LoadingService, DiagnosticService, SettingsService, EventManagerService, Store
];

import { CoreGuard } from './guards/core.guard';
export { CoreGuard } from './guards/core.guard';
export const TB_GUARDS = [CoreGuard];

import { HttpModule } from '@angular/http';

/**
 * Themes
 */
import { DarculaTheme } from './themes/darcula/darcula-theme.component';
import { ResetTheme } from './themes/reset/reset-theme.component';
const THEME_COMPONENTS = [
    DarculaTheme, ResetTheme
];

@NgModule({
    imports: [
        RouterModule.forChild([
            { path: 'darcula', component: DarculaTheme, outlet: 'theme' },
            { path: 'reset', component: ResetTheme, outlet: 'theme' },
        ]),
        HttpModule
    ],
    providers: [TB_SERVICES, TB_GUARDS],
    declarations: [THEME_COMPONENTS],
    entryComponents: [THEME_COMPONENTS]
})
export class TbCoreModule {
    static forRoot(): ModuleWithProviders {
        return {
            ngModule: TbCoreModule,
            providers: [TB_SERVICES, TB_GUARDS]
        };
    }
    constructor( @Optional() @SkipSelf() parentModule: TbCoreModule) {
        if (parentModule) {
            throw new Error(
                'TbCoreModule is already loaded. Import it in the AppModule only');
        }
    }
}
