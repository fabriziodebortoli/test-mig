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
import { RouterModule, Routes } from '@angular/router';
import { BrowserModule } from '@angular/platform-browser';

import { SharedModule } from '../shared/shared.module';

import { SearchComponent } from './components/menu/search/search.component';
import { HiddenTilesComponent } from './components/menu/hidden-tiles/hidden-tiles.component';
import { LocalizationService } from './services/localization.service';
import { MostUsedComponent } from './components/menu/most-used/most-used.component';
import { TileElementComponent } from './components/menu/tile-element/tile-element.component';
import { MenuComponent } from './components/menu/menu.component';
import { ApplicationSelectorComponent } from './components/menu/application-selector/application-selector.component';
import { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
import { MenuContainerComponent } from './components/menu/menu-container/menu-container.component';
import { TileContainerComponent } from './components/menu/tile-container/tile-container.component';
import { TileContentComponent } from './components/menu/tile-content/tile-content.component';
import { FavoritesComponent } from './components/menu/favorites/favorites.component';
import { LoginComponent } from './components/login/login.component';
import { Logger } from 'libclient';


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
    MaterialModule.forRoot(),
    BrowserModule
  ],

  declarations:
  [
    LoginComponent,
    MenuComponent,
    ApplicationSelectorComponent,
    GroupSelectorComponent,
    MenuContainerComponent,
    TileContainerComponent,
    TileContentComponent,
    FavoritesComponent,
    TileElementComponent,
    MostUsedComponent,
    HiddenTilesComponent,
    SearchComponent,
    ProductInfoDialogComponent,
    ConnectionInfoDialogComponent
  ],
  exports:
  [
    RouterModule,
    LoginComponent,
    MenuComponent,
    ApplicationSelectorComponent,
    GroupSelectorComponent,
    MenuContainerComponent,
    TileContainerComponent,
    TileContentComponent,
    FavoritesComponent,
    TileElementComponent,
    MostUsedComponent,
    HiddenTilesComponent,
    SearchComponent
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

