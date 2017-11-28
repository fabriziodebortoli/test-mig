import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskbuilderCoreModule } from '@taskbuilder/core';

import { NoSpacesEditComponent } from './controls/no-spaces/no-spaces.component';
import { VatComponent } from './controls/vat/vat.component';
import { NumberEditWithFillerComponent } from './controls/number-edit-with-filler/tb-number-edit-with-filler.component';
import { EsrComponent } from './controls/esr/esr.component';
import { StrBinEditComponent } from './controls/str-bin-edit/str-bin-edit.component';
import { ItemEditComponent } from './controls/item-edit/item-edit.component';
import { AutoSearchEditComponent } from './controls/auto-search-edit/auto-search-edit.component';
import { ChartOfAccountComponent } from './controls/chart-of-account/chart-of-account.component';

import { KeyValueFilterPipe } from './pipes/key-value-filter.pipe';

export { NoSpacesEditComponent } from './controls/no-spaces/no-spaces.component';
export { VatComponent } from './controls/vat/vat.component';
export { NumberEditWithFillerComponent } from './controls/number-edit-with-filler/tb-number-edit-with-filler.component';
export { EsrComponent } from './controls/esr/esr.component';
export { StrBinEditComponent } from './controls/str-bin-edit/str-bin-edit.component';
export { ItemEditComponent } from './controls/item-edit/item-edit.component';
export { AutoSearchEditComponent } from './controls/auto-search-edit/auto-search-edit.component';
export { ChartOfAccountComponent } from './controls/chart-of-account/chart-of-account.component';

export { KeyValueFilterPipe } from './pipes/key-value-filter.pipe';

const ERP_COMPONENTS = [
    AutoSearchEditComponent,
    EsrComponent,
    ItemEditComponent,
    NoSpacesEditComponent,
    NumberEditWithFillerComponent,
    StrBinEditComponent,
    VatComponent,
    ChartOfAccountComponent
];

const ERP_PIPES = [
    KeyValueFilterPipe
];

@NgModule({
    imports: [FormsModule, TaskbuilderCoreModule, CommonModule],
    declarations: [ERP_COMPONENTS, ERP_PIPES],
    exports: [ERP_COMPONENTS, ERP_PIPES],
    providers: []
})
export class ERPSharedModule { }
