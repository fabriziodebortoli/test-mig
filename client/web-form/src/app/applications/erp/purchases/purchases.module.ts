import { IDD_UPDATE_ACTUAL_PURCHASEComponent, IDD_UPDATE_ACTUAL_PURCHASEFactoryComponent } from './supplieractualupdating/IDD_UPDATE_ACTUAL_PURCHASE.component';
import { IDD_LOAD_RTSComponent, IDD_LOAD_RTSFactoryComponent } from './rtsloading/IDD_LOAD_RTS.component';
import { IDD_RETURNEDMATERIALComponent, IDD_RETURNEDMATERIALFactoryComponent } from './returntosupplierfrombol/IDD_RETURNEDMATERIAL.component';
import { IDD_PURCH_POSTDOC_WIZARDComponent, IDD_PURCH_POSTDOC_WIZARDFactoryComponent } from './purchdocposting/IDD_PURCH_POSTDOC_WIZARD.component';
import { IDD_PURCHASEDOC_MAINTENANCEComponent, IDD_PURCHASEDOC_MAINTENANCEFactoryComponent } from './purchdocmaintenance/IDD_PURCHASEDOC_MAINTENANCE.component';
import { IDD_PURCHASES_COMPANYUSER_SETTINGSComponent, IDD_PURCHASES_COMPANYUSER_SETTINGSFactoryComponent } from './purchasessettings/IDD_PURCHASES_COMPANYUSER_SETTINGS.component';
import { IDD_PARAMETERS_PURCHASESComponent, IDD_PARAMETERS_PURCHASESFactoryComponent } from './purchasesparameters/IDD_PARAMETERS_PURCHASES.component';
import { IDD_LOAD_PURCHASEORDERComponent, IDD_LOAD_PURCHASEORDERFactoryComponent } from './purchaseorderloading/IDD_LOAD_PURCHASEORDER.component';
import { IDD_PURCHASESDOC_DEL_WIZARDComponent, IDD_PURCHASESDOC_DEL_WIZARDFactoryComponent } from './purchasedocdeleting/IDD_PURCHASESDOC_DEL_WIZARD.component';
import { IDD_LEAD_TIME_CALCComponent, IDD_LEAD_TIME_CALCFactoryComponent } from './leadtimecalculation/IDD_LEAD_TIME_CALC.component';
import { IDD_NDB_LOAD_INVComponent, IDD_NDB_LOAD_INVFactoryComponent } from './invoiceloading/IDD_NDB_LOAD_INV.component';
import { IDD_IMPORT_DECLARATIONComponent, IDD_IMPORT_DECLARATIONFactoryComponent } from './importdeclloading/IDD_IMPORT_DECLARATION.component';
import { IDD_LOADBOLComponent, IDD_LOADBOLFactoryComponent } from './billofladingtoinvoiceloading/IDD_LOADBOL.component';
import { IDD_PURCH_LOAD_BOLComponent, IDD_PURCH_LOAD_BOLFactoryComponent } from './billofladingloading/IDD_PURCH_LOAD_BOL.component';
import { IDD_PURCHASE_DOCUMENT_INVOICEComponent, IDD_PURCHASE_DOCUMENT_INVOICEFactoryComponent } from './uipurchasedocview/IDD_PURCHASE_DOCUMENT_INVOICE.component';
import { IDD_PURCHASE_DOCUMENTComponent, IDD_PURCHASE_DOCUMENTFactoryComponent } from './uipurchasedocview/IDD_PURCHASE_DOCUMENT.component';
import { IDD_RTS_LOAD_FILTERComponent, IDD_RTS_LOAD_FILTERFactoryComponent } from './uipurchasedocloadfilters/IDD_RTS_LOAD_FILTER.component';
import { IDD_PURCHASE_LOADER_FILTERComponent, IDD_PURCHASE_LOADER_FILTERFactoryComponent } from './uipurchasedocloadfilters/IDD_PURCHASE_LOADER_FILTER.component';
import { IDD_PURCHASE_INVOICE_LOAD_FILTERComponent, IDD_PURCHASE_INVOICE_LOAD_FILTERFactoryComponent } from './uipurchasedocloadfilters/IDD_PURCHASE_INVOICE_LOAD_FILTER.component';
import { IDD_INVOICE_CORRECTION_LOAD_FILTERComponent, IDD_INVOICE_CORRECTION_LOAD_FILTERFactoryComponent } from './uipurchasedocloadfilters/IDD_INVOICE_CORRECTION_LOAD_FILTER.component';
import { IDD_BOL_LOAD_FILTERComponent, IDD_BOL_LOAD_FILTERFactoryComponent } from './uipurchasedocloadfilters/IDD_BOL_LOAD_FILTER.component';
import { IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAMEComponent, IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAMEFactoryComponent } from './uinotafiscalforsupplier/IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAME.component';
import { IDD_NFSComponent, IDD_NFSFactoryComponent } from './uinotafiscalforsupplier/IDD_NFS.component';
import { IDD_PD_PURCH_DOC_CASHComponent, IDD_PD_PURCH_DOC_CASHFactoryComponent } from './postingpayablesreceivables/IDD_PD_PURCH_DOC_CASH.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_UPDATE_ACTUAL_PURCHASE', component: IDD_UPDATE_ACTUAL_PURCHASEFactoryComponent },
            { path: 'IDD_LOAD_RTS', component: IDD_LOAD_RTSFactoryComponent },
            { path: 'IDD_RETURNEDMATERIAL', component: IDD_RETURNEDMATERIALFactoryComponent },
            { path: 'IDD_PURCH_POSTDOC_WIZARD', component: IDD_PURCH_POSTDOC_WIZARDFactoryComponent },
            { path: 'IDD_PURCHASEDOC_MAINTENANCE', component: IDD_PURCHASEDOC_MAINTENANCEFactoryComponent },
            { path: 'IDD_PURCHASES_COMPANYUSER_SETTINGS', component: IDD_PURCHASES_COMPANYUSER_SETTINGSFactoryComponent },
            { path: 'IDD_PARAMETERS_PURCHASES', component: IDD_PARAMETERS_PURCHASESFactoryComponent },
            { path: 'IDD_LOAD_PURCHASEORDER', component: IDD_LOAD_PURCHASEORDERFactoryComponent },
            { path: 'IDD_PURCHASESDOC_DEL_WIZARD', component: IDD_PURCHASESDOC_DEL_WIZARDFactoryComponent },
            { path: 'IDD_LEAD_TIME_CALC', component: IDD_LEAD_TIME_CALCFactoryComponent },
            { path: 'IDD_NDB_LOAD_INV', component: IDD_NDB_LOAD_INVFactoryComponent },
            { path: 'IDD_IMPORT_DECLARATION', component: IDD_IMPORT_DECLARATIONFactoryComponent },
            { path: 'IDD_LOADBOL', component: IDD_LOADBOLFactoryComponent },
            { path: 'IDD_PURCH_LOAD_BOL', component: IDD_PURCH_LOAD_BOLFactoryComponent },
            { path: 'IDD_PURCHASE_DOCUMENT_INVOICE', component: IDD_PURCHASE_DOCUMENT_INVOICEFactoryComponent },
            { path: 'IDD_PURCHASE_DOCUMENT', component: IDD_PURCHASE_DOCUMENTFactoryComponent },
            { path: 'IDD_RTS_LOAD_FILTER', component: IDD_RTS_LOAD_FILTERFactoryComponent },
            { path: 'IDD_PURCHASE_LOADER_FILTER', component: IDD_PURCHASE_LOADER_FILTERFactoryComponent },
            { path: 'IDD_PURCHASE_INVOICE_LOAD_FILTER', component: IDD_PURCHASE_INVOICE_LOAD_FILTERFactoryComponent },
            { path: 'IDD_INVOICE_CORRECTION_LOAD_FILTER', component: IDD_INVOICE_CORRECTION_LOAD_FILTERFactoryComponent },
            { path: 'IDD_BOL_LOAD_FILTER', component: IDD_BOL_LOAD_FILTERFactoryComponent },
            { path: 'IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAME', component: IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAMEFactoryComponent },
            { path: 'IDD_NFS', component: IDD_NFSFactoryComponent },
            { path: 'IDD_PD_PURCH_DOC_CASH', component: IDD_PD_PURCH_DOC_CASHFactoryComponent },
        ])],
    declarations: [
            IDD_UPDATE_ACTUAL_PURCHASEComponent, IDD_UPDATE_ACTUAL_PURCHASEFactoryComponent,
            IDD_LOAD_RTSComponent, IDD_LOAD_RTSFactoryComponent,
            IDD_RETURNEDMATERIALComponent, IDD_RETURNEDMATERIALFactoryComponent,
            IDD_PURCH_POSTDOC_WIZARDComponent, IDD_PURCH_POSTDOC_WIZARDFactoryComponent,
            IDD_PURCHASEDOC_MAINTENANCEComponent, IDD_PURCHASEDOC_MAINTENANCEFactoryComponent,
            IDD_PURCHASES_COMPANYUSER_SETTINGSComponent, IDD_PURCHASES_COMPANYUSER_SETTINGSFactoryComponent,
            IDD_PARAMETERS_PURCHASESComponent, IDD_PARAMETERS_PURCHASESFactoryComponent,
            IDD_LOAD_PURCHASEORDERComponent, IDD_LOAD_PURCHASEORDERFactoryComponent,
            IDD_PURCHASESDOC_DEL_WIZARDComponent, IDD_PURCHASESDOC_DEL_WIZARDFactoryComponent,
            IDD_LEAD_TIME_CALCComponent, IDD_LEAD_TIME_CALCFactoryComponent,
            IDD_NDB_LOAD_INVComponent, IDD_NDB_LOAD_INVFactoryComponent,
            IDD_IMPORT_DECLARATIONComponent, IDD_IMPORT_DECLARATIONFactoryComponent,
            IDD_LOADBOLComponent, IDD_LOADBOLFactoryComponent,
            IDD_PURCH_LOAD_BOLComponent, IDD_PURCH_LOAD_BOLFactoryComponent,
            IDD_PURCHASE_DOCUMENT_INVOICEComponent, IDD_PURCHASE_DOCUMENT_INVOICEFactoryComponent,
            IDD_PURCHASE_DOCUMENTComponent, IDD_PURCHASE_DOCUMENTFactoryComponent,
            IDD_RTS_LOAD_FILTERComponent, IDD_RTS_LOAD_FILTERFactoryComponent,
            IDD_PURCHASE_LOADER_FILTERComponent, IDD_PURCHASE_LOADER_FILTERFactoryComponent,
            IDD_PURCHASE_INVOICE_LOAD_FILTERComponent, IDD_PURCHASE_INVOICE_LOAD_FILTERFactoryComponent,
            IDD_INVOICE_CORRECTION_LOAD_FILTERComponent, IDD_INVOICE_CORRECTION_LOAD_FILTERFactoryComponent,
            IDD_BOL_LOAD_FILTERComponent, IDD_BOL_LOAD_FILTERFactoryComponent,
            IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAMEComponent, IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAMEFactoryComponent,
            IDD_NFSComponent, IDD_NFSFactoryComponent,
            IDD_PD_PURCH_DOC_CASHComponent, IDD_PD_PURCH_DOC_CASHFactoryComponent,
    ],
    exports: [
            IDD_UPDATE_ACTUAL_PURCHASEFactoryComponent,
            IDD_LOAD_RTSFactoryComponent,
            IDD_RETURNEDMATERIALFactoryComponent,
            IDD_PURCH_POSTDOC_WIZARDFactoryComponent,
            IDD_PURCHASEDOC_MAINTENANCEFactoryComponent,
            IDD_PURCHASES_COMPANYUSER_SETTINGSFactoryComponent,
            IDD_PARAMETERS_PURCHASESFactoryComponent,
            IDD_LOAD_PURCHASEORDERFactoryComponent,
            IDD_PURCHASESDOC_DEL_WIZARDFactoryComponent,
            IDD_LEAD_TIME_CALCFactoryComponent,
            IDD_NDB_LOAD_INVFactoryComponent,
            IDD_IMPORT_DECLARATIONFactoryComponent,
            IDD_LOADBOLFactoryComponent,
            IDD_PURCH_LOAD_BOLFactoryComponent,
            IDD_PURCHASE_DOCUMENT_INVOICEFactoryComponent,
            IDD_PURCHASE_DOCUMENTFactoryComponent,
            IDD_RTS_LOAD_FILTERFactoryComponent,
            IDD_PURCHASE_LOADER_FILTERFactoryComponent,
            IDD_PURCHASE_INVOICE_LOAD_FILTERFactoryComponent,
            IDD_INVOICE_CORRECTION_LOAD_FILTERFactoryComponent,
            IDD_BOL_LOAD_FILTERFactoryComponent,
            IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAMEFactoryComponent,
            IDD_NFSFactoryComponent,
            IDD_PD_PURCH_DOC_CASHFactoryComponent,
    ],
    entryComponents: [
            IDD_UPDATE_ACTUAL_PURCHASEComponent,
            IDD_LOAD_RTSComponent,
            IDD_RETURNEDMATERIALComponent,
            IDD_PURCH_POSTDOC_WIZARDComponent,
            IDD_PURCHASEDOC_MAINTENANCEComponent,
            IDD_PURCHASES_COMPANYUSER_SETTINGSComponent,
            IDD_PARAMETERS_PURCHASESComponent,
            IDD_LOAD_PURCHASEORDERComponent,
            IDD_PURCHASESDOC_DEL_WIZARDComponent,
            IDD_LEAD_TIME_CALCComponent,
            IDD_NDB_LOAD_INVComponent,
            IDD_IMPORT_DECLARATIONComponent,
            IDD_LOADBOLComponent,
            IDD_PURCH_LOAD_BOLComponent,
            IDD_PURCHASE_DOCUMENT_INVOICEComponent,
            IDD_PURCHASE_DOCUMENTComponent,
            IDD_RTS_LOAD_FILTERComponent,
            IDD_PURCHASE_LOADER_FILTERComponent,
            IDD_PURCHASE_INVOICE_LOAD_FILTERComponent,
            IDD_INVOICE_CORRECTION_LOAD_FILTERComponent,
            IDD_BOL_LOAD_FILTERComponent,
            IDD_NOTA_FISCAL_FOR_SUPPLIER_COMPLETE_MSG_FRAMEComponent,
            IDD_NFSComponent,
            IDD_PD_PURCH_DOC_CASHComponent,
    ]
})


export class PurchasesModule { };