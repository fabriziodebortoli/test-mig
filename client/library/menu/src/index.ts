import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';

import { Logger } from '@taskbuilder/core';

import { LoginComponent } from './components/login/login.component';
import { ApplicationSelectorComponent } from './components/menu/application-selector/application-selector.component';
import { FavoritesComponent } from './components/menu/favorites/favorites.component';
import { SearchComponent } from './components/menu/search/search.component';
import { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
import { MenuComponent } from './components/menu/menu.component';
import { MenuContainerComponent } from './components/menu/menu-container/menu-container.component';
import { MenuContentComponent } from './components/menu/menu-content/menu-content.component';
import { MenuElementComponent } from './components/menu/menu-element/menu-element.component';
import { MenuStepperComponent } from './components/menu/menu-stepper/menu-stepper.component';
import { MenuTabberComponent } from './components/menu/menu-tabber/menu-tabber.component';
import { MenuTabComponent } from './components/menu/menu-tabber/menu-tab/menu-tab.component';
import { MostUsedComponent } from './components/menu/most-used/most-used.component';

export { LoginComponent } from './components/login/login.component';
export { ApplicationSelectorComponent } from './components/menu/application-selector/application-selector.component';
export { FavoritesComponent } from './components/menu/favorites/favorites.component';
export { SearchComponent } from './components/menu/search/search.component';
export { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
export { MenuComponent } from './components/menu/menu.component';
export { MenuContainerComponent } from './components/menu/menu-container/menu-container.component';
export { MenuContentComponent } from './components/menu/menu-content/menu-content.component';
export { MenuElementComponent } from './components/menu/menu-element/menu-element.component';
export { MenuStepperComponent } from './components/menu/menu-stepper/menu-stepper.component';
export { MenuTabberComponent } from './components/menu/menu-tabber/menu-tabber.component';
export { MenuTabComponent } from './components/menu/menu-tabber/menu-tab/menu-tab.component';
export { MostUsedComponent } from './components/menu/most-used/most-used.component';

const MENU_COMPONENTS = [
  LoginComponent,
  ApplicationSelectorComponent, FavoritesComponent, MostUsedComponent, GroupSelectorComponent,
  MenuContainerComponent, MenuContentComponent, MenuElementComponent, MenuStepperComponent,
  MenuComponent, MenuTabberComponent, MenuTabComponent,
  SearchComponent
];

import { LayoutModule } from '@progress/kendo-angular-layout';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';

const KENDO_MODULES = [
  LayoutModule,
  ButtonsModule,
  DialogModule,
  InputsModule,
  DropDownsModule
];

import { MenuService } from './services/menu.service';
import { EventManagerService } from './services/event-manager.service';
import { SettingsService } from './services/settings.service';
import { HttpMenuService } from './services/http-menu.service';
import { ImageService } from './services/image.service';

export const TB_MENU_SERVICES = [
  MenuService,
  ImageService,
  HttpMenuService,
  SettingsService,
  EventManagerService
];

export { MenuService } from './services/menu.service';
export { ImageService } from './services/image.service';
export { HttpMenuService } from './services/http-menu.service';
export { SettingsService } from './services/settings.service';
export { EventManagerService } from './services/event-manager.service';

@NgModule({
  imports: [CommonModule, KENDO_MODULES],
  declarations: [MENU_COMPONENTS],
  exports: [MENU_COMPONENTS],
  providers: [TB_MENU_SERVICES]
})
export class TbMenuModule {
  constructor(public logger: Logger) {
    this.logger.debug('TbMenuModule from Core instantiated - ' + Math.round(new Date().getTime() / 1000));
  }
}
