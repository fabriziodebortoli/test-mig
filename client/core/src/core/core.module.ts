import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';

import 'hammerjs';

import { CookieService } from 'angular2-cookie/services/cookies.service';

/**
 * Servizi
 * 
 * Tutti i servizi condivisi TB (http, websocket, 
 */
import { AuthService } from './services/auth.service';
import { AppConfigService } from './services/app-config.service';
import { BOService } from './services/bo.service';
import { BOClient } from './services/bo.service';
import { ComponentService } from './services/component.service';
import { ComponentInfoService } from './services/component-info.service';
import { DataService } from './services/data.service';
import { DocumentService } from './services/document.service';
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
import { UrlService } from './services/url.service';
import { UtilsService } from './services/utils.service';
import { WebSocketService } from './services/websocket.service';
import { TbComponentService } from './services/tbcomponent.service';

export { AuthService } from './services/auth.service';
export { AppConfigService } from './services/app-config.service';
export { TbComponentService } from './services/tbcomponent.service';
export { BOService } from './services/bo.service';
export { BOClient } from './services/bo.service';
export { ComponentService } from './services/component.service';
export { ComponentInfoService } from './services/component-info.service';
export { DataService } from './services/data.service';
export { DocumentService } from './services/document.service';
export { EnumsService } from './services/enums.service';
export { EventDataService } from './services/eventdata.service';
export { ExplorerService } from './services/explorer.service';
export { HttpService } from './services/http.service';
export { InfoService } from './services/info.service';
export { LayoutService } from './services/layout.service';
export { Logger } from './services/logger.service';
export { TaskbuilderService } from './services/taskbuilder.service';
export { SidenavService } from './services/sidenav.service';
export { TabberService } from './services/tabber.service';
export { UrlService } from './services/url.service';
export { UtilsService } from './services/utils.service';
export { WebSocketService } from './services/websocket.service';

export const TB_SERVICES = [
    AppConfigService,
    TbComponentService, BOService, ComponentService, DocumentService, DataService, EnumsService,
    EventDataService, ExplorerService, HttpService, InfoService, LayoutService, Logger, AuthService,
    TaskbuilderService, SidenavService, TabberService, UrlService, UtilsService, WebSocketService
];

import { CoreGuard } from './guards/core.guard';
export { CoreGuard } from './guards/core.guard';
export const TB_GUARDS = [CoreGuard];

@NgModule({
    providers: [CookieService, TB_SERVICES, TB_GUARDS]
})
export class TbCoreModule {
    static forRoot(): ModuleWithProviders {
        return {
            ngModule: TbCoreModule,
            providers: [CookieService, TB_SERVICES, TB_GUARDS]
        };
    }
    constructor( @Optional() @SkipSelf() parentModule: TbCoreModule) {
        if (parentModule) {
            throw new Error(
                'TbCoreModule is already loaded. Import it in the AppModule only');
        }
    }
}
