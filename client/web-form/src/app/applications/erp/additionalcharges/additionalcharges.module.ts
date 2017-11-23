import { IDD_DEASSOCIATIONSComponent, IDD_DEASSOCIATIONSFactoryComponent } from './itemtoctgassociations/IDD_DEASSOCIATIONS.component';
import { IDD_DETEMPLATESComponent, IDD_DETEMPLATESFactoryComponent } from './distributiontemplates/IDD_DETEMPLATES.component';
import { IDD_ADDITIONAL_CHARGES_UPDATEBOXComponent, IDD_ADDITIONAL_CHARGES_UPDATEBOXFactoryComponent } from './cdadditionalchargespurchasedoc/IDD_ADDITIONAL_CHARGES_UPDATEBOX.component';
import { IDD_ADDITIONAL_CHARGES_PARAMETERSComponent, IDD_ADDITIONAL_CHARGES_PARAMETERSFactoryComponent } from './cdadditionalchargespurchasedoc/IDD_ADDITIONAL_CHARGES_PARAMETERS.component';
import { IDD_ASSAUTComponent, IDD_ASSAUTFactoryComponent } from './automaticassociation/IDD_ASSAUT.component';
import { IDD_ADD_CH_REBUILDComponent, IDD_ADD_CH_REBUILDFactoryComponent } from './additionalchargesrebuilding/IDD_ADD_CH_REBUILD.component';
import { IDD_ADDITIONAL_CHARGES_PARAMSComponent, IDD_ADDITIONAL_CHARGES_PARAMSFactoryComponent } from './additionalchargesparams/IDD_ADDITIONAL_CHARGES_PARAMS.component';
import { IDD_ADDITIONAL_CHARGES_LOADComponent, IDD_ADDITIONAL_CHARGES_LOADFactoryComponent } from './additionalcharges/IDD_ADDITIONAL_CHARGES_LOAD.component';
import { IDD_ADDITIONAL_CHARGESComponent, IDD_ADDITIONAL_CHARGESFactoryComponent } from './additionalcharges/IDD_ADDITIONAL_CHARGES.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_DEASSOCIATIONS', component: IDD_DEASSOCIATIONSFactoryComponent },
            { path: 'IDD_DETEMPLATES', component: IDD_DETEMPLATESFactoryComponent },
            { path: 'IDD_ADDITIONAL_CHARGES_UPDATEBOX', component: IDD_ADDITIONAL_CHARGES_UPDATEBOXFactoryComponent },
            { path: 'IDD_ADDITIONAL_CHARGES_PARAMETERS', component: IDD_ADDITIONAL_CHARGES_PARAMETERSFactoryComponent },
            { path: 'IDD_ASSAUT', component: IDD_ASSAUTFactoryComponent },
            { path: 'IDD_ADD_CH_REBUILD', component: IDD_ADD_CH_REBUILDFactoryComponent },
            { path: 'IDD_ADDITIONAL_CHARGES_PARAMS', component: IDD_ADDITIONAL_CHARGES_PARAMSFactoryComponent },
            { path: 'IDD_ADDITIONAL_CHARGES_LOAD', component: IDD_ADDITIONAL_CHARGES_LOADFactoryComponent },
            { path: 'IDD_ADDITIONAL_CHARGES', component: IDD_ADDITIONAL_CHARGESFactoryComponent },
        ])],
    declarations: [
            IDD_DEASSOCIATIONSComponent, IDD_DEASSOCIATIONSFactoryComponent,
            IDD_DETEMPLATESComponent, IDD_DETEMPLATESFactoryComponent,
            IDD_ADDITIONAL_CHARGES_UPDATEBOXComponent, IDD_ADDITIONAL_CHARGES_UPDATEBOXFactoryComponent,
            IDD_ADDITIONAL_CHARGES_PARAMETERSComponent, IDD_ADDITIONAL_CHARGES_PARAMETERSFactoryComponent,
            IDD_ASSAUTComponent, IDD_ASSAUTFactoryComponent,
            IDD_ADD_CH_REBUILDComponent, IDD_ADD_CH_REBUILDFactoryComponent,
            IDD_ADDITIONAL_CHARGES_PARAMSComponent, IDD_ADDITIONAL_CHARGES_PARAMSFactoryComponent,
            IDD_ADDITIONAL_CHARGES_LOADComponent, IDD_ADDITIONAL_CHARGES_LOADFactoryComponent,
            IDD_ADDITIONAL_CHARGESComponent, IDD_ADDITIONAL_CHARGESFactoryComponent,
    ],
    exports: [
            IDD_DEASSOCIATIONSFactoryComponent,
            IDD_DETEMPLATESFactoryComponent,
            IDD_ADDITIONAL_CHARGES_UPDATEBOXFactoryComponent,
            IDD_ADDITIONAL_CHARGES_PARAMETERSFactoryComponent,
            IDD_ASSAUTFactoryComponent,
            IDD_ADD_CH_REBUILDFactoryComponent,
            IDD_ADDITIONAL_CHARGES_PARAMSFactoryComponent,
            IDD_ADDITIONAL_CHARGES_LOADFactoryComponent,
            IDD_ADDITIONAL_CHARGESFactoryComponent,
    ],
    entryComponents: [
            IDD_DEASSOCIATIONSComponent,
            IDD_DETEMPLATESComponent,
            IDD_ADDITIONAL_CHARGES_UPDATEBOXComponent,
            IDD_ADDITIONAL_CHARGES_PARAMETERSComponent,
            IDD_ASSAUTComponent,
            IDD_ADD_CH_REBUILDComponent,
            IDD_ADDITIONAL_CHARGES_PARAMSComponent,
            IDD_ADDITIONAL_CHARGES_LOADComponent,
            IDD_ADDITIONAL_CHARGESComponent,
    ]
})


export class AdditionalChargesModule { };