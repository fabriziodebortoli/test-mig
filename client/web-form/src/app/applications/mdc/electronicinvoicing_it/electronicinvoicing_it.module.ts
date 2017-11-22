import { IDD_UPDATEVERSION_ADDDATAComponent, IDD_UPDATEVERSION_ADDDATAFactoryComponent } from './ei_itupdateversionadditionaldata/IDD_UPDATEVERSION_ADDDATA.component';
import { IDD_ADDITIONALDATAComponent, IDD_ADDITIONALDATAFactoryComponent } from './ei_itadditionaldata/IDD_ADDITIONALDATA.component';
import { IDD_EI_CHECKS_RVComponent, IDD_EI_CHECKS_RVFactoryComponent } from './attachmentmanager/IDD_EI_CHECKS_RV.component';
import { IDD_EI_CUSTOMERComponent, IDD_EI_CUSTOMERFactoryComponent } from './uieicommon/IDD_EI_CUSTOMER.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_UPDATEVERSION_ADDDATA', component: IDD_UPDATEVERSION_ADDDATAFactoryComponent },
            { path: 'IDD_ADDITIONALDATA', component: IDD_ADDITIONALDATAFactoryComponent },
            { path: 'IDD_EI_CHECKS_RV', component: IDD_EI_CHECKS_RVFactoryComponent },
            { path: 'IDD_EI_CUSTOMER', component: IDD_EI_CUSTOMERFactoryComponent },
        ])],
    declarations: [
            IDD_UPDATEVERSION_ADDDATAComponent, IDD_UPDATEVERSION_ADDDATAFactoryComponent,
            IDD_ADDITIONALDATAComponent, IDD_ADDITIONALDATAFactoryComponent,
            IDD_EI_CHECKS_RVComponent, IDD_EI_CHECKS_RVFactoryComponent,
            IDD_EI_CUSTOMERComponent, IDD_EI_CUSTOMERFactoryComponent,
    ],
    exports: [
            IDD_UPDATEVERSION_ADDDATAFactoryComponent,
            IDD_ADDITIONALDATAFactoryComponent,
            IDD_EI_CHECKS_RVFactoryComponent,
            IDD_EI_CUSTOMERFactoryComponent,
    ],
    entryComponents: [
            IDD_UPDATEVERSION_ADDDATAComponent,
            IDD_ADDITIONALDATAComponent,
            IDD_EI_CHECKS_RVComponent,
            IDD_EI_CUSTOMERComponent,
    ]
})


export class ElectronicInvoicing_ITModule { };