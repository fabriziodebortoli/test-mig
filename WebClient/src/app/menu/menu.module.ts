import { MostUsedComponent } from './components/menu/most-used/most-used.component';
import { TileElementComponent } from './components/menu/tile-element/tile-element.component';
import { HttpMenuService } from './services/http-menu.service';
import { MenuService } from './services/menu.service';
import { ImageService } from './services/image.service';
import { SharedModule } from 'tb-shared';
import { FormsModule } from '@angular/forms';
import { MenuComponent } from './components/menu/menu.component';
import { ApplicationSelectorComponent } from './components/menu/application-selector/application-selector.component';
import { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
import { MenuSelectorComponent } from './components/menu/menu-selector/menu-selector.component';
import { TileContainerComponent } from './components/menu/tile-container/tile-container.component';
import { TileContentComponent } from './components/menu/tile-content/tile-content.component';
import { FavoritesComponent } from './components/menu/favorites/favorites.component';
import { LoginComponent } from './components/login/login.component';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';
import { menuRouting } from './menu.routing';
import { RouterModule, Routes } from '@angular/router';
import { Logger } from 'libclient';

@NgModule({
  imports: [
    CommonModule,
    SharedModule,
    FormsModule,
    MaterialModule.forRoot(),
    menuRouting
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
    MostUsedComponent
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
    MostUsedComponent
  ],
  providers:
  [
    MenuService,
    ImageService,
    HttpMenuService
  ]
})
export class MenuModule {

  constructor(private logger: Logger) {
    this.logger.debug('MenuModule instantiated - ' + Math.round(new Date().getTime() / 1000));

  }
}

