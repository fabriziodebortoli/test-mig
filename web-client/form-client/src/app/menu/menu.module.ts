import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';
import { RouterModule, Routes } from '@angular/router';
import { BrowserModule } from '@angular/platform-browser';

import { SharedModule } from '../shared/shared.module';

import { SearchComponent } from './components/menu/search/search.component';
import { HiddenTilesComponent } from './components/menu/hidden-tiles/hidden-tiles.component';
import { ConnectionInfoDialogComponent } from './components/menu/connection-info-dialog/connection-info-dialog.component';
import { ProductInfoDialogComponent } from './components/menu/product-info-dialog/product-info-dialog.component';
import { RightSidenavComponent } from './components/menu/sidenav-right-content/sidenav-right-content.component';
import { LeftSidenavComponent } from './components/menu/sidenav-left-content/sidenav-left-content.component';
import { LocalizationService } from './services/localization.service';
import { MostUsedComponent } from './components/menu/most-used/most-used.component';
import { TileElementComponent } from './components/menu/tile-element/tile-element.component';
import { MenuComponent } from './components/menu/menu.component';
import { ApplicationSelectorComponent } from './components/menu/application-selector/application-selector.component';
import { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
import { MenuSelectorComponent } from './components/menu/menu-selector/menu-selector.component';
import { TileContainerComponent } from './components/menu/tile-container/tile-container.component';
import { TileContentComponent } from './components/menu/tile-content/tile-content.component';
import { FavoritesComponent } from './components/menu/favorites/favorites.component';
import { LoginComponent } from './components/login/login.component';

import { HttpMenuService } from './services/http-menu.service';
import { MenuService } from './services/menu.service';
import { ImageService } from './services/image.service';
import { EventManagerService } from './services/event-manager.service';
import { SettingsService } from './services/settings.service';

import { Logger } from 'libclient';

import { menuRouting } from './menu.routing';

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    FormsModule,
    MaterialModule.forRoot(),
    menuRouting,
    BrowserModule
  ],

  declarations:
  [
    LoginComponent,
    MenuComponent,
    ApplicationSelectorComponent,
    GroupSelectorComponent,
    MenuSelectorComponent,
    TileContainerComponent,
    TileContentComponent,
    FavoritesComponent,
    TileElementComponent,
    MostUsedComponent,
    LeftSidenavComponent,
    RightSidenavComponent,
    ProductInfoDialogComponent,
    ConnectionInfoDialogComponent,
    HiddenTilesComponent,
    SearchComponent
  ],
  exports:
  [
    RouterModule,
    LoginComponent,
    MenuComponent,
    ApplicationSelectorComponent,
    GroupSelectorComponent,
    MenuSelectorComponent,
    TileContainerComponent,
    TileContentComponent,
    FavoritesComponent,
    TileElementComponent,
    MostUsedComponent,
    LeftSidenavComponent,
    RightSidenavComponent,
    HiddenTilesComponent,
    SearchComponent
  ],
  providers:
  [
    MenuService,
    ImageService,
    HttpMenuService,
    SettingsService,
    LocalizationService,
    EventManagerService
  ],
  entryComponents: [
    ProductInfoDialogComponent,
    ConnectionInfoDialogComponent
  ]

})
export class MenuModule {

  constructor(private logger: Logger) {
    this.logger.debug('MenuModule instantiated - ' + Math.round(new Date().getTime() / 1000));

  }
}

