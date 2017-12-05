import { NgModule } from '@angular/core';

import { DialogModule } from '@progress/kendo-angular-dialog';
import { LayoutModule } from '@progress/kendo-angular-layout';
import { PopupModule } from '@progress/kendo-angular-popup';
import { ButtonsModule } from '@progress/kendo-angular-buttons';
import { InputsModule } from '@progress/kendo-angular-inputs';
import { DateInputsModule } from '@progress/kendo-angular-dateinputs';
import { DropDownsModule } from '@progress/kendo-angular-dropdowns';
import { GridModule, GridComponent } from '@progress/kendo-angular-grid';
import { ChartsModule } from '@progress/kendo-angular-charts';

const KENDO_UI_MODULES = [
    GridModule,
    ChartsModule,
    DialogModule,
    DateInputsModule,
    DropDownsModule,
    InputsModule,
    LayoutModule,
    PopupModule,
    ButtonsModule
];

@NgModule({
    imports: [KENDO_UI_MODULES],
    exports: [KENDO_UI_MODULES]
})
export class TbKendoModule { }