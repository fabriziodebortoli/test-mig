import { IDD_EI_CHECKS_RVComponent, IDD_EI_CHECKS_RVFactoryComponent } from './attachmentmanager/IDD_EI_CHECKS_RV.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_EI_CHECKS_RV', component: IDD_EI_CHECKS_RVFactoryComponent },
        ])],
    declarations: [
            IDD_EI_CHECKS_RVComponent, IDD_EI_CHECKS_RVFactoryComponent,
    ],
    exports: [
            IDD_EI_CHECKS_RVFactoryComponent,
    ],
    entryComponents: [
            IDD_EI_CHECKS_RVComponent,
    ]
})


export class ElectronicInvoicing_ITModule { };