import { IDD_TRANSFERTPLComponent, IDD_TRANSFERTPLFactoryComponent } from './transfertpl/IDD_TRANSFERTPL.component';
import { IDD_COSTTRANSFERComponent, IDD_COSTTRANSFERFactoryComponent } from './transfer/IDD_COSTTRANSFER.component';
import { IDD_COSTACCOUNTINGASSIGNMENTComponent, IDD_COSTACCOUNTINGASSIGNMENTFactoryComponent } from './postableassignment/IDD_COSTACCOUNTINGASSIGNMENT.component';
import { IDD_ENTRIESDELETINGComponent, IDD_ENTRIESDELETINGFactoryComponent } from './entriesdeleting/IDD_ENTRIESDELETING.component';
import { IDD_COSTCENTERSComponent, IDD_COSTCENTERSFactoryComponent } from './costcenters/IDD_COSTCENTERS.component';
import { IDD_COSTCENTERGROUPSComponent, IDD_COSTCENTERGROUPSFactoryComponent } from './costcentergroups/IDD_COSTCENTERGROUPS.component';
import { IDD_PARAMCOSTACCComponent, IDD_PARAMCOSTACCFactoryComponent } from './costaccountingparameters/IDD_PARAMCOSTACC.component';
import { IDD_GENERATEDETAILComponent, IDD_GENERATEDETAILFactoryComponent } from './costaccountingentries/IDD_GENERATEDETAIL.component';
import { IDD_COSTACCTEMPLETESAVEComponent, IDD_COSTACCTEMPLETESAVEFactoryComponent } from './costaccountingentries/IDD_COSTACCTEMPLETESAVE.component';
import { IDD_COSTACCComponent, IDD_COSTACCFactoryComponent } from './costaccountingentries/IDD_COSTACC.component';
import { IDD_COSTACCFROMSALESComponent, IDD_COSTACCFROMSALESFactoryComponent } from './costaccentriesfromsales/IDD_COSTACCFROMSALES.component';
import { IDD_COSTACCENTRIESFROMPURCHASEComponent, IDD_COSTACCENTRIESFROMPURCHASEFactoryComponent } from './costaccentriesfrompurchases/IDD_COSTACCENTRIESFROMPURCHASE.component';
import { IDD_COSTACCENTRIESFROMINVENTORYComponent, IDD_COSTACCENTRIESFROMINVENTORYFactoryComponent } from './costaccentriesfrominventory/IDD_COSTACCENTRIESFROMINVENTORY.component';
import { IDD_COSTACCENTRIESFROMDEPRECIATIONComponent, IDD_COSTACCENTRIESFROMDEPRECIATIONFactoryComponent } from './costaccentriesfromdepreciation/IDD_COSTACCENTRIESFROMDEPRECIATION.component';
import { IDD_COSTACCENTRIESFROMACCOUNTINGComponent, IDD_COSTACCENTRIESFROMACCOUNTINGFactoryComponent } from './costaccentriesfromaccounting/IDD_COSTACCENTRIESFROMACCOUNTING.component';
import { IDD_BUDGETENTRIESGENERATIONComponent, IDD_BUDGETENTRIESGENERATIONFactoryComponent } from './budgetentriesgeneration/IDD_BUDGETENTRIESGENERATION.component';
import { IDD_COSTACCOUNTINGREBUILDINGComponent, IDD_COSTACCOUNTINGREBUILDINGFactoryComponent } from './balancerebuilding/IDD_COSTACCOUNTINGREBUILDING.component';
import { IDD_COSTACCGRAPHComponent, IDD_COSTACCGRAPHFactoryComponent } from './uicostcentersgraphicnavigation/IDD_COSTACCGRAPH.component';
import { IDD_BUDGETANALYSISComponent, IDD_BUDGETANALYSISFactoryComponent } from './uicostaccountingbudgetanalysis/IDD_BUDGETANALYSIS.component';
import { IDD_COSTACCOUNTINGBALANCESHEETComponent, IDD_COSTACCOUNTINGBALANCESHEETFactoryComponent } from './uicostaccountingbalancesheet/IDD_COSTACCOUNTINGBALANCESHEET.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TRANSFERTPL', component: IDD_TRANSFERTPLFactoryComponent },
            { path: 'IDD_COSTTRANSFER', component: IDD_COSTTRANSFERFactoryComponent },
            { path: 'IDD_COSTACCOUNTINGASSIGNMENT', component: IDD_COSTACCOUNTINGASSIGNMENTFactoryComponent },
            { path: 'IDD_ENTRIESDELETING', component: IDD_ENTRIESDELETINGFactoryComponent },
            { path: 'IDD_COSTCENTERS', component: IDD_COSTCENTERSFactoryComponent },
            { path: 'IDD_COSTCENTERGROUPS', component: IDD_COSTCENTERGROUPSFactoryComponent },
            { path: 'IDD_PARAMCOSTACC', component: IDD_PARAMCOSTACCFactoryComponent },
            { path: 'IDD_GENERATEDETAIL', component: IDD_GENERATEDETAILFactoryComponent },
            { path: 'IDD_COSTACCTEMPLETESAVE', component: IDD_COSTACCTEMPLETESAVEFactoryComponent },
            { path: 'IDD_COSTACC', component: IDD_COSTACCFactoryComponent },
            { path: 'IDD_COSTACCFROMSALES', component: IDD_COSTACCFROMSALESFactoryComponent },
            { path: 'IDD_COSTACCENTRIESFROMPURCHASE', component: IDD_COSTACCENTRIESFROMPURCHASEFactoryComponent },
            { path: 'IDD_COSTACCENTRIESFROMINVENTORY', component: IDD_COSTACCENTRIESFROMINVENTORYFactoryComponent },
            { path: 'IDD_COSTACCENTRIESFROMDEPRECIATION', component: IDD_COSTACCENTRIESFROMDEPRECIATIONFactoryComponent },
            { path: 'IDD_COSTACCENTRIESFROMACCOUNTING', component: IDD_COSTACCENTRIESFROMACCOUNTINGFactoryComponent },
            { path: 'IDD_BUDGETENTRIESGENERATION', component: IDD_BUDGETENTRIESGENERATIONFactoryComponent },
            { path: 'IDD_COSTACCOUNTINGREBUILDING', component: IDD_COSTACCOUNTINGREBUILDINGFactoryComponent },
            { path: 'IDD_COSTACCGRAPH', component: IDD_COSTACCGRAPHFactoryComponent },
            { path: 'IDD_BUDGETANALYSIS', component: IDD_BUDGETANALYSISFactoryComponent },
            { path: 'IDD_COSTACCOUNTINGBALANCESHEET', component: IDD_COSTACCOUNTINGBALANCESHEETFactoryComponent },
        ])],
    declarations: [
            IDD_TRANSFERTPLComponent, IDD_TRANSFERTPLFactoryComponent,
            IDD_COSTTRANSFERComponent, IDD_COSTTRANSFERFactoryComponent,
            IDD_COSTACCOUNTINGASSIGNMENTComponent, IDD_COSTACCOUNTINGASSIGNMENTFactoryComponent,
            IDD_ENTRIESDELETINGComponent, IDD_ENTRIESDELETINGFactoryComponent,
            IDD_COSTCENTERSComponent, IDD_COSTCENTERSFactoryComponent,
            IDD_COSTCENTERGROUPSComponent, IDD_COSTCENTERGROUPSFactoryComponent,
            IDD_PARAMCOSTACCComponent, IDD_PARAMCOSTACCFactoryComponent,
            IDD_GENERATEDETAILComponent, IDD_GENERATEDETAILFactoryComponent,
            IDD_COSTACCTEMPLETESAVEComponent, IDD_COSTACCTEMPLETESAVEFactoryComponent,
            IDD_COSTACCComponent, IDD_COSTACCFactoryComponent,
            IDD_COSTACCFROMSALESComponent, IDD_COSTACCFROMSALESFactoryComponent,
            IDD_COSTACCENTRIESFROMPURCHASEComponent, IDD_COSTACCENTRIESFROMPURCHASEFactoryComponent,
            IDD_COSTACCENTRIESFROMINVENTORYComponent, IDD_COSTACCENTRIESFROMINVENTORYFactoryComponent,
            IDD_COSTACCENTRIESFROMDEPRECIATIONComponent, IDD_COSTACCENTRIESFROMDEPRECIATIONFactoryComponent,
            IDD_COSTACCENTRIESFROMACCOUNTINGComponent, IDD_COSTACCENTRIESFROMACCOUNTINGFactoryComponent,
            IDD_BUDGETENTRIESGENERATIONComponent, IDD_BUDGETENTRIESGENERATIONFactoryComponent,
            IDD_COSTACCOUNTINGREBUILDINGComponent, IDD_COSTACCOUNTINGREBUILDINGFactoryComponent,
            IDD_COSTACCGRAPHComponent, IDD_COSTACCGRAPHFactoryComponent,
            IDD_BUDGETANALYSISComponent, IDD_BUDGETANALYSISFactoryComponent,
            IDD_COSTACCOUNTINGBALANCESHEETComponent, IDD_COSTACCOUNTINGBALANCESHEETFactoryComponent,
    ],
    exports: [
            IDD_TRANSFERTPLFactoryComponent,
            IDD_COSTTRANSFERFactoryComponent,
            IDD_COSTACCOUNTINGASSIGNMENTFactoryComponent,
            IDD_ENTRIESDELETINGFactoryComponent,
            IDD_COSTCENTERSFactoryComponent,
            IDD_COSTCENTERGROUPSFactoryComponent,
            IDD_PARAMCOSTACCFactoryComponent,
            IDD_GENERATEDETAILFactoryComponent,
            IDD_COSTACCTEMPLETESAVEFactoryComponent,
            IDD_COSTACCFactoryComponent,
            IDD_COSTACCFROMSALESFactoryComponent,
            IDD_COSTACCENTRIESFROMPURCHASEFactoryComponent,
            IDD_COSTACCENTRIESFROMINVENTORYFactoryComponent,
            IDD_COSTACCENTRIESFROMDEPRECIATIONFactoryComponent,
            IDD_COSTACCENTRIESFROMACCOUNTINGFactoryComponent,
            IDD_BUDGETENTRIESGENERATIONFactoryComponent,
            IDD_COSTACCOUNTINGREBUILDINGFactoryComponent,
            IDD_COSTACCGRAPHFactoryComponent,
            IDD_BUDGETANALYSISFactoryComponent,
            IDD_COSTACCOUNTINGBALANCESHEETFactoryComponent,
    ],
    entryComponents: [
            IDD_TRANSFERTPLComponent,
            IDD_COSTTRANSFERComponent,
            IDD_COSTACCOUNTINGASSIGNMENTComponent,
            IDD_ENTRIESDELETINGComponent,
            IDD_COSTCENTERSComponent,
            IDD_COSTCENTERGROUPSComponent,
            IDD_PARAMCOSTACCComponent,
            IDD_GENERATEDETAILComponent,
            IDD_COSTACCTEMPLETESAVEComponent,
            IDD_COSTACCComponent,
            IDD_COSTACCFROMSALESComponent,
            IDD_COSTACCENTRIESFROMPURCHASEComponent,
            IDD_COSTACCENTRIESFROMINVENTORYComponent,
            IDD_COSTACCENTRIESFROMDEPRECIATIONComponent,
            IDD_COSTACCENTRIESFROMACCOUNTINGComponent,
            IDD_BUDGETENTRIESGENERATIONComponent,
            IDD_COSTACCOUNTINGREBUILDINGComponent,
            IDD_COSTACCGRAPHComponent,
            IDD_BUDGETANALYSISComponent,
            IDD_COSTACCOUNTINGBALANCESHEETComponent,
    ]
})


export class CostAccountingModule { };