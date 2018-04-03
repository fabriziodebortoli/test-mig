import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';

import { TbSharedModule, Logger } from '@taskbuilder/core';
import { TbIconsModule } from '@taskbuilder/icons';

import { TbMenuModule as CoreMenu } from '@taskbuilder/core';

/** 
 * HOME 
 */
import { StandaloneDocumentComponent } from './components/home/standalone.document/standalone.document.component';
import { StandaloneReportComponent } from './components/home/standalone.report/standalone.report.component';
import { HomeSidenavLeftComponent } from './components/home/home-sidenav-left/home-sidenav-left.component';
import { HomeComponent } from './components/home/home.component';
// ---------------------------------------------------------------------------------------------------------------
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
 * MENU
 */
import { FavoritesComponent } from './components/menu/favorites/favorites.component';
import { SearchComponent } from './components/menu/search/search.component';
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

export { FavoritesComponent } from './components/menu/favorites/favorites.component';
export { SearchComponent } from './components/menu/search/search.component';
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

const TB_MENU_COMPONENTS = [
  FavoritesComponent,
  SearchComponent,
  MenuComponent,
  MenuContainerComponent,
  MenuContentComponent,
  MenuElementComponent,
  ItemCustomizationsDropdownComponent,
  MenuStepperComponent,
  MenuTabberComponent,
  MenuTabComponent,
  MostUsedComponent,
  HiddenTilesComponent
];

const NG_MODULES = [
  CommonModule,
  ReactiveFormsModule,
  FormsModule
];

@NgModule({
  imports: [
    NG_MODULES,
    CoreMenu,
    TbIconsModule,
    TbSharedModule
  ],
  declarations: [
    TB_MENU_COMPONENTS,
    TB_HOME_COMPONENTS
  ],
  exports: [
    TB_MENU_COMPONENTS,
    TB_HOME_COMPONENTS
  ],
  // providers: [MenuService]
})
export class TbMenuModule {
  // static forRoot(): ModuleWithProviders {
  //   return {
  //     ngModule: TbMenuModule,
  //     providers: [MenuService]
  //   };
  // }
}
