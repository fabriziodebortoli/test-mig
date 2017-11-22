import { IDD_EIPAYMENTTYPEComponent, IDD_EIPAYMENTTYPEFactoryComponent } from './eipaymenttype/IDD_EIPAYMENTTYPE.component';
import { IDD_EIINVOICE_PARAMETERSComponent, IDD_EIINVOICE_PARAMETERSFactoryComponent } from './eiparameters/IDD_EIINVOICE_PARAMETERS.component';
import { IDD_EICODINGComponent, IDD_EICODINGFactoryComponent } from './eicoding/IDD_EICODING.component';
import { IDD_EVENT_VIEWERComponent, IDD_EVENT_VIEWERFactoryComponent } from './eventviewer/IDD_EVENT_VIEWER.component';
import { IDD_EI_DOC_ELABORATION_STATUSComponent, IDD_EI_DOC_ELABORATION_STATUSFactoryComponent } from './docelabotrationmanager/IDD_EI_DOC_ELABORATION_STATUS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_EIPAYMENTTYPE', component: IDD_EIPAYMENTTYPEFactoryComponent },
            { path: 'IDD_EIINVOICE_PARAMETERS', component: IDD_EIINVOICE_PARAMETERSFactoryComponent },
            { path: 'IDD_EICODING', component: IDD_EICODINGFactoryComponent },
            { path: 'IDD_EVENT_VIEWER', component: IDD_EVENT_VIEWERFactoryComponent },
            { path: 'IDD_EI_DOC_ELABORATION_STATUS', component: IDD_EI_DOC_ELABORATION_STATUSFactoryComponent },
        ])],
    declarations: [
            IDD_EIPAYMENTTYPEComponent, IDD_EIPAYMENTTYPEFactoryComponent,
            IDD_EIINVOICE_PARAMETERSComponent, IDD_EIINVOICE_PARAMETERSFactoryComponent,
            IDD_EICODINGComponent, IDD_EICODINGFactoryComponent,
            IDD_EVENT_VIEWERComponent, IDD_EVENT_VIEWERFactoryComponent,
            IDD_EI_DOC_ELABORATION_STATUSComponent, IDD_EI_DOC_ELABORATION_STATUSFactoryComponent,
    ],
    exports: [
            IDD_EIPAYMENTTYPEFactoryComponent,
            IDD_EIINVOICE_PARAMETERSFactoryComponent,
            IDD_EICODINGFactoryComponent,
            IDD_EVENT_VIEWERFactoryComponent,
            IDD_EI_DOC_ELABORATION_STATUSFactoryComponent,
    ],
    entryComponents: [
            IDD_EIPAYMENTTYPEComponent,
            IDD_EIINVOICE_PARAMETERSComponent,
            IDD_EICODINGComponent,
            IDD_EVENT_VIEWERComponent,
            IDD_EI_DOC_ELABORATION_STATUSComponent,
    ]
})


export class CoreModule { };