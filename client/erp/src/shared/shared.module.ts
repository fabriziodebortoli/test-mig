import { FormsModule } from '@angular/forms';
import { NgModule } from '@angular/core';
import { TaskbuilderCoreModule } from '@taskbuilder/core';

import { NoSpacesEditComponent } from './controls/no-spaces/no-spaces.component';
import { VatComponent } from './controls/vat/vat.component';
import { NumberEditWithFillerComponent} from './controls/number-edit-with-filler/tb-number-edit-with-filler.component';

export { NoSpacesEditComponent } from './controls/no-spaces/no-spaces.component';
export { VatComponent } from './controls/vat/vat.component';
export { NumberEditWithFillerComponent} from './controls/number-edit-with-filler/tb-number-edit-with-filler.component';


const ERP_COMPONENTS = [NoSpacesEditComponent, VatComponent, NumberEditWithFillerComponent];

@NgModule({
    imports: [FormsModule, TaskbuilderCoreModule],
    declarations: [ERP_COMPONENTS],
    exports: [ERP_COMPONENTS]
})
export class ERPSharedModule { }
