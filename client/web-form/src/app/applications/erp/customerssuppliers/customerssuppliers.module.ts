import { IDD_SUPPLIERSPECIFICATIONComponent, IDD_SUPPLIERSPECIFICATIONFactoryComponent } from './supplierspecification/IDD_SUPPLIERSPECIFICATION.component';
import { IDD_SUPPCTGComponent, IDD_SUPPCTGFactoryComponent } from './supplierscategories/IDD_SUPPCTG.component';
import { IDD_SUPPLIERCLASSIFICATIONComponent, IDD_SUPPLIERCLASSIFICATIONFactoryComponent } from './supplierclassification/IDD_SUPPLIERCLASSIFICATION.component';
import { IDD_CALC_FISCALCODEComponent, IDD_CALC_FISCALCODEFactoryComponent } from './fiscalcodecalc/IDD_CALC_FISCALCODE.component';
import { IDD_DOWNLOAD_CASH_TAX_COMPANIESComponent, IDD_DOWNLOAD_CASH_TAX_COMPANIESFactoryComponent } from './downloadcashtaxcompanies/IDD_DOWNLOAD_CASH_TAX_COMPANIES.component';
import { IDD_PARAMETERS_CUSTOMERSSUPPLIERSComponent, IDD_PARAMETERS_CUSTOMERSSUPPLIERSFactoryComponent } from './custsuppparameters/IDD_PARAMETERS_CUSTOMERSSUPPLIERS.component';
import { IDD_CUSTOMERSPECIFICATIONComponent, IDD_CUSTOMERSPECIFICATIONFactoryComponent } from './customerspecification/IDD_CUSTOMERSPECIFICATION.component';
import { IDD_CUSTCTGComponent, IDD_CUSTCTGFactoryComponent } from './customerscategories/IDD_CUSTCTG.component';
import { IDD_CUSTOMERCLASSIFICATIONComponent, IDD_CUSTOMERCLASSIFICATIONFactoryComponent } from './customerclassification/IDD_CUSTOMERCLASSIFICATION.component';
import { IDD_CIRCULARLETTERTEMPLATESComponent, IDD_CIRCULARLETTERTEMPLATESFactoryComponent } from './circularlettertemplates/IDD_CIRCULARLETTERTEMPLATES.component';
import { IDD_STPRIVACYSTATEMENTComponent, IDD_STPRIVACYSTATEMENTFactoryComponent } from './uiprivacystatementprint/IDD_STPRIVACYSTATEMENT.component';
import { IDD_SDDMANDATEPRINTComponent, IDD_SDDMANDATEPRINTFactoryComponent } from './uiprivacystatementprint/IDD_SDDMANDATEPRINT.component';
import { IDD_PRINTINTENTLETTERComponent, IDD_PRINTINTENTLETTERFactoryComponent } from './uiprivacystatementprint/IDD_PRINTINTENTLETTER.component';
import { IDD_PRINTINTENTANNULMENTComponent, IDD_PRINTINTENTANNULMENTFactoryComponent } from './uiprivacystatementprint/IDD_PRINTINTENTANNULMENT.component';
import { IDD_PRINTCIRCULARLETTERComponent, IDD_PRINTCIRCULARLETTERFactoryComponent } from './uiprivacystatementprint/IDD_PRINTCIRCULARLETTER.component';
import { IDD_IBANUPDATEComponent, IDD_IBANUPDATEFactoryComponent } from './uiprivacystatementprint/IDD_IBANUPDATE.component';
import { IDD_BUDGETCOPYComponent, IDD_BUDGETCOPYFactoryComponent } from './uiprivacystatementprint/IDD_BUDGETCOPY.component';
import { IDD_SUPPLIERS_COMPLETEComponent, IDD_SUPPLIERS_COMPLETEFactoryComponent } from './uicustomerssuppliers/IDD_SUPPLIERS_COMPLETE.component';
import { IDD_CUSTSUPPCOPYComponent, IDD_CUSTSUPPCOPYFactoryComponent } from './uicustomerssuppliers/IDD_CUSTSUPPCOPY.component';
import { IDD_CUSTOMERS_COMPLETEComponent, IDD_CUSTOMERS_COMPLETEFactoryComponent } from './uicustomerssuppliers/IDD_CUSTOMERS_COMPLETE.component';
import { IDD_CS_SUMMARIZEDComponent, IDD_CS_SUMMARIZEDFactoryComponent } from './uicustomerssuppliers/IDD_CS_SUMMARIZED.component';
import { IDD_CS_BRANCHES_ADD_ON_FLYComponent, IDD_CS_BRANCHES_ADD_ON_FLYFactoryComponent } from './uicustomerssuppliers/IDD_CS_BRANCHES_ADD_ON_FLY.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_SUPPLIERSPECIFICATION', component: IDD_SUPPLIERSPECIFICATIONFactoryComponent },
            { path: 'IDD_SUPPCTG', component: IDD_SUPPCTGFactoryComponent },
            { path: 'IDD_SUPPLIERCLASSIFICATION', component: IDD_SUPPLIERCLASSIFICATIONFactoryComponent },
            { path: 'IDD_CALC_FISCALCODE', component: IDD_CALC_FISCALCODEFactoryComponent },
            { path: 'IDD_DOWNLOAD_CASH_TAX_COMPANIES', component: IDD_DOWNLOAD_CASH_TAX_COMPANIESFactoryComponent },
            { path: 'IDD_PARAMETERS_CUSTOMERSSUPPLIERS', component: IDD_PARAMETERS_CUSTOMERSSUPPLIERSFactoryComponent },
            { path: 'IDD_CUSTOMERSPECIFICATION', component: IDD_CUSTOMERSPECIFICATIONFactoryComponent },
            { path: 'IDD_CUSTCTG', component: IDD_CUSTCTGFactoryComponent },
            { path: 'IDD_CUSTOMERCLASSIFICATION', component: IDD_CUSTOMERCLASSIFICATIONFactoryComponent },
            { path: 'IDD_CIRCULARLETTERTEMPLATES', component: IDD_CIRCULARLETTERTEMPLATESFactoryComponent },
            { path: 'IDD_STPRIVACYSTATEMENT', component: IDD_STPRIVACYSTATEMENTFactoryComponent },
            { path: 'IDD_SDDMANDATEPRINT', component: IDD_SDDMANDATEPRINTFactoryComponent },
            { path: 'IDD_PRINTINTENTLETTER', component: IDD_PRINTINTENTLETTERFactoryComponent },
            { path: 'IDD_PRINTINTENTANNULMENT', component: IDD_PRINTINTENTANNULMENTFactoryComponent },
            { path: 'IDD_PRINTCIRCULARLETTER', component: IDD_PRINTCIRCULARLETTERFactoryComponent },
            { path: 'IDD_IBANUPDATE', component: IDD_IBANUPDATEFactoryComponent },
            { path: 'IDD_BUDGETCOPY', component: IDD_BUDGETCOPYFactoryComponent },
            { path: 'IDD_SUPPLIERS_COMPLETE', component: IDD_SUPPLIERS_COMPLETEFactoryComponent },
            { path: 'IDD_CUSTSUPPCOPY', component: IDD_CUSTSUPPCOPYFactoryComponent },
            { path: 'IDD_CUSTOMERS_COMPLETE', component: IDD_CUSTOMERS_COMPLETEFactoryComponent },
            { path: 'IDD_CS_SUMMARIZED', component: IDD_CS_SUMMARIZEDFactoryComponent },
            { path: 'IDD_CS_BRANCHES_ADD_ON_FLY', component: IDD_CS_BRANCHES_ADD_ON_FLYFactoryComponent },
        ])],
    declarations: [
            IDD_SUPPLIERSPECIFICATIONComponent, IDD_SUPPLIERSPECIFICATIONFactoryComponent,
            IDD_SUPPCTGComponent, IDD_SUPPCTGFactoryComponent,
            IDD_SUPPLIERCLASSIFICATIONComponent, IDD_SUPPLIERCLASSIFICATIONFactoryComponent,
            IDD_CALC_FISCALCODEComponent, IDD_CALC_FISCALCODEFactoryComponent,
            IDD_DOWNLOAD_CASH_TAX_COMPANIESComponent, IDD_DOWNLOAD_CASH_TAX_COMPANIESFactoryComponent,
            IDD_PARAMETERS_CUSTOMERSSUPPLIERSComponent, IDD_PARAMETERS_CUSTOMERSSUPPLIERSFactoryComponent,
            IDD_CUSTOMERSPECIFICATIONComponent, IDD_CUSTOMERSPECIFICATIONFactoryComponent,
            IDD_CUSTCTGComponent, IDD_CUSTCTGFactoryComponent,
            IDD_CUSTOMERCLASSIFICATIONComponent, IDD_CUSTOMERCLASSIFICATIONFactoryComponent,
            IDD_CIRCULARLETTERTEMPLATESComponent, IDD_CIRCULARLETTERTEMPLATESFactoryComponent,
            IDD_STPRIVACYSTATEMENTComponent, IDD_STPRIVACYSTATEMENTFactoryComponent,
            IDD_SDDMANDATEPRINTComponent, IDD_SDDMANDATEPRINTFactoryComponent,
            IDD_PRINTINTENTLETTERComponent, IDD_PRINTINTENTLETTERFactoryComponent,
            IDD_PRINTINTENTANNULMENTComponent, IDD_PRINTINTENTANNULMENTFactoryComponent,
            IDD_PRINTCIRCULARLETTERComponent, IDD_PRINTCIRCULARLETTERFactoryComponent,
            IDD_IBANUPDATEComponent, IDD_IBANUPDATEFactoryComponent,
            IDD_BUDGETCOPYComponent, IDD_BUDGETCOPYFactoryComponent,
            IDD_SUPPLIERS_COMPLETEComponent, IDD_SUPPLIERS_COMPLETEFactoryComponent,
            IDD_CUSTSUPPCOPYComponent, IDD_CUSTSUPPCOPYFactoryComponent,
            IDD_CUSTOMERS_COMPLETEComponent, IDD_CUSTOMERS_COMPLETEFactoryComponent,
            IDD_CS_SUMMARIZEDComponent, IDD_CS_SUMMARIZEDFactoryComponent,
            IDD_CS_BRANCHES_ADD_ON_FLYComponent, IDD_CS_BRANCHES_ADD_ON_FLYFactoryComponent,
    ],
    exports: [
            IDD_SUPPLIERSPECIFICATIONFactoryComponent,
            IDD_SUPPCTGFactoryComponent,
            IDD_SUPPLIERCLASSIFICATIONFactoryComponent,
            IDD_CALC_FISCALCODEFactoryComponent,
            IDD_DOWNLOAD_CASH_TAX_COMPANIESFactoryComponent,
            IDD_PARAMETERS_CUSTOMERSSUPPLIERSFactoryComponent,
            IDD_CUSTOMERSPECIFICATIONFactoryComponent,
            IDD_CUSTCTGFactoryComponent,
            IDD_CUSTOMERCLASSIFICATIONFactoryComponent,
            IDD_CIRCULARLETTERTEMPLATESFactoryComponent,
            IDD_STPRIVACYSTATEMENTFactoryComponent,
            IDD_SDDMANDATEPRINTFactoryComponent,
            IDD_PRINTINTENTLETTERFactoryComponent,
            IDD_PRINTINTENTANNULMENTFactoryComponent,
            IDD_PRINTCIRCULARLETTERFactoryComponent,
            IDD_IBANUPDATEFactoryComponent,
            IDD_BUDGETCOPYFactoryComponent,
            IDD_SUPPLIERS_COMPLETEFactoryComponent,
            IDD_CUSTSUPPCOPYFactoryComponent,
            IDD_CUSTOMERS_COMPLETEFactoryComponent,
            IDD_CS_SUMMARIZEDFactoryComponent,
            IDD_CS_BRANCHES_ADD_ON_FLYFactoryComponent,
    ],
    entryComponents: [
            IDD_SUPPLIERSPECIFICATIONComponent,
            IDD_SUPPCTGComponent,
            IDD_SUPPLIERCLASSIFICATIONComponent,
            IDD_CALC_FISCALCODEComponent,
            IDD_DOWNLOAD_CASH_TAX_COMPANIESComponent,
            IDD_PARAMETERS_CUSTOMERSSUPPLIERSComponent,
            IDD_CUSTOMERSPECIFICATIONComponent,
            IDD_CUSTCTGComponent,
            IDD_CUSTOMERCLASSIFICATIONComponent,
            IDD_CIRCULARLETTERTEMPLATESComponent,
            IDD_STPRIVACYSTATEMENTComponent,
            IDD_SDDMANDATEPRINTComponent,
            IDD_PRINTINTENTLETTERComponent,
            IDD_PRINTINTENTANNULMENTComponent,
            IDD_PRINTCIRCULARLETTERComponent,
            IDD_IBANUPDATEComponent,
            IDD_BUDGETCOPYComponent,
            IDD_SUPPLIERS_COMPLETEComponent,
            IDD_CUSTSUPPCOPYComponent,
            IDD_CUSTOMERS_COMPLETEComponent,
            IDD_CS_SUMMARIZEDComponent,
            IDD_CS_BRANCHES_ADD_ON_FLYComponent,
    ]
})


export class CustomersSuppliersModule { };