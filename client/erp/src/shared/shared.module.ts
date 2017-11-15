import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskbuilderCoreModule } from '@taskbuilder/core';

import { NoSpacesEditComponent } from './controls/no-spaces/no-spaces.component';
import { VatComponent } from './controls/vat/vat.component';
import { NumberEditWithFillerComponent } from './controls/number-edit-with-filler/tb-number-edit-with-filler.component';
import { EsrComponent } from './controls/esr/esr.component';
import { StrBinEditComponent } from './controls/str-bin-edit/str-bin-edit.component';
import { ChartOfAccountComponent } from './controls/chart-of-account/chart-of-account.component';

export { NoSpacesEditComponent } from './controls/no-spaces/no-spaces.component';
export { VatComponent } from './controls/vat/vat.component';
export { NumberEditWithFillerComponent } from './controls/number-edit-with-filler/tb-number-edit-with-filler.component';
export { EsrComponent } from './controls/esr/esr.component';
export { StrBinEditComponent } from './controls/str-bin-edit/str-bin-edit.component';
export { ChartOfAccountComponent } from './controls/chart-of-account/chart-of-account.component';

const ERP_COMPONENTS = [NoSpacesEditComponent, VatComponent,
    NumberEditWithFillerComponent, EsrComponent, StrBinEditComponent, ChartOfAccountComponent];

@NgModule({
    imports: [FormsModule, TaskbuilderCoreModule, CommonModule],
    declarations: [ERP_COMPONENTS],
    exports: [ERP_COMPONENTS],
    providers: []
})
export class ERPSharedModule { }
