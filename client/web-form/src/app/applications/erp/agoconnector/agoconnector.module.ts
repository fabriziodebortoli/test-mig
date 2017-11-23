import { IDD_AGO_PARAMETERSComponent, IDD_AGO_PARAMETERSFactoryComponent } from './agoparameters/IDD_AGO_PARAMETERS.component';
import { IDD_AGO_IMPORTComponent, IDD_AGO_IMPORTFactoryComponent } from './agoimport/IDD_AGO_IMPORT.component';
import { IDD_AGO_EXPORTComponent, IDD_AGO_EXPORTFactoryComponent } from './agoexport/IDD_AGO_EXPORT.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_AGO_PARAMETERS', component: IDD_AGO_PARAMETERSFactoryComponent },
            { path: 'IDD_AGO_IMPORT', component: IDD_AGO_IMPORTFactoryComponent },
            { path: 'IDD_AGO_EXPORT', component: IDD_AGO_EXPORTFactoryComponent },
        ])],
    declarations: [
            IDD_AGO_PARAMETERSComponent, IDD_AGO_PARAMETERSFactoryComponent,
            IDD_AGO_IMPORTComponent, IDD_AGO_IMPORTFactoryComponent,
            IDD_AGO_EXPORTComponent, IDD_AGO_EXPORTFactoryComponent,
    ],
    exports: [
            IDD_AGO_PARAMETERSFactoryComponent,
            IDD_AGO_IMPORTFactoryComponent,
            IDD_AGO_EXPORTFactoryComponent,
    ],
    entryComponents: [
            IDD_AGO_PARAMETERSComponent,
            IDD_AGO_IMPORTComponent,
            IDD_AGO_EXPORTComponent,
    ]
})


export class AGOConnectorModule { };