import { IDD_IBC_PARAMETERSComponent, IDD_IBC_PARAMETERSFactoryComponent } from './ibconnectorparameters/IDD_IBC_PARAMETERS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_IBC_PARAMETERS', component: IDD_IBC_PARAMETERSFactoryComponent },
        ])],
    declarations: [
            IDD_IBC_PARAMETERSComponent, IDD_IBC_PARAMETERSFactoryComponent,
    ],
    exports: [
            IDD_IBC_PARAMETERSFactoryComponent,
    ],
    entryComponents: [
            IDD_IBC_PARAMETERSComponent,
    ]
})


export class IBConnectorModule { };