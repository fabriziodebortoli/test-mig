import { IDD_TD_PYMTTERMComponent, IDD_TD_PYMTTERMFactoryComponent } from './paymentterms/IDD_TD_PYMTTERM.component';
import { IDD_PYMTTERM_COPYComponent, IDD_PYMTTERM_COPYFactoryComponent } from './paymentterms/IDD_PYMTTERM_COPY.component';
import { IDD_CREDITCARDComponent, IDD_CREDITCARDFactoryComponent } from './creditcard/IDD_CREDITCARD.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TD_PYMTTERM', component: IDD_TD_PYMTTERMFactoryComponent },
            { path: 'IDD_PYMTTERM_COPY', component: IDD_PYMTTERM_COPYFactoryComponent },
            { path: 'IDD_CREDITCARD', component: IDD_CREDITCARDFactoryComponent },
        ])],
    declarations: [
            IDD_TD_PYMTTERMComponent, IDD_TD_PYMTTERMFactoryComponent,
            IDD_PYMTTERM_COPYComponent, IDD_PYMTTERM_COPYFactoryComponent,
            IDD_CREDITCARDComponent, IDD_CREDITCARDFactoryComponent,
    ],
    exports: [
            IDD_TD_PYMTTERMFactoryComponent,
            IDD_PYMTTERM_COPYFactoryComponent,
            IDD_CREDITCARDFactoryComponent,
    ],
    entryComponents: [
            IDD_TD_PYMTTERMComponent,
            IDD_PYMTTERM_COPYComponent,
            IDD_CREDITCARDComponent,
    ]
})


export class PaymentTermsModule { };