import { IDD_UPDATE_SALESPEOPLEComponent, IDD_UPDATE_SALESPEOPLEFactoryComponent } from './updatesalespeople/IDD_UPDATE_SALESPEOPLE.component';
import { IDD_PARAMETERS_SALESPEOPLEComponent, IDD_PARAMETERS_SALESPEOPLEFactoryComponent } from './salespeopleparameters/IDD_PARAMETERS_SALESPEOPLE.component';
import { IDD_SALESPEOPLE_FIRRComponent, IDD_SALESPEOPLE_FIRRFactoryComponent } from './salespeople/IDD_SALESPEOPLE_FIRR.component';
import { IDD_SALESPEOPLE_COMPLETEComponent, IDD_SALESPEOPLE_COMPLETEFactoryComponent } from './salespeople/IDD_SALESPEOPLE_COMPLETE.component';
import { IDD_SALESPEOPLE_ALLOWANCEComponent, IDD_SALESPEOPLE_ALLOWANCEFactoryComponent } from './salespeople/IDD_SALESPEOPLE_ALLOWANCE.component';
import { IDD_SALESPEOPLE_ADD_ON_FLYComponent, IDD_SALESPEOPLE_ADD_ON_FLYFactoryComponent } from './salespeople/IDD_SALESPEOPLE_ADD_ON_FLY.component';
import { IDD_FIRRCALCULATIONComponent, IDD_FIRRCALCULATIONFactoryComponent } from './firrcalculation/IDD_FIRRCALCULATION.component';
import { IDD_RECALENASARCO_FEESComponent, IDD_RECALENASARCO_FEESFactoryComponent } from './enasarcorebuilding/IDD_RECALENASARCO_FEES.component';
import { IDD_COMMISSIONS_SETTLEMENTComponent, IDD_COMMISSIONS_SETTLEMENTFactoryComponent } from './commissionssettlement/IDD_COMMISSIONS_SETTLEMENT.component';
import { IDD_COMMISSIONS_REBUILDINGComponent, IDD_COMMISSIONS_REBUILDINGFactoryComponent } from './commissionsrebuilding/IDD_COMMISSIONS_REBUILDING.component';
import { IDD_COMMISSIONS_GENERATIONComponent, IDD_COMMISSIONS_GENERATIONFactoryComponent } from './commissionsgeneration/IDD_COMMISSIONS_GENERATION.component';
import { IDD_COMMISSIONS_ENTRIES_DELETINGComponent, IDD_COMMISSIONS_ENTRIES_DELETINGFactoryComponent } from './commissionsentriesdeleting/IDD_COMMISSIONS_ENTRIES_DELETING.component';
import { IDD_ENTRYSALESPERSON_FULLComponent, IDD_ENTRYSALESPERSON_FULLFactoryComponent } from './commissionsentries/IDD_ENTRYSALESPERSON_FULL.component';
import { IDD_COMM_POLICIES_COPYComponent, IDD_COMM_POLICIES_COPYFactoryComponent } from './commissionpolicies/IDD_COMM_POLICIES_COPY.component';
import { IDD_COMM_POLICIESComponent, IDD_COMM_POLICIESFactoryComponent } from './commissionpolicies/IDD_COMM_POLICIES.component';
import { IDD_CTGPRO_HEADER_FULLComponent, IDD_CTGPRO_HEADER_FULLFactoryComponent } from './commissioncategories/IDD_CTGPRO_HEADER_FULL.component';
import { IDD_COMMISSIONS_STATE_SETTINGComponent, IDD_COMMISSIONS_STATE_SETTINGFactoryComponent } from './commisionstatesetting/IDD_COMMISSIONS_STATE_SETTING.component';
import { IDD_SALES_PEOPLE_BALANCE_REBUILDINGComponent, IDD_SALES_PEOPLE_BALANCE_REBUILDINGFactoryComponent } from './balancerebuilding/IDD_SALES_PEOPLE_BALANCE_REBUILDING.component';
import { IDD_SALESAREA_FULLComponent, IDD_SALESAREA_FULLFactoryComponent } from './areas/IDD_SALESAREA_FULL.component';
import { IDD_ALLOWANCECALCULATIONComponent, IDD_ALLOWANCECALCULATIONFactoryComponent } from './allowancecalculation/IDD_ALLOWANCECALCULATION.component';
import { IDD_ACTUAL_ACCRUAL_DATEComponent, IDD_ACTUAL_ACCRUAL_DATEFactoryComponent } from './actualaccrualdate/IDD_ACTUAL_ACCRUAL_DATE.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_UPDATE_SALESPEOPLE', component: IDD_UPDATE_SALESPEOPLEFactoryComponent },
            { path: 'IDD_PARAMETERS_SALESPEOPLE', component: IDD_PARAMETERS_SALESPEOPLEFactoryComponent },
            { path: 'IDD_SALESPEOPLE_FIRR', component: IDD_SALESPEOPLE_FIRRFactoryComponent },
            { path: 'IDD_SALESPEOPLE_COMPLETE', component: IDD_SALESPEOPLE_COMPLETEFactoryComponent },
            { path: 'IDD_SALESPEOPLE_ALLOWANCE', component: IDD_SALESPEOPLE_ALLOWANCEFactoryComponent },
            { path: 'IDD_SALESPEOPLE_ADD_ON_FLY', component: IDD_SALESPEOPLE_ADD_ON_FLYFactoryComponent },
            { path: 'IDD_FIRRCALCULATION', component: IDD_FIRRCALCULATIONFactoryComponent },
            { path: 'IDD_RECALENASARCO_FEES', component: IDD_RECALENASARCO_FEESFactoryComponent },
            { path: 'IDD_COMMISSIONS_SETTLEMENT', component: IDD_COMMISSIONS_SETTLEMENTFactoryComponent },
            { path: 'IDD_COMMISSIONS_REBUILDING', component: IDD_COMMISSIONS_REBUILDINGFactoryComponent },
            { path: 'IDD_COMMISSIONS_GENERATION', component: IDD_COMMISSIONS_GENERATIONFactoryComponent },
            { path: 'IDD_COMMISSIONS_ENTRIES_DELETING', component: IDD_COMMISSIONS_ENTRIES_DELETINGFactoryComponent },
            { path: 'IDD_ENTRYSALESPERSON_FULL', component: IDD_ENTRYSALESPERSON_FULLFactoryComponent },
            { path: 'IDD_COMM_POLICIES_COPY', component: IDD_COMM_POLICIES_COPYFactoryComponent },
            { path: 'IDD_COMM_POLICIES', component: IDD_COMM_POLICIESFactoryComponent },
            { path: 'IDD_CTGPRO_HEADER_FULL', component: IDD_CTGPRO_HEADER_FULLFactoryComponent },
            { path: 'IDD_COMMISSIONS_STATE_SETTING', component: IDD_COMMISSIONS_STATE_SETTINGFactoryComponent },
            { path: 'IDD_SALES_PEOPLE_BALANCE_REBUILDING', component: IDD_SALES_PEOPLE_BALANCE_REBUILDINGFactoryComponent },
            { path: 'IDD_SALESAREA_FULL', component: IDD_SALESAREA_FULLFactoryComponent },
            { path: 'IDD_ALLOWANCECALCULATION', component: IDD_ALLOWANCECALCULATIONFactoryComponent },
            { path: 'IDD_ACTUAL_ACCRUAL_DATE', component: IDD_ACTUAL_ACCRUAL_DATEFactoryComponent },
        ])],
    declarations: [
            IDD_UPDATE_SALESPEOPLEComponent, IDD_UPDATE_SALESPEOPLEFactoryComponent,
            IDD_PARAMETERS_SALESPEOPLEComponent, IDD_PARAMETERS_SALESPEOPLEFactoryComponent,
            IDD_SALESPEOPLE_FIRRComponent, IDD_SALESPEOPLE_FIRRFactoryComponent,
            IDD_SALESPEOPLE_COMPLETEComponent, IDD_SALESPEOPLE_COMPLETEFactoryComponent,
            IDD_SALESPEOPLE_ALLOWANCEComponent, IDD_SALESPEOPLE_ALLOWANCEFactoryComponent,
            IDD_SALESPEOPLE_ADD_ON_FLYComponent, IDD_SALESPEOPLE_ADD_ON_FLYFactoryComponent,
            IDD_FIRRCALCULATIONComponent, IDD_FIRRCALCULATIONFactoryComponent,
            IDD_RECALENASARCO_FEESComponent, IDD_RECALENASARCO_FEESFactoryComponent,
            IDD_COMMISSIONS_SETTLEMENTComponent, IDD_COMMISSIONS_SETTLEMENTFactoryComponent,
            IDD_COMMISSIONS_REBUILDINGComponent, IDD_COMMISSIONS_REBUILDINGFactoryComponent,
            IDD_COMMISSIONS_GENERATIONComponent, IDD_COMMISSIONS_GENERATIONFactoryComponent,
            IDD_COMMISSIONS_ENTRIES_DELETINGComponent, IDD_COMMISSIONS_ENTRIES_DELETINGFactoryComponent,
            IDD_ENTRYSALESPERSON_FULLComponent, IDD_ENTRYSALESPERSON_FULLFactoryComponent,
            IDD_COMM_POLICIES_COPYComponent, IDD_COMM_POLICIES_COPYFactoryComponent,
            IDD_COMM_POLICIESComponent, IDD_COMM_POLICIESFactoryComponent,
            IDD_CTGPRO_HEADER_FULLComponent, IDD_CTGPRO_HEADER_FULLFactoryComponent,
            IDD_COMMISSIONS_STATE_SETTINGComponent, IDD_COMMISSIONS_STATE_SETTINGFactoryComponent,
            IDD_SALES_PEOPLE_BALANCE_REBUILDINGComponent, IDD_SALES_PEOPLE_BALANCE_REBUILDINGFactoryComponent,
            IDD_SALESAREA_FULLComponent, IDD_SALESAREA_FULLFactoryComponent,
            IDD_ALLOWANCECALCULATIONComponent, IDD_ALLOWANCECALCULATIONFactoryComponent,
            IDD_ACTUAL_ACCRUAL_DATEComponent, IDD_ACTUAL_ACCRUAL_DATEFactoryComponent,
    ],
    exports: [
            IDD_UPDATE_SALESPEOPLEFactoryComponent,
            IDD_PARAMETERS_SALESPEOPLEFactoryComponent,
            IDD_SALESPEOPLE_FIRRFactoryComponent,
            IDD_SALESPEOPLE_COMPLETEFactoryComponent,
            IDD_SALESPEOPLE_ALLOWANCEFactoryComponent,
            IDD_SALESPEOPLE_ADD_ON_FLYFactoryComponent,
            IDD_FIRRCALCULATIONFactoryComponent,
            IDD_RECALENASARCO_FEESFactoryComponent,
            IDD_COMMISSIONS_SETTLEMENTFactoryComponent,
            IDD_COMMISSIONS_REBUILDINGFactoryComponent,
            IDD_COMMISSIONS_GENERATIONFactoryComponent,
            IDD_COMMISSIONS_ENTRIES_DELETINGFactoryComponent,
            IDD_ENTRYSALESPERSON_FULLFactoryComponent,
            IDD_COMM_POLICIES_COPYFactoryComponent,
            IDD_COMM_POLICIESFactoryComponent,
            IDD_CTGPRO_HEADER_FULLFactoryComponent,
            IDD_COMMISSIONS_STATE_SETTINGFactoryComponent,
            IDD_SALES_PEOPLE_BALANCE_REBUILDINGFactoryComponent,
            IDD_SALESAREA_FULLFactoryComponent,
            IDD_ALLOWANCECALCULATIONFactoryComponent,
            IDD_ACTUAL_ACCRUAL_DATEFactoryComponent,
    ],
    entryComponents: [
            IDD_UPDATE_SALESPEOPLEComponent,
            IDD_PARAMETERS_SALESPEOPLEComponent,
            IDD_SALESPEOPLE_FIRRComponent,
            IDD_SALESPEOPLE_COMPLETEComponent,
            IDD_SALESPEOPLE_ALLOWANCEComponent,
            IDD_SALESPEOPLE_ADD_ON_FLYComponent,
            IDD_FIRRCALCULATIONComponent,
            IDD_RECALENASARCO_FEESComponent,
            IDD_COMMISSIONS_SETTLEMENTComponent,
            IDD_COMMISSIONS_REBUILDINGComponent,
            IDD_COMMISSIONS_GENERATIONComponent,
            IDD_COMMISSIONS_ENTRIES_DELETINGComponent,
            IDD_ENTRYSALESPERSON_FULLComponent,
            IDD_COMM_POLICIES_COPYComponent,
            IDD_COMM_POLICIESComponent,
            IDD_CTGPRO_HEADER_FULLComponent,
            IDD_COMMISSIONS_STATE_SETTINGComponent,
            IDD_SALES_PEOPLE_BALANCE_REBUILDINGComponent,
            IDD_SALESAREA_FULLComponent,
            IDD_ALLOWANCECALCULATIONComponent,
            IDD_ACTUAL_ACCRUAL_DATEComponent,
    ]
})


export class SalesPeopleModule { };