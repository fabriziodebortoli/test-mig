import { IDD_TAX_STATEMENT_PARAMETERSComponent, IDD_TAX_STATEMENT_PARAMETERSFactoryComponent } from './taxstatement/IDD_TAX_STATEMENT_PARAMETERS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TAX_STATEMENT_PARAMETERS', component: IDD_TAX_STATEMENT_PARAMETERSFactoryComponent },
        ])],
    declarations: [
            IDD_TAX_STATEMENT_PARAMETERSComponent, IDD_TAX_STATEMENT_PARAMETERSFactoryComponent,
    ],
    exports: [
            IDD_TAX_STATEMENT_PARAMETERSFactoryComponent,
    ],
    entryComponents: [
            IDD_TAX_STATEMENT_PARAMETERSComponent,
    ]
})


export class Accounting_CHModule { };