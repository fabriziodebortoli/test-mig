import { NgModule, ModuleWithProviders } from '@angular/core';

import { TbSharedModule } from '../shared/shared.module';
import { Logger } from '../core';

/**
 * Menu Services
 */
import { MenuService } from './services/menu.service';
import { EventManagerService } from './services/event-manager.service';
import { SettingsService } from './services/settings.service';
import { HttpMenuService } from './services/http-menu.service';
import { ImageService } from './services/image.service';
import { LocalizationService } from './services/localization.service';

export * from './services/menu.service';
export * from './services/event-manager.service';
export * from './services/settings.service';
export * from './services/http-menu.service';
export * from './services/image.service';
export * from './services/localization.service';

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


// WARNING: put here only GLOBAL services, NOT component level services
const MENU_SERVICES = [
  MenuService,
  ImageService,
  HttpMenuService,
  SettingsService,
  LocalizationService,
  EventManagerService
];

@NgModule({
  imports: [
    TbSharedModule
  ],
  declarations:
  [
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
  exports:
  [
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
  ]
  ,
  providers: [MENU_SERVICES],
  entryComponents: [
    ProductInfoDialogComponent,
    ConnectionInfoDialogComponent
  ]
})
export class TbMenuModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: TbMenuModule,
      providers: [MENU_SERVICES]
    };
  }

  constructor(private logger: Logger) {
    this.logger.debug('TbMenuModule instantiated - ' + Math.round(new Date().getTime() / 1000));
  }
}

