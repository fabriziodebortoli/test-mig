import { IDD_LOAD_SUPPQUOTAComponent, IDD_LOAD_SUPPQUOTAFactoryComponent } from './suppquotaloading/IDD_LOAD_SUPPQUOTA.component';
import { IDD_SUPP_QUOTAComponent, IDD_SUPP_QUOTAFactoryComponent } from './suppquota/IDD_SUPP_QUOTA.component';
import { IDD_PRINT_SUPPQUOTATIONSComponent, IDD_PRINT_SUPPQUOTATIONSFactoryComponent } from './supplierquotationsprint/IDD_PRINT_SUPPQUOTATIONS.component';
import { IDD_DELETE_SUPPQUOTATIONSComponent, IDD_DELETE_SUPPQUOTATIONSFactoryComponent } from './supplierquotationsdeleting/IDD_DELETE_SUPPQUOTATIONS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_LOAD_SUPPQUOTA', component: IDD_LOAD_SUPPQUOTAFactoryComponent },
            { path: 'IDD_SUPP_QUOTA', component: IDD_SUPP_QUOTAFactoryComponent },
            { path: 'IDD_PRINT_SUPPQUOTATIONS', component: IDD_PRINT_SUPPQUOTATIONSFactoryComponent },
            { path: 'IDD_DELETE_SUPPQUOTATIONS', component: IDD_DELETE_SUPPQUOTATIONSFactoryComponent },
        ])],
    declarations: [
            IDD_LOAD_SUPPQUOTAComponent, IDD_LOAD_SUPPQUOTAFactoryComponent,
            IDD_SUPP_QUOTAComponent, IDD_SUPP_QUOTAFactoryComponent,
            IDD_PRINT_SUPPQUOTATIONSComponent, IDD_PRINT_SUPPQUOTATIONSFactoryComponent,
            IDD_DELETE_SUPPQUOTATIONSComponent, IDD_DELETE_SUPPQUOTATIONSFactoryComponent,
    ],
    exports: [
            IDD_LOAD_SUPPQUOTAFactoryComponent,
            IDD_SUPP_QUOTAFactoryComponent,
            IDD_PRINT_SUPPQUOTATIONSFactoryComponent,
            IDD_DELETE_SUPPQUOTATIONSFactoryComponent,
    ],
    entryComponents: [
            IDD_LOAD_SUPPQUOTAComponent,
            IDD_SUPP_QUOTAComponent,
            IDD_PRINT_SUPPQUOTATIONSComponent,
            IDD_DELETE_SUPPQUOTATIONSComponent,
    ]
})


export class SupplierQuotationsModule { };