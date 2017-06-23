import { ModuleWithProviders } from '@angular/core';
import { Logger } from '../core';
export * from './services/menu.service';
export * from './services/event-manager.service';
export * from './services/settings.service';
export * from './services/http-menu.service';
export * from './services/image.service';
export * from './services/localization.service';
export * from './components/menu/group-selector/group-selector.component';
export * from './components/menu/menu-stepper/menu-stepper.component';
export * from './components/menu/connection-info-dialog/connection-info-dialog.component';
export * from './components/menu/product-info-dialog/product-info-dialog.component';
export * from './components/menu/search/search.component';
export * from './components/menu/most-used/most-used.component';
export * from './components/menu/menu.component';
export * from './components/menu/application-selector/application-selector.component';
export * from './components/menu/favorites/favorites.component';
export * from './components/login/login.component';
export * from './components/menu/menu-tabber/menu-tabber.component';
export * from './components/menu/menu-tabber/menu-tab/menu-tab.component';
export * from './components/menu/menu-container/menu-container.component';
export * from './components/menu/menu-element/menu-element.component';
export * from './components/menu/menu-content/menu-content.component';
export declare class TbMenuModule {
    private logger;
    static forRoot(): ModuleWithProviders;
    constructor(logger: Logger);
}
