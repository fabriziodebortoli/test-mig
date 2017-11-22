import { IDD_WHOLDINGPAY_FEESComponent, IDD_WHOLDINGPAY_FEESFactoryComponent } from './withholdingpayment/IDD_WHOLDINGPAY_FEES.component';
import { IDD_PARAMETERS_PAYEESComponent, IDD_PARAMETERS_PAYEESFactoryComponent } from './payeesdefaults/IDD_PARAMETERS_PAYEES.component';
import { IDD_ASLET770_FEESComponent, IDD_ASLET770_FEESFactoryComponent } from './form770rebuilding/IDD_ASLET770_FEES.component';
import { IDD_FEETEMPLATES_COPYComponent, IDD_FEETEMPLATES_COPYFactoryComponent } from './feetemplates/IDD_FEETEMPLATES_COPY.component';
import { IDD_FEETEMPLATESComponent, IDD_FEETEMPLATESFactoryComponent } from './feetemplates/IDD_FEETEMPLATES.component';
import { IDD_FEESComponent, IDD_FEESFactoryComponent } from './feeslinkedtoje/IDD_FEES.component';
import { IDD_DUTYCODESComponent, IDD_DUTYCODESFactoryComponent } from './dutycodes/IDD_DUTYCODES.component';
import { IDD_770ONFILEComponent, IDD_770ONFILEFactoryComponent } from './770onfile/IDD_770ONFILE.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_WHOLDINGPAY_FEES', component: IDD_WHOLDINGPAY_FEESFactoryComponent },
            { path: 'IDD_PARAMETERS_PAYEES', component: IDD_PARAMETERS_PAYEESFactoryComponent },
            { path: 'IDD_ASLET770_FEES', component: IDD_ASLET770_FEESFactoryComponent },
            { path: 'IDD_FEETEMPLATES_COPY', component: IDD_FEETEMPLATES_COPYFactoryComponent },
            { path: 'IDD_FEETEMPLATES', component: IDD_FEETEMPLATESFactoryComponent },
            { path: 'IDD_FEES', component: IDD_FEESFactoryComponent },
            { path: 'IDD_DUTYCODES', component: IDD_DUTYCODESFactoryComponent },
            { path: 'IDD_770ONFILE', component: IDD_770ONFILEFactoryComponent },
        ])],
    declarations: [
            IDD_WHOLDINGPAY_FEESComponent, IDD_WHOLDINGPAY_FEESFactoryComponent,
            IDD_PARAMETERS_PAYEESComponent, IDD_PARAMETERS_PAYEESFactoryComponent,
            IDD_ASLET770_FEESComponent, IDD_ASLET770_FEESFactoryComponent,
            IDD_FEETEMPLATES_COPYComponent, IDD_FEETEMPLATES_COPYFactoryComponent,
            IDD_FEETEMPLATESComponent, IDD_FEETEMPLATESFactoryComponent,
            IDD_FEESComponent, IDD_FEESFactoryComponent,
            IDD_DUTYCODESComponent, IDD_DUTYCODESFactoryComponent,
            IDD_770ONFILEComponent, IDD_770ONFILEFactoryComponent,
    ],
    exports: [
            IDD_WHOLDINGPAY_FEESFactoryComponent,
            IDD_PARAMETERS_PAYEESFactoryComponent,
            IDD_ASLET770_FEESFactoryComponent,
            IDD_FEETEMPLATES_COPYFactoryComponent,
            IDD_FEETEMPLATESFactoryComponent,
            IDD_FEESFactoryComponent,
            IDD_DUTYCODESFactoryComponent,
            IDD_770ONFILEFactoryComponent,
    ],
    entryComponents: [
            IDD_WHOLDINGPAY_FEESComponent,
            IDD_PARAMETERS_PAYEESComponent,
            IDD_ASLET770_FEESComponent,
            IDD_FEETEMPLATES_COPYComponent,
            IDD_FEETEMPLATESComponent,
            IDD_FEESComponent,
            IDD_DUTYCODESComponent,
            IDD_770ONFILEComponent,
    ]
})


export class PayeesModule { };