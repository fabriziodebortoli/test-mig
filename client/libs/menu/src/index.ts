import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { TbSharedModule, Logger } from '@taskbuilder/core';

/** 
 * HOME 
 */
import { StandaloneDocumentComponent } from './components/home/standalone.document/standalone.document.component';
import { StandaloneReportComponent } from './components/home/standalone.report/standalone.report.component';
import { HomeSidenavLeftComponent } from './components/home/home-sidenav-left/home-sidenav-left.component';

import { HomeComponent } from './components/home/home.component';

export { StandaloneDocumentComponent } from './components/home/standalone.document/standalone.document.component';
export { StandaloneReportComponent } from './components/home/standalone.report/standalone.report.component';
export { HomeSidenavLeftComponent } from './components/home/home-sidenav-left/home-sidenav-left.component';
export { HomeComponent } from './components/home/home.component';

const TB_HOME_COMPONENTS = [
  HomeComponent, HomeSidenavLeftComponent,
  StandaloneDocumentComponent,
  StandaloneReportComponent
];


/**
 * Modulo Settings
 */
import { TbSettingsModule } from './settings/settings.module';
export * from './settings/settings.module';




import { LoginComponent } from './components/login/login.component';
import { ApplicationSelectorComponent } from './components/menu/application-selector/application-selector.component';
import { FavoritesComponent } from './components/menu/favorites/favorites.component';
import { SearchComponent } from './components/menu/search/search.component';
import { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
import { MenuComponent } from './components/menu/menu.component';
import { MenuContainerComponent } from './components/menu/menu-container/menu-container.component';
import { MenuContentComponent } from './components/menu/menu-content/menu-content.component';
import { MenuElementComponent } from './components/menu/menu-element/menu-element.component';
import { ItemCustomizationsDropdownComponent } from './components/menu/menu-element/item-customizations-dropdown/item-customizations-dropdown.component';
import { MenuStepperComponent } from './components/menu/menu-stepper/menu-stepper.component';
import { MenuTabberComponent } from './components/menu/menu-tabber/menu-tabber.component';
import { MenuTabComponent } from './components/menu/menu-tabber/menu-tab/menu-tab.component';
import { MostUsedComponent } from './components/menu/most-used/most-used.component';
import { HiddenTilesComponent } from './components/menu/hidden-tiles/hidden-tiles.component';

export { LoginComponent } from './components/login/login.component';
export { ApplicationSelectorComponent } from './components/menu/application-selector/application-selector.component';
export { FavoritesComponent } from './components/menu/favorites/favorites.component';
export { SearchComponent } from './components/menu/search/search.component';
export { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
export { MenuComponent } from './components/menu/menu.component';
export { MenuContainerComponent } from './components/menu/menu-container/menu-container.component';
export { MenuContentComponent } from './components/menu/menu-content/menu-content.component';
export { MenuElementComponent } from './components/menu/menu-element/menu-element.component';
export { ItemCustomizationsDropdownComponent } from './components/menu/menu-element/item-customizations-dropdown/item-customizations-dropdown.component';
export { MenuStepperComponent } from './components/menu/menu-stepper/menu-stepper.component';
export { MenuTabberComponent } from './components/menu/menu-tabber/menu-tabber.component';
export { MenuTabComponent } from './components/menu/menu-tabber/menu-tab/menu-tab.component';
export { MostUsedComponent } from './components/menu/most-used/most-used.component';
export { HiddenTilesComponent } from './components/menu/hidden-tiles/hidden-tiles.component';

export const TB_MENU_COMPONENTS = [
  LoginComponent,
  ApplicationSelectorComponent,
  FavoritesComponent,
  MostUsedComponent,
  GroupSelectorComponent,
  MenuContainerComponent,
  MenuContentComponent,
  MenuElementComponent,
  ItemCustomizationsDropdownComponent,
  MenuStepperComponent,
  SearchComponent,
  MenuComponent,
  MenuTabberComponent,
  MenuTabComponent,
  HiddenTilesComponent
];

import { MenuService } from './services/menu.service';
import { HttpMenuService } from './services/http-menu.service';
import { ImageService } from './services/image.service';


//WARNING: put here only GLOBAL services, NOT component level services
export const TB_MENU_SERVICES = [
  MenuService,
  ImageService,
  HttpMenuService
];

export { MenuService } from './services/menu.service';
export { ImageService } from './services/image.service';
export { HttpMenuService } from './services/http-menu.service';

const NG_MODULES = [
  CommonModule,
  ReactiveFormsModule,
  FormsModule
];


@NgModule({
  imports: [
    NG_MODULES,
    TbSharedModule,
    TbIconsModule,
    TbSettingsModule,
    TbMenuModule,
    TB_HOME_COMPONENTS
  ],
  declarations: [
    TB_MENU_COMPONENTS
  ],
  exports: [
    TB_HOME_COMPONENTS,
    TB_MENU_COMPONENTS,
    // TbSettingsModule
  ],
  providers: [MenuService]
})
export class TbMenuModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: TbMenuModule,
      providers: [MenuService]
    };
  }
}
