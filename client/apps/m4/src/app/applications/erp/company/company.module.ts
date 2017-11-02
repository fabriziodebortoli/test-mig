import { IDD_XGATEPARAMETERS_FOR_DOCTYPEComponent, IDD_XGATEPARAMETERS_FOR_DOCTYPEFactoryComponent } from './xgateparametersfordoctype/IDD_XGATEPARAMETERS_FOR_DOCTYPE.component';
import { IDD_XGATE_PARAMComponent, IDD_XGATE_PARAMFactoryComponent } from './xgateparameters/IDD_XGATE_PARAM.component';
import { IDD_TTITLESComponent, IDD_TTITLESFactoryComponent } from './titles/IDD_TTITLES.component';
import { IDD_TAXCODESUPDATEComponent, IDD_TAXCODESUPDATEFactoryComponent } from './taxcodesupdate/IDD_TAXCODESUPDATE.component';
import { IDD_TAX_CODES_DEFAULTSComponent, IDD_TAX_CODES_DEFAULTSFactoryComponent } from './taxcodesdefaults/IDD_TAX_CODES_DEFAULTS.component';
import { IDD_TAXComponent, IDD_TAXFactoryComponent } from './tax/IDD_TAX.component';
import { IDD_COPYTAXComponent, IDD_COPYTAXFactoryComponent } from './tax/IDD_COPYTAX.component';
import { IDD_REPORTMULTICOPIESComponent, IDD_REPORTMULTICOPIESFactoryComponent } from './reportmulticopies/IDD_REPORTMULTICOPIES.component';
import { IDD_PROJECT_CODESComponent, IDD_PROJECT_CODESFactoryComponent } from './projectcodes/IDD_PROJECT_CODES.component';
import { IDD_FISCALYEARGENERATIONComponent, IDD_FISCALYEARGENERATIONFactoryComponent } from './newyeargeneration/IDD_FISCALYEARGENERATION.component';
import { IDD_CONVPRECISIONTIMEComponent, IDD_CONVPRECISIONTIMEFactoryComponent } from './modifyelapsedtimeprecision/IDD_CONVPRECISIONTIME.component';
import { IDD_MAILComponent, IDD_MAILFactoryComponent } from './mailparameters/IDD_MAIL.component';
import { IDD_ISO_COUNTRIESComponent, IDD_ISO_COUNTRIESFactoryComponent } from './isocountrycodes/IDD_ISO_COUNTRIES.component';
import { IDD_PARAMETERS_CURRENCIESComponent, IDD_PARAMETERS_CURRENCIESFactoryComponent } from './currenciesparameters/IDD_PARAMETERS_CURRENCIES.component';
import { IDD_CURRENCIESComponent, IDD_CURRENCIESFactoryComponent } from './currencies/IDD_CURRENCIES.component';
import { IDD_CONTRACT_CODESComponent, IDD_CONTRACT_CODESFactoryComponent } from './contractcodes/IDD_CONTRACT_CODES.component';
import { IDD_COMPANYComponent, IDD_COMPANYFactoryComponent } from './company/IDD_COMPANY.component';
import { IDD_ACTIVITYCODESComponent, IDD_ACTIVITYCODESFactoryComponent } from './activitycodes/IDD_ACTIVITYCODES.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_XGATEPARAMETERS_FOR_DOCTYPE', component: IDD_XGATEPARAMETERS_FOR_DOCTYPEFactoryComponent },
            { path: 'IDD_XGATE_PARAM', component: IDD_XGATE_PARAMFactoryComponent },
            { path: 'IDD_TTITLES', component: IDD_TTITLESFactoryComponent },
            { path: 'IDD_TAXCODESUPDATE', component: IDD_TAXCODESUPDATEFactoryComponent },
            { path: 'IDD_TAX_CODES_DEFAULTS', component: IDD_TAX_CODES_DEFAULTSFactoryComponent },
            { path: 'IDD_TAX', component: IDD_TAXFactoryComponent },
            { path: 'IDD_COPYTAX', component: IDD_COPYTAXFactoryComponent },
            { path: 'IDD_REPORTMULTICOPIES', component: IDD_REPORTMULTICOPIESFactoryComponent },
            { path: 'IDD_PROJECT_CODES', component: IDD_PROJECT_CODESFactoryComponent },
            { path: 'IDD_FISCALYEARGENERATION', component: IDD_FISCALYEARGENERATIONFactoryComponent },
            { path: 'IDD_CONVPRECISIONTIME', component: IDD_CONVPRECISIONTIMEFactoryComponent },
            { path: 'IDD_MAIL', component: IDD_MAILFactoryComponent },
            { path: 'IDD_ISO_COUNTRIES', component: IDD_ISO_COUNTRIESFactoryComponent },
            { path: 'IDD_PARAMETERS_CURRENCIES', component: IDD_PARAMETERS_CURRENCIESFactoryComponent },
            { path: 'IDD_CURRENCIES', component: IDD_CURRENCIESFactoryComponent },
            { path: 'IDD_CONTRACT_CODES', component: IDD_CONTRACT_CODESFactoryComponent },
            { path: 'IDD_COMPANY', component: IDD_COMPANYFactoryComponent },
            { path: 'IDD_ACTIVITYCODES', component: IDD_ACTIVITYCODESFactoryComponent },
        ])],
    declarations: [
            IDD_XGATEPARAMETERS_FOR_DOCTYPEComponent, IDD_XGATEPARAMETERS_FOR_DOCTYPEFactoryComponent,
            IDD_XGATE_PARAMComponent, IDD_XGATE_PARAMFactoryComponent,
            IDD_TTITLESComponent, IDD_TTITLESFactoryComponent,
            IDD_TAXCODESUPDATEComponent, IDD_TAXCODESUPDATEFactoryComponent,
            IDD_TAX_CODES_DEFAULTSComponent, IDD_TAX_CODES_DEFAULTSFactoryComponent,
            IDD_TAXComponent, IDD_TAXFactoryComponent,
            IDD_COPYTAXComponent, IDD_COPYTAXFactoryComponent,
            IDD_REPORTMULTICOPIESComponent, IDD_REPORTMULTICOPIESFactoryComponent,
            IDD_PROJECT_CODESComponent, IDD_PROJECT_CODESFactoryComponent,
            IDD_FISCALYEARGENERATIONComponent, IDD_FISCALYEARGENERATIONFactoryComponent,
            IDD_CONVPRECISIONTIMEComponent, IDD_CONVPRECISIONTIMEFactoryComponent,
            IDD_MAILComponent, IDD_MAILFactoryComponent,
            IDD_ISO_COUNTRIESComponent, IDD_ISO_COUNTRIESFactoryComponent,
            IDD_PARAMETERS_CURRENCIESComponent, IDD_PARAMETERS_CURRENCIESFactoryComponent,
            IDD_CURRENCIESComponent, IDD_CURRENCIESFactoryComponent,
            IDD_CONTRACT_CODESComponent, IDD_CONTRACT_CODESFactoryComponent,
            IDD_COMPANYComponent, IDD_COMPANYFactoryComponent,
            IDD_ACTIVITYCODESComponent, IDD_ACTIVITYCODESFactoryComponent,
    ],
    exports: [
            IDD_XGATEPARAMETERS_FOR_DOCTYPEFactoryComponent,
            IDD_XGATE_PARAMFactoryComponent,
            IDD_TTITLESFactoryComponent,
            IDD_TAXCODESUPDATEFactoryComponent,
            IDD_TAX_CODES_DEFAULTSFactoryComponent,
            IDD_TAXFactoryComponent,
            IDD_COPYTAXFactoryComponent,
            IDD_REPORTMULTICOPIESFactoryComponent,
            IDD_PROJECT_CODESFactoryComponent,
            IDD_FISCALYEARGENERATIONFactoryComponent,
            IDD_CONVPRECISIONTIMEFactoryComponent,
            IDD_MAILFactoryComponent,
            IDD_ISO_COUNTRIESFactoryComponent,
            IDD_PARAMETERS_CURRENCIESFactoryComponent,
            IDD_CURRENCIESFactoryComponent,
            IDD_CONTRACT_CODESFactoryComponent,
            IDD_COMPANYFactoryComponent,
            IDD_ACTIVITYCODESFactoryComponent,
    ],
    entryComponents: [
            IDD_XGATEPARAMETERS_FOR_DOCTYPEComponent,
            IDD_XGATE_PARAMComponent,
            IDD_TTITLESComponent,
            IDD_TAXCODESUPDATEComponent,
            IDD_TAX_CODES_DEFAULTSComponent,
            IDD_TAXComponent,
            IDD_COPYTAXComponent,
            IDD_REPORTMULTICOPIESComponent,
            IDD_PROJECT_CODESComponent,
            IDD_FISCALYEARGENERATIONComponent,
            IDD_CONVPRECISIONTIMEComponent,
            IDD_MAILComponent,
            IDD_ISO_COUNTRIESComponent,
            IDD_PARAMETERS_CURRENCIESComponent,
            IDD_CURRENCIESComponent,
            IDD_CONTRACT_CODESComponent,
            IDD_COMPANYComponent,
            IDD_ACTIVITYCODESComponent,
    ]
})


export class CompanyModule { };