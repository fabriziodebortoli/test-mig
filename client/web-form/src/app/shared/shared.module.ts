import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
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

import {
    BodyEditComponent,
    LinearGaugeComponent
} from './controls/';

import { GridComponent } from './controls/grid/grid.component';

import { HotlinkComponent } from './controls/hotlink/hotlink.component';


import { TaskbuilderCoreModule } from '@taskbuilder/core';

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

const TB_COMPONENTS = [
    GridComponent,

    BodyEditComponent, LinearGaugeComponent,

    HotlinkComponent
];

@NgModule({
    imports: [
        ReactiveFormsModule,
        CommonModule,
        FormsModule,
        ReactiveFormsModule,
        MaterialModule,
        RouterModule,
        KENDO_UI_MODULES,
        TaskbuilderCoreModule
    ],
    declarations: [TB_COMPONENTS],
    exports: [TB_COMPONENTS, TaskbuilderCoreModule]

})
export class SharedModule {

    constructor() {

    }

}
