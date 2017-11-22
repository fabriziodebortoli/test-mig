import { IDD_TS_EXPORTCSVFILEComponent, IDD_TS_EXPORTCSVFILEFactoryComponent } from './tsexportcsvfile/IDD_TS_EXPORTCSVFILE.component';
import { IDD_TS_CONNECTOR_PARAMETERSComponent, IDD_TS_CONNECTOR_PARAMETERSFactoryComponent } from './tsconnectorparameters/IDD_TS_CONNECTOR_PARAMETERS.component';
import { IDD_TS_DELETINGComponent, IDD_TS_DELETINGFactoryComponent } from './tscleartransmission/IDD_TS_DELETING.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TS_EXPORTCSVFILE', component: IDD_TS_EXPORTCSVFILEFactoryComponent },
            { path: 'IDD_TS_CONNECTOR_PARAMETERS', component: IDD_TS_CONNECTOR_PARAMETERSFactoryComponent },
            { path: 'IDD_TS_DELETING', component: IDD_TS_DELETINGFactoryComponent },
        ])],
    declarations: [
            IDD_TS_EXPORTCSVFILEComponent, IDD_TS_EXPORTCSVFILEFactoryComponent,
            IDD_TS_CONNECTOR_PARAMETERSComponent, IDD_TS_CONNECTOR_PARAMETERSFactoryComponent,
            IDD_TS_DELETINGComponent, IDD_TS_DELETINGFactoryComponent,
    ],
    exports: [
            IDD_TS_EXPORTCSVFILEFactoryComponent,
            IDD_TS_CONNECTOR_PARAMETERSFactoryComponent,
            IDD_TS_DELETINGFactoryComponent,
    ],
    entryComponents: [
            IDD_TS_EXPORTCSVFILEComponent,
            IDD_TS_CONNECTOR_PARAMETERSComponent,
            IDD_TS_DELETINGComponent,
    ]
})


export class TESANConnectorModule { };