import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
<<<<<<< HEAD:client/core.tmp/.tmp/shared/shared.module.ts
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MaterialModule } from '@angular/material';
import { RouterModule } from '@angular/router';

import { GridModule } from '@progress/kendo-angular-grid';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DialogModule } from '@progress/kendo-angular-dialog';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { PopupModule } from '@progress/kendo-angular-popup';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { ChartsModule } from '@progress/kendo-angular-charts';

const KENDO_UI_MODULES = [
    GridModule,
    InputsModule,
    DateInputsModule,
    DialogModule,
    DropDownsModule,
    LayoutModule,
    PopupModule,
    ButtonsModule,
    ChartsModule
];

import { MasonryModule } from 'angular2-masonry';
=======

import { TaskbuilderCoreModule } from '@taskbuilder/core';

@NgModule({
    imports: [
        CommonModule,
        TaskbuilderCoreModule
    ],
    exports: [TaskbuilderCoreModule]
>>>>>>> SharedModule pulito:client/web-form/src/app/shared/shared.module.ts

const TB_MODULES = [
    ReactiveFormsModule, InputsModule,
    DialogModule,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MaterialModule,
    RouterModule,
    MasonryModule,
    KENDO_UI_MODULES
];

/**
 * Direttive per style o funzionalità applicate a componenti base
 */
import { TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective } from './directives';
import { ContextMenuDirective, LayoutTypeColumnDirective, LayoutTypeHboxDirective, LayoutTypeVboxDirective } from './directives';
export * from './directives';

const TB_DIRECTIVES = [
    TileMicroDirective, TileMiniDirective, TileStandardDirective, TileWideDirective, TileAutofillDirective,
    ContextMenuDirective, LayoutTypeColumnDirective, LayoutTypeHboxDirective, LayoutTypeVboxDirective
];

@NgModule({
    imports: [TB_MODULES],
    declarations: [TB_DIRECTIVES],
    exports: [TB_MODULES, TB_DIRECTIVES]
})
export class TbSharedModule { }
