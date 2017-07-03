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
    ComboComponent, EnumComboComponent, RadioComponent, ImageComponent,
    CheckBoxComponent, ButtonComponent, StateButtonComponent, ColorPickerComponent, BoolEditComponent, BodyEditComponent,
    LinearGaugeComponent
} from './controls/';

import { GridComponent } from './controls/grid/grid.component';
import { PlaceholderComponent } from './controls/placeholder/placeholder.component';
import { DateInputComponent } from './controls/date-input/date-input.component';

import { EmailComponent } from './controls/email/email.component';
import { TimeInputComponent } from './controls/time-input/time-input.component';
import { SectionTitleComponent } from './controls/section-title/section-title.component';
import { TextareaComponent } from './controls/textarea/textarea.component';
import { FileComponent } from './controls/file/file.component';
import { LinkComponent } from './controls/link/link.component';
import { ComboSimpleComponent } from './controls/combo-simple/combo-simple.component';

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
    ComboComponent, EnumComboComponent, RadioComponent, CheckBoxComponent, ButtonComponent, GridComponent,
    DateInputComponent, StateButtonComponent, TimeInputComponent,
    PlaceholderComponent, ImageComponent, ColorPickerComponent,
    BoolEditComponent, BodyEditComponent, LinkComponent, LinearGaugeComponent,
    EmailComponent, SectionTitleComponent, TextareaComponent, FileComponent, ComboSimpleComponent,
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
