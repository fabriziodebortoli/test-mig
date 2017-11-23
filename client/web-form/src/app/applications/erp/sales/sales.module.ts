import { IDD_SALES_COMPANYUSER_SETTINGSComponent, IDD_SALES_COMPANYUSER_SETTINGSFactoryComponent } from './salessettings/IDD_SALES_COMPANYUSER_SETTINGS.component';
import { IDD_SALES_PARAMETERSComponent, IDD_SALES_PARAMETERSFactoryComponent } from './salesparameters/IDD_SALES_PARAMETERS.component';
import { IDD_SALE_ORD_FULFILLMENTComponent, IDD_SALE_ORD_FULFILLMENTFactoryComponent } from './saleordfulfilment/IDD_SALE_ORD_FULFILLMENT.component';
import { IDD_SALE_ORD_EDITComponent, IDD_SALE_ORD_EDITFactoryComponent } from './saleordfulfilment/IDD_SALE_ORD_EDIT.component';
import { IDD_LOAD_RFCComponent, IDD_LOAD_RFCFactoryComponent } from './rfcloading/IDD_LOAD_RFC.component';
import { IDD_PICKED_REBUILDINGComponent, IDD_PICKED_REBUILDINGFactoryComponent } from './pickedrebuilding/IDD_PICKED_REBUILDING.component';
import { IDD_GROUPING_CODESComponent, IDD_GROUPING_CODESFactoryComponent } from './groupingcodes/IDD_GROUPING_CODES.component';
import { IDD_WIZ_SALES_POSTDOC_MAINComponent, IDD_WIZ_SALES_POSTDOC_MAINFactoryComponent } from './documentposting/IDD_WIZ_SALES_POSTDOC_MAIN.component';
import { IDD_WIZ_POSTING_NFC_MAINComponent, IDD_WIZ_POSTING_NFC_MAINFactoryComponent } from './documentposting/IDD_WIZ_POSTING_NFC_MAIN.component';
import { IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAINComponent, IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAINFactoryComponent } from './documentposting/IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAIN.component';
import { IDD_SALE_DOC_MAINTENANCEComponent, IDD_SALE_DOC_MAINTENANCEFactoryComponent } from './documentmaintenance/IDD_SALE_DOC_MAINTENANCE.component';
import { IDD_SALESDOC_DEL_WIZARDComponent, IDD_SALESDOC_DEL_WIZARDFactoryComponent } from './documentdeleting/IDD_SALESDOC_DEL_WIZARD.component';
import { IDD_LOAD_DNComponent, IDD_LOAD_DNFactoryComponent } from './dnloading/IDD_LOAD_DN.component';
import { IDD_UPDATE_ACTUALComponent, IDD_UPDATE_ACTUALFactoryComponent } from './customeractualrebuilding/IDD_UPDATE_ACTUAL.component';
import { IDD_LOAD_BOLComponent, IDD_LOAD_BOLFactoryComponent } from './billofladingloading/IDD_LOAD_BOL.component';
import { IDD_LOAD_INVComponent, IDD_LOAD_INVFactoryComponent } from './uisaleinvoiceloading/IDD_LOAD_INV.component';
import { IDD_SALE_DOCUMENTComponent, IDD_SALE_DOCUMENTFactoryComponent } from './uisaledocview/IDD_SALE_DOCUMENT.component';
import { IDD_INTERSTORAGEComponent, IDD_INTERSTORAGEFactoryComponent } from './uisaledocview/IDD_INTERSTORAGE.component';
import { IDD_LOADER_SALE_ORDComponent, IDD_LOADER_SALE_ORDFactoryComponent } from './uisaledocloadfilters/IDD_LOADER_SALE_ORD.component';
import { IDD_LOADER_PROFORMA_INVOICEComponent, IDD_LOADER_PROFORMA_INVOICEFactoryComponent } from './uisaledocloadfilters/IDD_LOADER_PROFORMA_INVOICE.component';
import { IDD_LOADER_PLComponent, IDD_LOADER_PLFactoryComponent } from './uisaledocloadfilters/IDD_LOADER_PL.component';
import { IDD_LOADER_DOCUMENT_FILTERComponent, IDD_LOADER_DOCUMENT_FILTERFactoryComponent } from './uisaledocloadfilters/IDD_LOADER_DOCUMENT_FILTER.component';
import { IDD_LOADER_DNComponent, IDD_LOADER_DNFactoryComponent } from './uisaledocloadfilters/IDD_LOADER_DN.component';
import { IDD_NOTA_FISCAL_CUSTOMERComponent, IDD_NOTA_FISCAL_CUSTOMERFactoryComponent } from './uinotafiscalforcustomer/IDD_NOTA_FISCAL_CUSTOMER.component';
import { IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCALComponent, IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCALFactoryComponent } from './uinotafiscalforcustomer/IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCAL.component';
import { IDD_WIZVIEW_DEFINV_RFCComponent, IDD_WIZVIEW_DEFINV_RFCFactoryComponent } from './uideferredinvoicing/IDD_WIZVIEW_DEFINV_RFC.component';
import { IDD_WIZVIEW_DEFINV_RECEIPTComponent, IDD_WIZVIEW_DEFINV_RECEIPTFactoryComponent } from './uideferredinvoicing/IDD_WIZVIEW_DEFINV_RECEIPT.component';
import { IDD_WIZVIEW_DEFINV_PLComponent, IDD_WIZVIEW_DEFINV_PLFactoryComponent } from './uideferredinvoicing/IDD_WIZVIEW_DEFINV_PL.component';
import { IDD_WIZVIEW_DEFINVComponent, IDD_WIZVIEW_DEFINVFactoryComponent } from './uideferredinvoicing/IDD_WIZVIEW_DEFINV.component';
import { IDD_PD_DOC_CASHComponent, IDD_PD_DOC_CASHFactoryComponent } from './salespostingpayablesreceivables/IDD_PD_DOC_CASH.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_SALES_COMPANYUSER_SETTINGS', component: IDD_SALES_COMPANYUSER_SETTINGSFactoryComponent },
            { path: 'IDD_SALES_PARAMETERS', component: IDD_SALES_PARAMETERSFactoryComponent },
            { path: 'IDD_SALE_ORD_FULFILLMENT', component: IDD_SALE_ORD_FULFILLMENTFactoryComponent },
            { path: 'IDD_SALE_ORD_EDIT', component: IDD_SALE_ORD_EDITFactoryComponent },
            { path: 'IDD_LOAD_RFC', component: IDD_LOAD_RFCFactoryComponent },
            { path: 'IDD_PICKED_REBUILDING', component: IDD_PICKED_REBUILDINGFactoryComponent },
            { path: 'IDD_GROUPING_CODES', component: IDD_GROUPING_CODESFactoryComponent },
            { path: 'IDD_WIZ_SALES_POSTDOC_MAIN', component: IDD_WIZ_SALES_POSTDOC_MAINFactoryComponent },
            { path: 'IDD_WIZ_POSTING_NFC_MAIN', component: IDD_WIZ_POSTING_NFC_MAINFactoryComponent },
            { path: 'IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAIN', component: IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAINFactoryComponent },
            { path: 'IDD_SALE_DOC_MAINTENANCE', component: IDD_SALE_DOC_MAINTENANCEFactoryComponent },
            { path: 'IDD_SALESDOC_DEL_WIZARD', component: IDD_SALESDOC_DEL_WIZARDFactoryComponent },
            { path: 'IDD_LOAD_DN', component: IDD_LOAD_DNFactoryComponent },
            { path: 'IDD_UPDATE_ACTUAL', component: IDD_UPDATE_ACTUALFactoryComponent },
            { path: 'IDD_LOAD_BOL', component: IDD_LOAD_BOLFactoryComponent },
            { path: 'IDD_LOAD_INV', component: IDD_LOAD_INVFactoryComponent },
            { path: 'IDD_SALE_DOCUMENT', component: IDD_SALE_DOCUMENTFactoryComponent },
            { path: 'IDD_INTERSTORAGE', component: IDD_INTERSTORAGEFactoryComponent },
            { path: 'IDD_LOADER_SALE_ORD', component: IDD_LOADER_SALE_ORDFactoryComponent },
            { path: 'IDD_LOADER_PROFORMA_INVOICE', component: IDD_LOADER_PROFORMA_INVOICEFactoryComponent },
            { path: 'IDD_LOADER_PL', component: IDD_LOADER_PLFactoryComponent },
            { path: 'IDD_LOADER_DOCUMENT_FILTER', component: IDD_LOADER_DOCUMENT_FILTERFactoryComponent },
            { path: 'IDD_LOADER_DN', component: IDD_LOADER_DNFactoryComponent },
            { path: 'IDD_NOTA_FISCAL_CUSTOMER', component: IDD_NOTA_FISCAL_CUSTOMERFactoryComponent },
            { path: 'IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCAL', component: IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCALFactoryComponent },
            { path: 'IDD_WIZVIEW_DEFINV_RFC', component: IDD_WIZVIEW_DEFINV_RFCFactoryComponent },
            { path: 'IDD_WIZVIEW_DEFINV_RECEIPT', component: IDD_WIZVIEW_DEFINV_RECEIPTFactoryComponent },
            { path: 'IDD_WIZVIEW_DEFINV_PL', component: IDD_WIZVIEW_DEFINV_PLFactoryComponent },
            { path: 'IDD_WIZVIEW_DEFINV', component: IDD_WIZVIEW_DEFINVFactoryComponent },
            { path: 'IDD_PD_DOC_CASH', component: IDD_PD_DOC_CASHFactoryComponent },
        ])],
    declarations: [
            IDD_SALES_COMPANYUSER_SETTINGSComponent, IDD_SALES_COMPANYUSER_SETTINGSFactoryComponent,
            IDD_SALES_PARAMETERSComponent, IDD_SALES_PARAMETERSFactoryComponent,
            IDD_SALE_ORD_FULFILLMENTComponent, IDD_SALE_ORD_FULFILLMENTFactoryComponent,
            IDD_SALE_ORD_EDITComponent, IDD_SALE_ORD_EDITFactoryComponent,
            IDD_LOAD_RFCComponent, IDD_LOAD_RFCFactoryComponent,
            IDD_PICKED_REBUILDINGComponent, IDD_PICKED_REBUILDINGFactoryComponent,
            IDD_GROUPING_CODESComponent, IDD_GROUPING_CODESFactoryComponent,
            IDD_WIZ_SALES_POSTDOC_MAINComponent, IDD_WIZ_SALES_POSTDOC_MAINFactoryComponent,
            IDD_WIZ_POSTING_NFC_MAINComponent, IDD_WIZ_POSTING_NFC_MAINFactoryComponent,
            IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAINComponent, IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAINFactoryComponent,
            IDD_SALE_DOC_MAINTENANCEComponent, IDD_SALE_DOC_MAINTENANCEFactoryComponent,
            IDD_SALESDOC_DEL_WIZARDComponent, IDD_SALESDOC_DEL_WIZARDFactoryComponent,
            IDD_LOAD_DNComponent, IDD_LOAD_DNFactoryComponent,
            IDD_UPDATE_ACTUALComponent, IDD_UPDATE_ACTUALFactoryComponent,
            IDD_LOAD_BOLComponent, IDD_LOAD_BOLFactoryComponent,
            IDD_LOAD_INVComponent, IDD_LOAD_INVFactoryComponent,
            IDD_SALE_DOCUMENTComponent, IDD_SALE_DOCUMENTFactoryComponent,
            IDD_INTERSTORAGEComponent, IDD_INTERSTORAGEFactoryComponent,
            IDD_LOADER_SALE_ORDComponent, IDD_LOADER_SALE_ORDFactoryComponent,
            IDD_LOADER_PROFORMA_INVOICEComponent, IDD_LOADER_PROFORMA_INVOICEFactoryComponent,
            IDD_LOADER_PLComponent, IDD_LOADER_PLFactoryComponent,
            IDD_LOADER_DOCUMENT_FILTERComponent, IDD_LOADER_DOCUMENT_FILTERFactoryComponent,
            IDD_LOADER_DNComponent, IDD_LOADER_DNFactoryComponent,
            IDD_NOTA_FISCAL_CUSTOMERComponent, IDD_NOTA_FISCAL_CUSTOMERFactoryComponent,
            IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCALComponent, IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCALFactoryComponent,
            IDD_WIZVIEW_DEFINV_RFCComponent, IDD_WIZVIEW_DEFINV_RFCFactoryComponent,
            IDD_WIZVIEW_DEFINV_RECEIPTComponent, IDD_WIZVIEW_DEFINV_RECEIPTFactoryComponent,
            IDD_WIZVIEW_DEFINV_PLComponent, IDD_WIZVIEW_DEFINV_PLFactoryComponent,
            IDD_WIZVIEW_DEFINVComponent, IDD_WIZVIEW_DEFINVFactoryComponent,
            IDD_PD_DOC_CASHComponent, IDD_PD_DOC_CASHFactoryComponent,
    ],
    exports: [
            IDD_SALES_COMPANYUSER_SETTINGSFactoryComponent,
            IDD_SALES_PARAMETERSFactoryComponent,
            IDD_SALE_ORD_FULFILLMENTFactoryComponent,
            IDD_SALE_ORD_EDITFactoryComponent,
            IDD_LOAD_RFCFactoryComponent,
            IDD_PICKED_REBUILDINGFactoryComponent,
            IDD_GROUPING_CODESFactoryComponent,
            IDD_WIZ_SALES_POSTDOC_MAINFactoryComponent,
            IDD_WIZ_POSTING_NFC_MAINFactoryComponent,
            IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAINFactoryComponent,
            IDD_SALE_DOC_MAINTENANCEFactoryComponent,
            IDD_SALESDOC_DEL_WIZARDFactoryComponent,
            IDD_LOAD_DNFactoryComponent,
            IDD_UPDATE_ACTUALFactoryComponent,
            IDD_LOAD_BOLFactoryComponent,
            IDD_LOAD_INVFactoryComponent,
            IDD_SALE_DOCUMENTFactoryComponent,
            IDD_INTERSTORAGEFactoryComponent,
            IDD_LOADER_SALE_ORDFactoryComponent,
            IDD_LOADER_PROFORMA_INVOICEFactoryComponent,
            IDD_LOADER_PLFactoryComponent,
            IDD_LOADER_DOCUMENT_FILTERFactoryComponent,
            IDD_LOADER_DNFactoryComponent,
            IDD_NOTA_FISCAL_CUSTOMERFactoryComponent,
            IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCALFactoryComponent,
            IDD_WIZVIEW_DEFINV_RFCFactoryComponent,
            IDD_WIZVIEW_DEFINV_RECEIPTFactoryComponent,
            IDD_WIZVIEW_DEFINV_PLFactoryComponent,
            IDD_WIZVIEW_DEFINVFactoryComponent,
            IDD_PD_DOC_CASHFactoryComponent,
    ],
    entryComponents: [
            IDD_SALES_COMPANYUSER_SETTINGSComponent,
            IDD_SALES_PARAMETERSComponent,
            IDD_SALE_ORD_FULFILLMENTComponent,
            IDD_SALE_ORD_EDITComponent,
            IDD_LOAD_RFCComponent,
            IDD_PICKED_REBUILDINGComponent,
            IDD_GROUPING_CODESComponent,
            IDD_WIZ_SALES_POSTDOC_MAINComponent,
            IDD_WIZ_POSTING_NFC_MAINComponent,
            IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAINComponent,
            IDD_SALE_DOC_MAINTENANCEComponent,
            IDD_SALESDOC_DEL_WIZARDComponent,
            IDD_LOAD_DNComponent,
            IDD_UPDATE_ACTUALComponent,
            IDD_LOAD_BOLComponent,
            IDD_LOAD_INVComponent,
            IDD_SALE_DOCUMENTComponent,
            IDD_INTERSTORAGEComponent,
            IDD_LOADER_SALE_ORDComponent,
            IDD_LOADER_PROFORMA_INVOICEComponent,
            IDD_LOADER_PLComponent,
            IDD_LOADER_DOCUMENT_FILTERComponent,
            IDD_LOADER_DNComponent,
            IDD_NOTA_FISCAL_CUSTOMERComponent,
            IDD_DOC_COMPLETE_MSG_FRAME_NOTA_FISCALComponent,
            IDD_WIZVIEW_DEFINV_RFCComponent,
            IDD_WIZVIEW_DEFINV_RECEIPTComponent,
            IDD_WIZVIEW_DEFINV_PLComponent,
            IDD_WIZVIEW_DEFINVComponent,
            IDD_PD_DOC_CASHComponent,
    ]
})


export class SalesModule { };