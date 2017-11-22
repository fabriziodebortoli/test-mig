import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TaskbuilderCoreModule } from '@taskbuilder/core';

// import { NoSpacesEditComponent } from './controls/no-spaces/no-spaces.component';
// import { VatComponent } from './controls/vat/vat.component';
// import { NumberEditWithFillerComponent } from './controls/number-edit-with-filler/tb-number-edit-with-filler.component';
// import { EsrComponent } from './controls/esr/esr.component';
// import { StrBinEditComponent } from './controls/str-bin-edit/str-bin-edit.component';
// import { ItemEditComponent } from './controls/item-edit/item-edit.component';

import * as Controls from './controls';
export * from './controls';

import * as Pipes from './pipes';
export * from './pipes';

// export { NoSpacesEditComponent } from './controls/no-spaces/no-spaces.component';
// export { VatComponent } from './controls/vat/vat.component';
// export { NumberEditWithFillerComponent } from './controls/number-edit-with-filler/tb-number-edit-with-filler.component';
// export { EsrComponent } from './controls/esr/esr.component';
// export { StrBinEditComponent } from './controls/str-bin-edit/str-bin-edit.component';
// export { ItemEditComponent } from './controls/item-edit/item-edit.component';

const ERP_COMPONENTS = [
    Controls.AutoSearchEditComponent,
    Controls.EsrComponent,
    Controls.ItemEditComponent,
    Controls.NoSpacesEditComponent,
    Controls.NumberEditWithFillerComponent,
    Controls.StrBinEditComponent,
    Controls.VatComponent,
    Controls.ChartOfAccountComponent
];

const ERP_PIPES = [
    Pipes.KeyValueFilterPipe
];

@NgModule({
    imports: [FormsModule, TaskbuilderCoreModule, CommonModule],
    declarations: [ERP_COMPONENTS, ERP_PIPES],
    exports: [ERP_COMPONENTS, ERP_PIPES],
    providers: []
})
export class ERPSharedModule { }
