import { IDD_SALEFORECASTSComponent, IDD_SALEFORECASTSFactoryComponent } from './saleforecasts/IDD_SALEFORECASTS.component';
import { IDD_PURCHASEREQComponent, IDD_PURCHASEREQFactoryComponent } from './purchaserequest/IDD_PURCHASEREQ.component';
import { IDD_PURCHREQGENRATIONComponent, IDD_PURCHREQGENRATIONFactoryComponent } from './purchaseordfrompurchasereq/IDD_PURCHREQGENRATION.component';
import { IDD_PURCHREQGENComponent, IDD_PURCHREQGENFactoryComponent } from './purchaseordfrompurchasereq/IDD_PURCHREQGEN.component';
import { IDD_MRPWIZComponent, IDD_MRPWIZFactoryComponent } from './mrpwiz/IDD_MRPWIZ.component';
import { IDD_MRP_SIMULATION_SMALLComponent, IDD_MRP_SIMULATION_SMALLFactoryComponent } from './mrpsimulationsmall/IDD_MRP_SIMULATION_SMALL.component';
import { IDD_MRP_SIMComponent, IDD_MRP_SIMFactoryComponent } from './mrpsimulation/IDD_MRP_SIM.component';
import { IDD_TD_MRP_SCHEDComponent, IDD_TD_MRP_SCHEDFactoryComponent } from './mrpschedulable/IDD_TD_MRP_SCHED.component';
import { IDD_MRPMOCONFIRMATIONComponent, IDD_MRPMOCONFIRMATIONFactoryComponent } from './mrpmoconfirmation/IDD_MRPMOCONFIRMATION.component';
import { IDD_MRP_DEFAULTASSIGNATIONComponent, IDD_MRP_DEFAULTASSIGNATIONFactoryComponent } from './mrpdefaultassignation/IDD_MRP_DEFAULTASSIGNATION.component';
import { IDD_MRPComponent, IDD_MRPFactoryComponent } from './mrp/IDD_MRP.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_SALEFORECASTS', component: IDD_SALEFORECASTSFactoryComponent },
            { path: 'IDD_PURCHASEREQ', component: IDD_PURCHASEREQFactoryComponent },
            { path: 'IDD_PURCHREQGENRATION', component: IDD_PURCHREQGENRATIONFactoryComponent },
            { path: 'IDD_PURCHREQGEN', component: IDD_PURCHREQGENFactoryComponent },
            { path: 'IDD_MRPWIZ', component: IDD_MRPWIZFactoryComponent },
            { path: 'IDD_MRP_SIMULATION_SMALL', component: IDD_MRP_SIMULATION_SMALLFactoryComponent },
            { path: 'IDD_MRP_SIM', component: IDD_MRP_SIMFactoryComponent },
            { path: 'IDD_TD_MRP_SCHED', component: IDD_TD_MRP_SCHEDFactoryComponent },
            { path: 'IDD_MRPMOCONFIRMATION', component: IDD_MRPMOCONFIRMATIONFactoryComponent },
            { path: 'IDD_MRP_DEFAULTASSIGNATION', component: IDD_MRP_DEFAULTASSIGNATIONFactoryComponent },
            { path: 'IDD_MRP', component: IDD_MRPFactoryComponent },
        ])],
    declarations: [
            IDD_SALEFORECASTSComponent, IDD_SALEFORECASTSFactoryComponent,
            IDD_PURCHASEREQComponent, IDD_PURCHASEREQFactoryComponent,
            IDD_PURCHREQGENRATIONComponent, IDD_PURCHREQGENRATIONFactoryComponent,
            IDD_PURCHREQGENComponent, IDD_PURCHREQGENFactoryComponent,
            IDD_MRPWIZComponent, IDD_MRPWIZFactoryComponent,
            IDD_MRP_SIMULATION_SMALLComponent, IDD_MRP_SIMULATION_SMALLFactoryComponent,
            IDD_MRP_SIMComponent, IDD_MRP_SIMFactoryComponent,
            IDD_TD_MRP_SCHEDComponent, IDD_TD_MRP_SCHEDFactoryComponent,
            IDD_MRPMOCONFIRMATIONComponent, IDD_MRPMOCONFIRMATIONFactoryComponent,
            IDD_MRP_DEFAULTASSIGNATIONComponent, IDD_MRP_DEFAULTASSIGNATIONFactoryComponent,
            IDD_MRPComponent, IDD_MRPFactoryComponent,
    ],
    exports: [
            IDD_SALEFORECASTSFactoryComponent,
            IDD_PURCHASEREQFactoryComponent,
            IDD_PURCHREQGENRATIONFactoryComponent,
            IDD_PURCHREQGENFactoryComponent,
            IDD_MRPWIZFactoryComponent,
            IDD_MRP_SIMULATION_SMALLFactoryComponent,
            IDD_MRP_SIMFactoryComponent,
            IDD_TD_MRP_SCHEDFactoryComponent,
            IDD_MRPMOCONFIRMATIONFactoryComponent,
            IDD_MRP_DEFAULTASSIGNATIONFactoryComponent,
            IDD_MRPFactoryComponent,
    ],
    entryComponents: [
            IDD_SALEFORECASTSComponent,
            IDD_PURCHASEREQComponent,
            IDD_PURCHREQGENRATIONComponent,
            IDD_PURCHREQGENComponent,
            IDD_MRPWIZComponent,
            IDD_MRP_SIMULATION_SMALLComponent,
            IDD_MRP_SIMComponent,
            IDD_TD_MRP_SCHEDComponent,
            IDD_MRPMOCONFIRMATIONComponent,
            IDD_MRP_DEFAULTASSIGNATIONComponent,
            IDD_MRPComponent,
    ]
})


export class MRPModule { };