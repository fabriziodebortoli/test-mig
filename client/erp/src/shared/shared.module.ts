import { NgModule } from '@angular/core';
import { TaskbuilderCoreModule } from '@TaskBuilder/core';

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

import { NoSpacesEditComponent } from './controls/no-spaces/no-spaces.component';
import { VatComponent } from './controls/vat/vat.component';

export { NoSpacesEditComponent } from './controls/no-spaces/no-spaces.component';
export { VatComponent } from './controls/vat/vat.component';

const ERP_COMPONENTS = [NoSpacesEditComponent, VatComponent];

@NgModule({
    imports: [TaskbuilderCoreModule, KENDO_UI_MODULES],
    declarations: [ERP_COMPONENTS],
    exports: [ERP_COMPONENTS]
})
export class ERPSharedModule { }
