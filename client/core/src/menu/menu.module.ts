import { NgModule, ModuleWithProviders } from '@angular/core';

import { TbSharedModule } from '../shared/shared.module';
import { TbCoreModule } from '../core/core.module';

import { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
import { MenuStepperComponent } from './components/menu/menu-stepper/menu-stepper.component';
import { MenuService } from './services/menu.service';
import { ConnectionInfoDialogComponent } from './components/menu/connection-info-dialog/connection-info-dialog.component';
import { ProductInfoDialogComponent } from './components/menu/product-info-dialog/product-info-dialog.component';
import { EventManagerService } from './services/event-manager.service';
import { SettingsService } from './services/settings.service';
import { HttpMenuService } from './services/http-menu.service';
import { ImageService } from './services/image.service';

import { SearchComponent } from './components/menu/search/search.component';
import { LocalizationService } from './services/localization.service';
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

import { Logger } from '../core';
import { Accordion, AccordionGroup } from '../shared';

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
    TbCoreModule,
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
    Accordion,
    AccordionGroup,
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
    MenuContentComponent,
    Accordion,
    AccordionGroup
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

