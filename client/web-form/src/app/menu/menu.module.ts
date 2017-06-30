import { BrowserModule } from '@angular/platform-browser';
import { GroupSelectorComponent } from './components/menu/group-selector/group-selector.component';
import { MenuStepperComponent } from './components/menu/menu-stepper/menu-stepper.component';

import { ConnectionInfoDialogComponent } from './components/menu/connection-info-dialog/connection-info-dialog.component';
import { ProductInfoDialogComponent } from './components/menu/product-info-dialog/product-info-dialog.component';

import { NgModule, ModuleWithProviders } from '@angular/core';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { CommonModule } from '@angular/common';
import { MaterialModule } from '@angular/material';
import { RouterModule } from '@angular/router';

import { MasonryModule } from 'angular2-masonry';

import { SharedModule } from '../shared/shared.module';

import { SearchComponent } from './components/menu/search/search.component';
import { MostUsedComponent } from './components/menu/most-used/most-used.component';
import { MenuComponent } from './components/menu/menu.component';

import { FavoritesComponent } from './components/menu/favorites/favorites.component';

import { GridModule } from '@progress/kendo-angular-grid';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { DropDownsModule, AutoCompleteComponent } from '@progress/kendo-angular-dropdowns';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { PopupModule } from '@progress/kendo-angular-popup';
import { ButtonsModule } from '@progress/kendo-angular-buttons';

import { Logger } from '@taskbuilder/core';

import { MenuTabberComponent } from './components/menu/menu-tabber/menu-tabber.component';
import { MenuTabComponent } from './components/menu/menu-tabber/menu-tab/menu-tab.component';
import { MenuContainerComponent } from './components/menu/menu-container/menu-container.component';
import { MenuElementComponent } from './components/menu/menu-element/menu-element.component';
import { MenuContentComponent } from './components/menu/menu-content/menu-content.component';

const KENDO_UI_MODULES = [
  GridModule,
  InputsModule,
  DateInputsModule,
  DialogModule,
  DropDownsModule,
  LayoutModule,
  PopupModule,
  ButtonsModule
];

@NgModule({
  imports: [
    BrowserModule,
    CommonModule,
    SharedModule,
    FormsModule,
    MaterialModule,
    ReactiveFormsModule,
    MasonryModule,
    KENDO_UI_MODULES
  ],

  declarations:
  [
    MenuComponent,
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
    RouterModule,
    MenuComponent,
    MenuElementComponent,
    MenuContainerComponent,
    FavoritesComponent,
    MostUsedComponent,
    SearchComponent,
    GroupSelectorComponent,
    MenuStepperComponent,
    MenuContentComponent
  ],
  entryComponents: [
    ProductInfoDialogComponent,
    ConnectionInfoDialogComponent
  ]
})
export class MenuModule {
  static forRoot(): ModuleWithProviders {
    return {
      ngModule: MenuModule
    };
  }

  constructor(private logger: Logger) {
    this.logger.debug('MenuModule instantiated - ' + Math.round(new Date().getTime() / 1000));
  }
}

