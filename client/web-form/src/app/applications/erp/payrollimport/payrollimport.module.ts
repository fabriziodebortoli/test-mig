import { IDD_PAYROLLTEMPLATESComponent, IDD_PAYROLLTEMPLATESFactoryComponent } from './payrolltemplates/IDD_PAYROLLTEMPLATES.component';
import { IDD_PAYROLLREASONSComponent, IDD_PAYROLLREASONSFactoryComponent } from './payrollreasons/IDD_PAYROLLREASONS.component';
import { IDD_PAYROLLPARAMETERSComponent, IDD_PAYROLLPARAMETERSFactoryComponent } from './payrollparameters/IDD_PAYROLLPARAMETERS.component';
import { IDD_PAYROLLIMPORTComponent, IDD_PAYROLLIMPORTFactoryComponent } from './payrollimport/IDD_PAYROLLIMPORT.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_PAYROLLTEMPLATES', component: IDD_PAYROLLTEMPLATESFactoryComponent },
            { path: 'IDD_PAYROLLREASONS', component: IDD_PAYROLLREASONSFactoryComponent },
            { path: 'IDD_PAYROLLPARAMETERS', component: IDD_PAYROLLPARAMETERSFactoryComponent },
            { path: 'IDD_PAYROLLIMPORT', component: IDD_PAYROLLIMPORTFactoryComponent },
        ])],
    declarations: [
            IDD_PAYROLLTEMPLATESComponent, IDD_PAYROLLTEMPLATESFactoryComponent,
            IDD_PAYROLLREASONSComponent, IDD_PAYROLLREASONSFactoryComponent,
            IDD_PAYROLLPARAMETERSComponent, IDD_PAYROLLPARAMETERSFactoryComponent,
            IDD_PAYROLLIMPORTComponent, IDD_PAYROLLIMPORTFactoryComponent,
    ],
    exports: [
            IDD_PAYROLLTEMPLATESFactoryComponent,
            IDD_PAYROLLREASONSFactoryComponent,
            IDD_PAYROLLPARAMETERSFactoryComponent,
            IDD_PAYROLLIMPORTFactoryComponent,
    ],
    entryComponents: [
            IDD_PAYROLLTEMPLATESComponent,
            IDD_PAYROLLREASONSComponent,
            IDD_PAYROLLPARAMETERSComponent,
            IDD_PAYROLLIMPORTComponent,
    ]
})


export class PayrollImportModule { };