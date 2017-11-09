import { TbSharedModule } from './../shared/shared.module';
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
import { TbComponentServiceParams } from './services/tbcomponent.service.params';
import { LocalizationService } from './services/localization.service';
import { LoadingService } from './services/loading.service';
import { DiagnosticService } from './services/diagnostic.service';
import { SettingsService } from './services/settings.service';
import { EventManagerService } from './services/event-manager.service';
import { Store } from './services/store.service';
import { ThemeService } from './services/theme.service';

export { AuthService } from './services/auth.service';
export { TbComponentService } from './services/tbcomponent.service';
export { TbComponentServiceParams } from './services/tbcomponent.service.params';
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
    TbComponentService, TbComponentServiceParams, BOService, ComponentService, DocumentService, DataService, EasystudioService, EnumsService,
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
import { ArcticTheme } from './themes/arctic/arctic-theme.component';
import { BorealisTheme } from './themes/borealis/borealis-theme.component';
import { DefaultTheme } from './themes/default/default-theme.component';
import { M4Theme } from './themes/m4/m4-theme.component';
import { InfinityTheme } from './themes/infinity/infinity-theme.component';
import { LakeTheme } from './themes/lake/lake-theme.component';
import { OceanTheme } from './themes/ocean/ocean-theme.component';
import { PurpleTheme } from './themes/purple/purple-theme.component';
import { SnowFlakeTheme } from './themes/snowflake/snowflake-theme.component';
import { UnderwaterTheme } from './themes/underwater/underwater-theme.component';



const THEME_COMPONENTS = [
    ArcticTheme, BorealisTheme, DefaultTheme, M4Theme, InfinityTheme, LakeTheme, OceanTheme, PurpleTheme, SnowFlakeTheme, UnderwaterTheme
];

/**
 * Pagine informative (404, ServerDown, Landing, ecc)
 */
import { ServerDownPage } from './pages/server-down/server-down.page';
export { ServerDownPage } from './pages/server-down/server-down.page';
const TB_PAGES = [
    ServerDownPage
];

@NgModule({
    imports: [
        TbSharedModule,
        RouterModule.forChild([

            { path: 'arctic', component: ArcticTheme, outlet: 'theme' },
            { path: 'borealis', component: BorealisTheme, outlet: 'theme' },
            { path: 'default', component: DefaultTheme, outlet: 'theme' },
            { path: 'm4', component: M4Theme, outlet: 'theme' },
            { path: 'infinity', component: InfinityTheme, outlet: 'theme' },
            { path: 'lake', component: LakeTheme, outlet: 'theme' },
            { path: 'ocean', component: OceanTheme, outlet: 'theme' },
            { path: 'purple', component: PurpleTheme, outlet: 'theme' },
            { path: 'snowflake', component: SnowFlakeTheme, outlet: 'theme' },
            { path: 'underwater', component: UnderwaterTheme, outlet: 'theme' },
            { path: 'server-down', component: ServerDownPage },

        ]),
        HttpModule
    ],
    providers: [TB_SERVICES, TB_GUARDS],
    declarations: [THEME_COMPONENTS, TB_PAGES],
    exports: [TB_PAGES],
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
