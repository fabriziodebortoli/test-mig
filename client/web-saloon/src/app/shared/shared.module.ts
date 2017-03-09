import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { MaterialModule } from '@angular/material';

import { GridModule } from '@progress/kendo-angular-grid';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { PopupModule } from '@progress/kendo-angular-popup';
import { SortableModule } from '@progress/kendo-angular-sortable';
import { ChartsModule } from '@progress/kendo-angular-charts';

import { PageNotFoundComponent } from './page-not-found/page-not-found.component';
import { LoginComponent } from './login/login.component';

const COMPONENTS = [
  PageNotFoundComponent
];

const KENDO_UI_MODULES = [
  GridModule,
  ButtonsModule,
  InputsModule,
  DateInputsModule,
  DialogModule,
  DropDownsModule,
  LayoutModule,
  PopupModule,
  SortableModule,
  ChartsModule
];

@NgModule({
  imports: [
    CommonModule,
    FormsModule,
    MaterialModule,
    KENDO_UI_MODULES
  ],
  exports: [
    MaterialModule,
    KENDO_UI_MODULES
  ],
  declarations: [
    COMPONENTS,
    LoginComponent
  ]
})
export class SharedModule { }
