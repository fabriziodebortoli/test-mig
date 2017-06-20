import { ModuleWithProviders, NgModule, Optional, SkipSelf } from '@angular/core';

/**
 * Servizi
 * 
 * Tutti i servizi condivisi TB (http, websocket, 
 */
import {
    Logger, HttpService, UtilsService, InfoService, LayoutService, SidenavService, TabberService
    // BOService, BOHelperService, ComponentService, DataService, DocumentService, EnumsService, EventDataService,
    // ExplorerService, HttpService, InfoService, LayoutService, Logger, LoginSessionService, SidenavService,
    // TabberService, UtilsService, WebSocketService
} from './services';

// export * from './services/bo.service';
// export * from './services/bohelper.service';
// export * from './services/component.service';
// export * from './services/data.service';
// export * from './services/document.service';
// export * from './services/enums.service';
// export * from './services/eventdata.service';
// export * from './services/explorer.service';
export * from './services/http.service';
export * from './services/info.service';
export * from './services/layout.service';
export * from './services/logger.service';
// export * from './services/login-session.service';
export * from './services/sidenav.service';
export * from './services/tabber.service';
export * from './services/url.service';
export * from './services/utils.service';
// export * from './services/websocket.service';

export const TB_SERVICES = [
    Logger, HttpService, UtilsService, InfoService, LayoutService, SidenavService, TabberService
    // BOService, BOHelperService, ComponentService, DataService, DocumentService, EnumsService, EventDataService,
    // ExplorerService, HttpService, InfoService, LayoutService, Logger, LoginSessionService, SidenavService,
    // TabberService, UtilsService, WebSocketService
];

@NgModule({
    providers: [TB_SERVICES]
})
export class TbCoreModule {
    static forRoot(): ModuleWithProviders {
        return {
            ngModule: TbCoreModule,
            providers: [TB_SERVICES]
        };
    }
    constructor( @Optional() @SkipSelf() parentModule: TbCoreModule) {
        if (parentModule) {
            throw new Error(
                'TbCoreModule is already loaded. Import it in the AppModule only');
        }
    }
}
