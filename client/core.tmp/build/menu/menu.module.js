import { NgModule } from '@angular/core';
import { TbSharedModule } from '../shared/shared.module';
import { Logger } from '../core/index';
/**
 * Menu Services
 */
import { MenuService } from './services/menu.service';
import { EventManagerService } from './services/event-manager.service';
import { SettingsService } from './services/settings.service';
import { HttpMenuService } from './services/http-menu.service';
import { ImageService } from './services/image.service';
import { LocalizationService } from './services/localization.service';
export { MenuService } from './services/menu.service';
export { EventManagerService } from './services/event-manager.service';
export { SettingsService } from './services/settings.service';
export { HttpMenuService } from './services/http-menu.service';
export { ImageService } from './services/image.service';
export { LocalizationService } from './services/localization.service';
/**
 * Menu Components
 */
import { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
import { MenuStepperComponent } from './components/menu/menu-stepper/menu-stepper.component';
import { ConnectionInfoDialogComponent } from './components/menu/connection-info-dialog/connection-info-dialog.component';
import { ProductInfoDialogComponent } from './components/menu/product-info-dialog/product-info-dialog.component';
import { SearchComponent } from './components/menu/search/search.component';
import { MostUsedComponent } from './components/menu/most-used/most-used.component';
import { MenuComponent } from './components/menu/menu.component';
import { ApplicationSelectorComponent } from './components/menu/application-selector/application-selector.component';
import { FavoritesComponent } from './components/menu/favorites/favorites.component';
import { LoginComponent } from './components/login/login.component';
import { MenuTabberComponent } from './components/menu/menu-tabber/menu-tabber.component';
import { MenuTabComponent } from './components/menu/menu-tabber/menu-tab/menu-tab.component';
import { MenuContainerComponent } from './components/menu/menu-container/menu-container.component';
import { MenuElementComponent } from './components/menu/menu-element/menu-element.component';
import { MenuContentComponent } from './components/menu/menu-content/menu-content.component';
export { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
export { MenuStepperComponent } from './components/menu/menu-stepper/menu-stepper.component';
export { ConnectionInfoDialogComponent } from './components/menu/connection-info-dialog/connection-info-dialog.component';
export { ProductInfoDialogComponent } from './components/menu/product-info-dialog/product-info-dialog.component';
export { SearchComponent } from './components/menu/search/search.component';
export { MostUsedComponent } from './components/menu/most-used/most-used.component';
export { MenuComponent } from './components/menu/menu.component';
export { ApplicationSelectorComponent } from './components/menu/application-selector/application-selector.component';
export { FavoritesComponent } from './components/menu/favorites/favorites.component';
export { LoginComponent } from './components/login/login.component';
export { MenuTabberComponent } from './components/menu/menu-tabber/menu-tabber.component';
export { MenuTabComponent } from './components/menu/menu-tabber/menu-tab/menu-tab.component';
export { MenuContainerComponent } from './components/menu/menu-container/menu-container.component';
export { MenuElementComponent } from './components/menu/menu-element/menu-element.component';
export { MenuContentComponent } from './components/menu/menu-content/menu-content.component';
// WARNING: put here only GLOBAL services, NOT component level services
const /** @type {?} */ MENU_SERVICES = [
    MenuService,
    ImageService,
    HttpMenuService,
    SettingsService,
    LocalizationService,
    EventManagerService
];
export class TbMenuModule {
    /**
     * @param {?} logger
     */
    constructor(logger) {
        this.logger = logger;
        this.logger.debug('TbMenuModule instantiated - ' + Math.round(new Date().getTime() / 1000));
    }
    /**
     * @return {?}
     */
    static forRoot() {
        return {
            ngModule: TbMenuModule,
            providers: [MENU_SERVICES]
        };
    }
}
TbMenuModule.decorators = [
    { type: NgModule, args: [{
                imports: [
                    TbSharedModule
                ],
                declarations: [
                    LoginComponent,
                    MenuComponent,
                    ApplicationSelectorComponent,
                    MenuContainerComponent,
                    FavoritesComponent,
                    MostUsedComponent,
                    SearchComponent,
                    ProductInfoDialogComponent,
                    ConnectionInfoDialogComponent,
                    GroupSelectorComponent,
                    MenuStepperComponent,
                    MenuTabberComponent,
                    MenuTabComponent,
                    MenuElementComponent,
                    MenuContentComponent
                ],
                exports: [
                    LoginComponent,
                    MenuComponent,
                    MenuElementComponent,
                    ApplicationSelectorComponent,
                    MenuContainerComponent,
                    FavoritesComponent,
                    MostUsedComponent,
                    SearchComponent,
                    GroupSelectorComponent,
                    MenuStepperComponent,
                    MenuContentComponent
                ],
                providers: [MENU_SERVICES],
                entryComponents: [
                    ProductInfoDialogComponent,
                    ConnectionInfoDialogComponent
                ]
            },] },
];
/**
 * @nocollapse
 */
TbMenuModule.ctorParameters = () => [
    { type: Logger, },
];
function TbMenuModule_tsickle_Closure_declarations() {
    /** @type {?} */
    TbMenuModule.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    TbMenuModule.ctorParameters;
    /** @type {?} */
    TbMenuModule.prototype.logger;
}
