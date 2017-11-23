import { IDD_OMNIA_PARAMETERSComponent, IDD_OMNIA_PARAMETERSFactoryComponent } from './omniaparameters/IDD_OMNIA_PARAMETERS.component';
import { IDD_OMNIA_IMPORTComponent, IDD_OMNIA_IMPORTFactoryComponent } from './omniaimportmasterdata/IDD_OMNIA_IMPORT.component';
import { IDD_OMNIA_EXPORTComponent, IDD_OMNIA_EXPORTFactoryComponent } from './omniaexport/IDD_OMNIA_EXPORT.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_OMNIA_PARAMETERS', component: IDD_OMNIA_PARAMETERSFactoryComponent },
            { path: 'IDD_OMNIA_IMPORT', component: IDD_OMNIA_IMPORTFactoryComponent },
            { path: 'IDD_OMNIA_EXPORT', component: IDD_OMNIA_EXPORTFactoryComponent },
        ])],
    declarations: [
            IDD_OMNIA_PARAMETERSComponent, IDD_OMNIA_PARAMETERSFactoryComponent,
            IDD_OMNIA_IMPORTComponent, IDD_OMNIA_IMPORTFactoryComponent,
            IDD_OMNIA_EXPORTComponent, IDD_OMNIA_EXPORTFactoryComponent,
    ],
    exports: [
            IDD_OMNIA_PARAMETERSFactoryComponent,
            IDD_OMNIA_IMPORTFactoryComponent,
            IDD_OMNIA_EXPORTFactoryComponent,
    ],
    entryComponents: [
            IDD_OMNIA_PARAMETERSComponent,
            IDD_OMNIA_IMPORTComponent,
            IDD_OMNIA_EXPORTComponent,
    ]
})


export class OMNIAConnectorModule { };