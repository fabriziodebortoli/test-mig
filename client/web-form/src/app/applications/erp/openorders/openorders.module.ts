import { IDD_RECALCULATE_UNITComponent, IDD_RECALCULATE_UNITFactoryComponent } from './recalculateunitvalue/IDD_RECALCULATE_UNIT.component';
import { IDD_OPENORDERS_DELIVERYSCHEDULES_DAILYComponent, IDD_OPENORDERS_DELIVERYSCHEDULES_DAILYFactoryComponent } from './deliveryschedules/IDD_OPENORDERS_DELIVERYSCHEDULES_DAILY.component';
import { IDD_OPENORDERS_DELIVERYSCHEDULES_BATCHComponent, IDD_OPENORDERS_DELIVERYSCHEDULES_BATCHFactoryComponent } from './deliveryschedules/IDD_OPENORDERS_DELIVERYSCHEDULES_BATCH.component';
import { IDD_OPENORDERS_CUSTOMERCONTRACTSComponent, IDD_OPENORDERS_CUSTOMERCONTRACTSFactoryComponent } from './customercontracts/IDD_OPENORDERS_CUSTOMERCONTRACTS.component';
import { IDD_LOAD_CUSTITEMSComponent, IDD_LOAD_CUSTITEMSFactoryComponent } from './custitemsloading/IDD_LOAD_CUSTITEMS.component';
import { IDD_CUSTCONTRSALEComponent, IDD_CUSTCONTRSALEFactoryComponent } from './custcontractssale/IDD_CUSTCONTRSALE.component';
import { IDD_LOAD_CUSTCONTRComponent, IDD_LOAD_CUSTCONTRFactoryComponent } from './custcontractsloading/IDD_LOAD_CUSTCONTR.component';
import { IDD_OPENORDERS_CONF_LEVELSComponent, IDD_OPENORDERS_CONF_LEVELSFactoryComponent } from './confirmationlevels/IDD_OPENORDERS_CONF_LEVELS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_RECALCULATE_UNIT', component: IDD_RECALCULATE_UNITFactoryComponent },
            { path: 'IDD_OPENORDERS_DELIVERYSCHEDULES_DAILY', component: IDD_OPENORDERS_DELIVERYSCHEDULES_DAILYFactoryComponent },
            { path: 'IDD_OPENORDERS_DELIVERYSCHEDULES_BATCH', component: IDD_OPENORDERS_DELIVERYSCHEDULES_BATCHFactoryComponent },
            { path: 'IDD_OPENORDERS_CUSTOMERCONTRACTS', component: IDD_OPENORDERS_CUSTOMERCONTRACTSFactoryComponent },
            { path: 'IDD_LOAD_CUSTITEMS', component: IDD_LOAD_CUSTITEMSFactoryComponent },
            { path: 'IDD_CUSTCONTRSALE', component: IDD_CUSTCONTRSALEFactoryComponent },
            { path: 'IDD_LOAD_CUSTCONTR', component: IDD_LOAD_CUSTCONTRFactoryComponent },
            { path: 'IDD_OPENORDERS_CONF_LEVELS', component: IDD_OPENORDERS_CONF_LEVELSFactoryComponent },
        ])],
    declarations: [
            IDD_RECALCULATE_UNITComponent, IDD_RECALCULATE_UNITFactoryComponent,
            IDD_OPENORDERS_DELIVERYSCHEDULES_DAILYComponent, IDD_OPENORDERS_DELIVERYSCHEDULES_DAILYFactoryComponent,
            IDD_OPENORDERS_DELIVERYSCHEDULES_BATCHComponent, IDD_OPENORDERS_DELIVERYSCHEDULES_BATCHFactoryComponent,
            IDD_OPENORDERS_CUSTOMERCONTRACTSComponent, IDD_OPENORDERS_CUSTOMERCONTRACTSFactoryComponent,
            IDD_LOAD_CUSTITEMSComponent, IDD_LOAD_CUSTITEMSFactoryComponent,
            IDD_CUSTCONTRSALEComponent, IDD_CUSTCONTRSALEFactoryComponent,
            IDD_LOAD_CUSTCONTRComponent, IDD_LOAD_CUSTCONTRFactoryComponent,
            IDD_OPENORDERS_CONF_LEVELSComponent, IDD_OPENORDERS_CONF_LEVELSFactoryComponent,
    ],
    exports: [
            IDD_RECALCULATE_UNITFactoryComponent,
            IDD_OPENORDERS_DELIVERYSCHEDULES_DAILYFactoryComponent,
            IDD_OPENORDERS_DELIVERYSCHEDULES_BATCHFactoryComponent,
            IDD_OPENORDERS_CUSTOMERCONTRACTSFactoryComponent,
            IDD_LOAD_CUSTITEMSFactoryComponent,
            IDD_CUSTCONTRSALEFactoryComponent,
            IDD_LOAD_CUSTCONTRFactoryComponent,
            IDD_OPENORDERS_CONF_LEVELSFactoryComponent,
    ],
    entryComponents: [
            IDD_RECALCULATE_UNITComponent,
            IDD_OPENORDERS_DELIVERYSCHEDULES_DAILYComponent,
            IDD_OPENORDERS_DELIVERYSCHEDULES_BATCHComponent,
            IDD_OPENORDERS_CUSTOMERCONTRACTSComponent,
            IDD_LOAD_CUSTITEMSComponent,
            IDD_CUSTCONTRSALEComponent,
            IDD_LOAD_CUSTCONTRComponent,
            IDD_OPENORDERS_CONF_LEVELSComponent,
    ]
})


export class OpenOrdersModule { };