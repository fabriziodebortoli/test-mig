import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { TaskbuilderCoreModule } from '@taskbuilder/core';

import { NoSpacesEditComponent } from './controls/no-spaces/no-spaces.component';
import { VatComponent } from './controls/vat/vat.component';

export { NoSpacesEditComponent } from './controls/no-spaces/no-spaces.component';
export { VatComponent } from './controls/vat/vat.component';

const ERP_COMPONENTS = [NoSpacesEditComponent, VatComponent];

@NgModule({
    imports: [FormsModule, TaskbuilderCoreModule],
    declarations: [ERP_COMPONENTS],
    exports: [ERP_COMPONENTS]
})
export class ERPSharedModule { }
