import { IDD_LOGISTIC_CONTROLComponent, IDD_LOGISTIC_CONTROLFactoryComponent } from './logisticscontrolpanel/IDD_LOGISTIC_CONTROL.component';
import { IDD_INVOICE_PARAMComponent, IDD_INVOICE_PARAMFactoryComponent } from './invoiceparameters/IDD_INVOICE_PARAM.component';
import { IDD_DOCUMENT_COPYComponent, IDD_DOCUMENT_COPYFactoryComponent } from './documentcopy/IDD_DOCUMENT_COPY.component';
import { IDD_CORRECTIONDOC_PARAMComponent, IDD_CORRECTIONDOC_PARAMFactoryComponent } from './correctiondocparameters/IDD_CORRECTIONDOC_PARAM.component';
import { IDD_SCA_BOX_ERRORComponent, IDD_SCA_BOX_ERRORFactoryComponent } from './uipymtschedulebox/IDD_SCA_BOX_ERROR.component';
import { IDD_INVOICE_DOCUMENTComponent, IDD_INVOICE_DOCUMENTFactoryComponent } from './uiinvoicedoc/IDD_INVOICE_DOCUMENT.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_LOGISTIC_CONTROL', component: IDD_LOGISTIC_CONTROLFactoryComponent },
            { path: 'IDD_INVOICE_PARAM', component: IDD_INVOICE_PARAMFactoryComponent },
            { path: 'IDD_DOCUMENT_COPY', component: IDD_DOCUMENT_COPYFactoryComponent },
            { path: 'IDD_CORRECTIONDOC_PARAM', component: IDD_CORRECTIONDOC_PARAMFactoryComponent },
            { path: 'IDD_SCA_BOX_ERROR', component: IDD_SCA_BOX_ERRORFactoryComponent },
            { path: 'IDD_INVOICE_DOCUMENT', component: IDD_INVOICE_DOCUMENTFactoryComponent },
        ])],
    declarations: [
            IDD_LOGISTIC_CONTROLComponent, IDD_LOGISTIC_CONTROLFactoryComponent,
            IDD_INVOICE_PARAMComponent, IDD_INVOICE_PARAMFactoryComponent,
            IDD_DOCUMENT_COPYComponent, IDD_DOCUMENT_COPYFactoryComponent,
            IDD_CORRECTIONDOC_PARAMComponent, IDD_CORRECTIONDOC_PARAMFactoryComponent,
            IDD_SCA_BOX_ERRORComponent, IDD_SCA_BOX_ERRORFactoryComponent,
            IDD_INVOICE_DOCUMENTComponent, IDD_INVOICE_DOCUMENTFactoryComponent,
    ],
    exports: [
            IDD_LOGISTIC_CONTROLFactoryComponent,
            IDD_INVOICE_PARAMFactoryComponent,
            IDD_DOCUMENT_COPYFactoryComponent,
            IDD_CORRECTIONDOC_PARAMFactoryComponent,
            IDD_SCA_BOX_ERRORFactoryComponent,
            IDD_INVOICE_DOCUMENTFactoryComponent,
    ],
    entryComponents: [
            IDD_LOGISTIC_CONTROLComponent,
            IDD_INVOICE_PARAMComponent,
            IDD_DOCUMENT_COPYComponent,
            IDD_CORRECTIONDOC_PARAMComponent,
            IDD_SCA_BOX_ERRORComponent,
            IDD_INVOICE_DOCUMENTComponent,
    ]
})


export class InvoiceMngModule { };