
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
import { FormattersService } from './services/formatters.service';
import { EventDataService } from './services/eventdata.service';
import { ExplorerService } from './services/explorer.service';
import { HttpService } from './services/http.service';
import { InfoService } from './services/info.service';
import { LayoutService } from './services/layout.service';
import { Logger } from './services/logger.service';
import { TaskBuilderService } from './services/taskbuilder.service';
import { SidenavService } from './services/sidenav.service';
import { TabberService } from './services/tabber.service';
import { UtilsService } from './services/utils.service';
import { WebSocketService } from './services/websocket.service';
import { TbComponentService } from './services/tbcomponent.service';
import { TbComponentServiceParams } from './services/tbcomponent.service.params';
import { BOServiceParams } from './services/bo.service.params';
import { LoadingService } from './services/loading.service';
import { DiagnosticService } from './services/diagnostic.service';
import { SettingsService } from './services/settings.service';
import { EventManagerService } from './services/event-manager.service';
import { Store } from './services/store.service';
import { ThemeService } from './services/theme.service';
import { ParameterService } from './services/parameters.service';
import { PaginatorService } from './services/paginator.service';
import { FilterService} from './services/filter.service';
import { HyperLinkService} from './services/hyperlink.service';
import { ComponentMediator } from './services/component-mediator.service';
import { StorageService } from './services/storage.service';
import { BodyEditService } from './services/body-edit.service';
import { RsSnapshotService } from './services/rs-snapshot.service';
import { ActivationService } from './services/activation.service';

export { AuthService } from './services/auth.service';
export { TbComponentService } from './services/tbcomponent.service';
export { TbComponentServiceParams } from './services/tbcomponent.service.params';
export { BOServiceParams } from './services/bo.service.params';
export { BOService } from './services/bo.service';
export { BOClient } from './services/bo.service';
export { ComponentService } from './services/component.service';
export { ComponentInfoService } from './services/component-info.service';
export { DataService } from './services/data.service';
export { DocumentService } from './services/document.service';
export { EasystudioService } from './services/easystudio.service';
export { EnumsService } from './services/enums.service';
export { FormattersService } from './services/formatters.service';
export { EventDataService } from './services/eventdata.service';
export { ExplorerService } from './services/explorer.service';
export { HttpService } from './services/http.service';
export { InfoService } from './services/info.service';
export { LayoutService } from './services/layout.service';
export { Logger } from './services/logger.service';
export { TaskBuilderService } from './services/taskbuilder.service';
export { SidenavService } from './services/sidenav.service';
export { TabberService } from './services/tabber.service';
export { UtilsService } from './services/utils.service';
export { WebSocketService } from './services/websocket.service';
export { LoadingService } from './services/loading.service';
export { DiagnosticService } from './services/diagnostic.service';
export { SettingsService } from './services/settings.service';
export { EventManagerService } from './services/event-manager.service';
export { Store } from './services/store.service';
export { ThemeService } from './services/theme.service';
export { ParameterService } from './services/parameters.service';
export { PaginatorService } from './services/paginator.service';
export { FilterService} from './services/filter.service';
export { HyperLinkService} from './services/hyperlink.service';
export { StorageService } from './services/storage.service';
export { ComponentMediator } from './services/component-mediator.service';
export { BodyEditService } from './services/body-edit.service';
export { RsSnapshotService } from './services/rs-snapshot.service';
export { ActivationService } from './services/activation.service';

export const TB_SERVICES = [
    TbComponentService, TbComponentServiceParams, BOService, BOServiceParams, ComponentService, DocumentService, DataService, EasystudioService,
    EnumsService, ParameterService,
    FormattersService, EventDataService, ExplorerService, HttpService, InfoService, LayoutService, Logger, AuthService,
    TaskBuilderService, SidenavService, TabberService, UtilsService, WebSocketService, ThemeService,
    LoadingService, DiagnosticService, SettingsService, EventManagerService, Store, StorageService, BodyEditService, RsSnapshotService,
    ActivationService
];

import { CoreGuard } from './guards/core.guard';
export { CoreGuard } from './guards/core.guard';
export const TB_GUARDS = [CoreGuard];

import { HttpModule } from '@angular/http';

/**
 * Themes
 */
import { BorealisTheme } from './themes/borealis/borealis-theme.component';
import { DefaultTheme } from './themes/default/default-theme.component';
import { InfinityTheme } from './themes/infinity/infinity-theme.component';

const THEME_COMPONENTS = [    BorealisTheme, DefaultTheme, InfinityTheme];

/**
 * Pagine informative (404, ServerDown, Landing, ecc)
 */
import { ServerDownPage } from './pages/server-down/server-down.page';
export { ServerDownPage } from './pages/server-down/server-down.page';
const TB_PAGES = [
    ServerDownPage
];

/**
 * Culture
 */
import { LOCALE_ID } from '@angular/core';
import { registerLocaleData } from '@angular/common';

import localebg from '@angular/common/locales/bg';
import localedech from '@angular/common/locales/de-CH';
import localeel from '@angular/common/locales/el';
import localeen from '@angular/common/locales/en';
import localeescl from '@angular/common/locales/es-CL';
import localehu from '@angular/common/locales/hu';
import localeit from '@angular/common/locales/it';
import localeitch from '@angular/common/locales/it-CH';
import localepl from '@angular/common/locales/pl';
import localero from '@angular/common/locales/ro';
import localesi from '@angular/common/locales/si';
import localetr from '@angular/common/locales/tr';
import localept from '@angular/common/locales/pt';
import localezh from '@angular/common/locales/zh';
import localehr from '@angular/common/locales/hr';
import localede from '@angular/common/locales/de';
import localees from '@angular/common/locales/es';
import localefr from '@angular/common/locales/fr';
import localesr from '@angular/common/locales/sr';
import localesrcyrl from '@angular/common/locales/sr-Cyrl';
import localesrlatn from '@angular/common/locales/sr-Latn';
import localesl from '@angular/common/locales/sl';
import localeru from '@angular/common/locales/ru';

const culture = localStorage.getItem('ui_culture') ? localStorage.getItem('ui_culture') : 'en-EN';
registerLocaleData(findLocaleData(culture));
findLocaleData(culture);
export function findLocaleData(locale: string): any {
    if (!locale)
        return localeit;
    const normalizedLocale = locale.toLowerCase().replace(/_/g, '-');
    switch (normalizedLocale) {
        case 'de-ch': return localedech;
        case 'es-cl': return localeescl;
        case 'it-ch': return localeitch;
        case 'sr-cyrl': return localesrcyrl;
        case 'sr-latn': return localesrlatn;
    }

    const parentLocale = normalizedLocale.split('-')[0];
    switch (parentLocale) {
        case 'bg': return localebg;
        case 'el': return localeel;
        case 'en': return localeen;
        case 'hu': return localehu;
        case 'it': return localeit;
        case 'pl': return localepl;
        case 'ro': return localero;
        case 'si': return localesi;
        case 'tr': return localetr;
        case 'pt': return localept;
        case 'zh': return localezh;
        case 'hr': return localehr;
        case 'de': return localede;
        case 'es': return localees;
        case 'fr': return localefr;
        case 'sr': return localesr;
        case 'sl': return localesl;
        case 'ru': return localeru;
    }

    return localeit;
}
@NgModule({
    imports: [
        TbSharedModule,

        RouterModule.forChild([
            // In futuro è possibile fare un refactor creando un modulo per ogni tema caricabile in lazy loading
            { path: 'borealis', component: BorealisTheme, outlet: 'theme' },
            { path: 'default', component: DefaultTheme, outlet: 'theme' },
            { path: 'infinity', component: InfinityTheme, outlet: 'theme' },

            { path: 'server-down', component: ServerDownPage }
        ]),
        HttpModule
    ],
    providers: [
        TB_SERVICES,
        TB_GUARDS,
        { provide: LOCALE_ID, useValue: culture }
    ],
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
