import { IDD_PARAMETERS_CHARTOFACCOUNTSComponent, IDD_PARAMETERS_CHARTOFACCOUNTSFactoryComponent } from './chartofaccountsparameters/IDD_PARAMETERS_CHARTOFACCOUNTS.component';
import { IDD_COAGRAPHComponent, IDD_COAGRAPHFactoryComponent } from './chartofaccountsnavigation/IDD_COAGRAPH.component';
import { IDD_COAGROUPSComponent, IDD_COAGROUPSFactoryComponent } from './chartofaccountsgroups/IDD_COAGROUPS.component';
import { IDD_PLAN_ACCOUNTSComponent, IDD_PLAN_ACCOUNTSFactoryComponent } from './chartofaccounts/IDD_PLAN_ACCOUNTS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_PARAMETERS_CHARTOFACCOUNTS', component: IDD_PARAMETERS_CHARTOFACCOUNTSFactoryComponent },
            { path: 'IDD_COAGRAPH', component: IDD_COAGRAPHFactoryComponent },
            { path: 'IDD_COAGROUPS', component: IDD_COAGROUPSFactoryComponent },
            { path: 'IDD_PLAN_ACCOUNTS', component: IDD_PLAN_ACCOUNTSFactoryComponent },
        ])],
    declarations: [
            IDD_PARAMETERS_CHARTOFACCOUNTSComponent, IDD_PARAMETERS_CHARTOFACCOUNTSFactoryComponent,
            IDD_COAGRAPHComponent, IDD_COAGRAPHFactoryComponent,
            IDD_COAGROUPSComponent, IDD_COAGROUPSFactoryComponent,
            IDD_PLAN_ACCOUNTSComponent, IDD_PLAN_ACCOUNTSFactoryComponent,
    ],
    exports: [
            IDD_PARAMETERS_CHARTOFACCOUNTSFactoryComponent,
            IDD_COAGRAPHFactoryComponent,
            IDD_COAGROUPSFactoryComponent,
            IDD_PLAN_ACCOUNTSFactoryComponent,
    ],
    entryComponents: [
            IDD_PARAMETERS_CHARTOFACCOUNTSComponent,
            IDD_COAGRAPHComponent,
            IDD_COAGROUPSComponent,
            IDD_PLAN_ACCOUNTSComponent,
    ]
})


export class ChartOfAccountsModule { };