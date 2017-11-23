import { IDD_TAXSETTLEMENTUPDATEComponent, IDD_TAXSETTLEMENTUPDATEFactoryComponent } from './taxsettlementupdate/IDD_TAXSETTLEMENTUPDATE.component';
import { IDD_TAXSETTLEMENTSENDINGSComponent, IDD_TAXSETTLEMENTSENDINGSFactoryComponent } from './taxsettlementsendings/IDD_TAXSETTLEMENTSENDINGS.component';
import { IDD_TAXSETTLEMENTCOMMComponent, IDD_TAXSETTLEMENTCOMMFactoryComponent } from './taxsettlement/IDD_TAXSETTLEMENTCOMM.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TAXSETTLEMENTUPDATE', component: IDD_TAXSETTLEMENTUPDATEFactoryComponent },
            { path: 'IDD_TAXSETTLEMENTSENDINGS', component: IDD_TAXSETTLEMENTSENDINGSFactoryComponent },
            { path: 'IDD_TAXSETTLEMENTCOMM', component: IDD_TAXSETTLEMENTCOMMFactoryComponent },
        ])],
    declarations: [
            IDD_TAXSETTLEMENTUPDATEComponent, IDD_TAXSETTLEMENTUPDATEFactoryComponent,
            IDD_TAXSETTLEMENTSENDINGSComponent, IDD_TAXSETTLEMENTSENDINGSFactoryComponent,
            IDD_TAXSETTLEMENTCOMMComponent, IDD_TAXSETTLEMENTCOMMFactoryComponent,
    ],
    exports: [
            IDD_TAXSETTLEMENTUPDATEFactoryComponent,
            IDD_TAXSETTLEMENTSENDINGSFactoryComponent,
            IDD_TAXSETTLEMENTCOMMFactoryComponent,
    ],
    entryComponents: [
            IDD_TAXSETTLEMENTUPDATEComponent,
            IDD_TAXSETTLEMENTSENDINGSComponent,
            IDD_TAXSETTLEMENTCOMMComponent,
    ]
})


export class TaxSettlement_ITModule { };