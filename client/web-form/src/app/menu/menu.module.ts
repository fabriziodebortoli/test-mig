import { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
import { MenuStepperComponent } from './components/menu/menu-stepper/menu-stepper.component';
import { MenuService } from './services/menu.service';
import { ConnectionInfoDialogComponent } from './components/menu/connection-info-dialog/connection-info-dialog.component';
import { ProductInfoDialogComponent } from './components/menu/product-info-dialog/product-info-dialog.component';
import { EventManagerService } from './services/event-manager.service';
import { SettingsService } from './services/settings.service';
import { HttpMenuService } from './services/http-menu.service';
import { ImageService } from './services/image.service';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';
import { RouterModule } from '@angular/router';

import { SharedModule } from '../shared/shared.module';

import { SearchComponent } from './components/menu/search/search.component';
import { LocalizationService } from './services/localization.service';
import { MostUsedComponent } from './components/menu/most-used/most-used.component';
import { TileElementComponent } from './components/menu/tile-element/tile-element.component';
import { MenuComponent } from './components/menu/menu.component';
import { ApplicationSelectorComponent } from './components/menu/application-selector/application-selector.component';
import { MenuContainerComponent } from './components/menu/menu-container/menu-container.component';
import { FavoritesComponent } from './components/menu/favorites/favorites.component';

import { LoginComponent } from './components/login/login.component';
import { Logger } from 'libclient';
import { Accordion, AccordionGroup } from '../shared/containers/accordion/accordion.component';
import { MenuTabberComponent } from './components/menu/menu-tabber/menu-tabber.component';
import { MenuTabComponent } from './components/menu/menu-tabber/menu-tab/menu-tab.component';

//WARNING: put here only GLOBAL services, NOT component level services
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
    CommonModule,
    SharedModule,
    FormsModule,
    MaterialModule.forRoot()
  ],

  declarations:
  [
    LoginComponent,
    MenuComponent,
    ApplicationSelectorComponent,
    MenuContainerComponent,
    FavoritesComponent,
    TileElementComponent,
    MostUsedComponent,
    SearchComponent,
    ProductInfoDialogComponent,
    ConnectionInfoDialogComponent,
    GroupSelectorComponent,
    MenuStepperComponent,
    Accordion,
    AccordionGroup,
    MenuTabberComponent,
    MenuTabComponent
  ],
  exports:
  [
    RouterModule,
    LoginComponent,
    MenuComponent,
    ApplicationSelectorComponent,
    MenuContainerComponent,
    FavoritesComponent,
    TileElementComponent,
    MostUsedComponent,
    SearchComponent,
    GroupSelectorComponent,
    MenuStepperComponent,
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
export class MenuModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: MenuModule,
      providers: [MENU_SERVICES]
    };
  }

  constructor(private logger: Logger) {
    this.logger.debug('MenuModule instantiated - ' + Math.round(new Date().getTime() / 1000));

  }
}

