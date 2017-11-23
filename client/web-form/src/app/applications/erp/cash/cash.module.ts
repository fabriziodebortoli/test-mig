import { IDD_CASH_STUBBOOKComponent, IDD_CASH_STUBBOOKFactoryComponent } from './cashstubbooks/IDD_CASH_STUBBOOK.component';
import { IDD_CASH_STUBBOOK_NUMERATORComponent, IDD_CASH_STUBBOOK_NUMERATORFactoryComponent } from './cashstubbooknumbers/IDD_CASH_STUBBOOK_NUMERATOR.component';
import { IDD_CASHREASONSComponent, IDD_CASHREASONSFactoryComponent } from './cashreasons/IDD_CASHREASONS.component';
import { IDD_PARAMETERS_CASHComponent, IDD_PARAMETERS_CASHFactoryComponent } from './cashparameters/IDD_PARAMETERS_CASH.component';
import { IDD_CASHENTRIES_WORKINGSESSIONComponent, IDD_CASHENTRIES_WORKINGSESSIONFactoryComponent } from './cashmanagement/IDD_CASHENTRIES_WORKINGSESSION.component';
import { IDD_CASHENTRIESComponent, IDD_CASHENTRIESFactoryComponent } from './cashmanagement/IDD_CASHENTRIES.component';
import { IDD_CASHCLEARINGComponent, IDD_CASHCLEARINGFactoryComponent } from './cashclearing/IDD_CASHCLEARING.component';
import { IDD_ACCOUNTINGPOSTING_WIZARDComponent, IDD_ACCOUNTINGPOSTING_WIZARDFactoryComponent } from './cashaccountingposting/IDD_ACCOUNTINGPOSTING_WIZARD.component';
import { IDD_CASHComponent, IDD_CASHFactoryComponent } from './cash/IDD_CASH.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_CASH_STUBBOOK', component: IDD_CASH_STUBBOOKFactoryComponent },
            { path: 'IDD_CASH_STUBBOOK_NUMERATOR', component: IDD_CASH_STUBBOOK_NUMERATORFactoryComponent },
            { path: 'IDD_CASHREASONS', component: IDD_CASHREASONSFactoryComponent },
            { path: 'IDD_PARAMETERS_CASH', component: IDD_PARAMETERS_CASHFactoryComponent },
            { path: 'IDD_CASHENTRIES_WORKINGSESSION', component: IDD_CASHENTRIES_WORKINGSESSIONFactoryComponent },
            { path: 'IDD_CASHENTRIES', component: IDD_CASHENTRIESFactoryComponent },
            { path: 'IDD_CASHCLEARING', component: IDD_CASHCLEARINGFactoryComponent },
            { path: 'IDD_ACCOUNTINGPOSTING_WIZARD', component: IDD_ACCOUNTINGPOSTING_WIZARDFactoryComponent },
            { path: 'IDD_CASH', component: IDD_CASHFactoryComponent },
        ])],
    declarations: [
            IDD_CASH_STUBBOOKComponent, IDD_CASH_STUBBOOKFactoryComponent,
            IDD_CASH_STUBBOOK_NUMERATORComponent, IDD_CASH_STUBBOOK_NUMERATORFactoryComponent,
            IDD_CASHREASONSComponent, IDD_CASHREASONSFactoryComponent,
            IDD_PARAMETERS_CASHComponent, IDD_PARAMETERS_CASHFactoryComponent,
            IDD_CASHENTRIES_WORKINGSESSIONComponent, IDD_CASHENTRIES_WORKINGSESSIONFactoryComponent,
            IDD_CASHENTRIESComponent, IDD_CASHENTRIESFactoryComponent,
            IDD_CASHCLEARINGComponent, IDD_CASHCLEARINGFactoryComponent,
            IDD_ACCOUNTINGPOSTING_WIZARDComponent, IDD_ACCOUNTINGPOSTING_WIZARDFactoryComponent,
            IDD_CASHComponent, IDD_CASHFactoryComponent,
    ],
    exports: [
            IDD_CASH_STUBBOOKFactoryComponent,
            IDD_CASH_STUBBOOK_NUMERATORFactoryComponent,
            IDD_CASHREASONSFactoryComponent,
            IDD_PARAMETERS_CASHFactoryComponent,
            IDD_CASHENTRIES_WORKINGSESSIONFactoryComponent,
            IDD_CASHENTRIESFactoryComponent,
            IDD_CASHCLEARINGFactoryComponent,
            IDD_ACCOUNTINGPOSTING_WIZARDFactoryComponent,
            IDD_CASHFactoryComponent,
    ],
    entryComponents: [
            IDD_CASH_STUBBOOKComponent,
            IDD_CASH_STUBBOOK_NUMERATORComponent,
            IDD_CASHREASONSComponent,
            IDD_PARAMETERS_CASHComponent,
            IDD_CASHENTRIES_WORKINGSESSIONComponent,
            IDD_CASHENTRIESComponent,
            IDD_CASHCLEARINGComponent,
            IDD_ACCOUNTINGPOSTING_WIZARDComponent,
            IDD_CASHComponent,
    ]
})


export class CashModule { };