import { IDD_PARAMETERS_WEEE_FULLComponent, IDD_PARAMETERS_WEEE_FULLFactoryComponent } from './weeeparameters/IDD_PARAMETERS_WEEE_FULL.component';
import { IDD_WEEEASSOCIAComponent, IDD_WEEEASSOCIAFactoryComponent } from './weeeitemsctgassociation/IDD_WEEEASSOCIA.component';
import { IDD_WEEEENTRY_FULLComponent, IDD_WEEEENTRY_FULLFactoryComponent } from './weeeentries/IDD_WEEEENTRY_FULL.component';
import { IDD_WEEECTG_FULLComponent, IDD_WEEECTG_FULLFactoryComponent } from './weeectg/IDD_WEEECTG_FULL.component';
import { IDD_WEEE_CALCULATIONComponent, IDD_WEEE_CALCULATIONFactoryComponent } from './weeecalculation/IDD_WEEE_CALCULATION.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_PARAMETERS_WEEE_FULL', component: IDD_PARAMETERS_WEEE_FULLFactoryComponent },
            { path: 'IDD_WEEEASSOCIA', component: IDD_WEEEASSOCIAFactoryComponent },
            { path: 'IDD_WEEEENTRY_FULL', component: IDD_WEEEENTRY_FULLFactoryComponent },
            { path: 'IDD_WEEECTG_FULL', component: IDD_WEEECTG_FULLFactoryComponent },
            { path: 'IDD_WEEE_CALCULATION', component: IDD_WEEE_CALCULATIONFactoryComponent },
        ])],
    declarations: [
            IDD_PARAMETERS_WEEE_FULLComponent, IDD_PARAMETERS_WEEE_FULLFactoryComponent,
            IDD_WEEEASSOCIAComponent, IDD_WEEEASSOCIAFactoryComponent,
            IDD_WEEEENTRY_FULLComponent, IDD_WEEEENTRY_FULLFactoryComponent,
            IDD_WEEECTG_FULLComponent, IDD_WEEECTG_FULLFactoryComponent,
            IDD_WEEE_CALCULATIONComponent, IDD_WEEE_CALCULATIONFactoryComponent,
    ],
    exports: [
            IDD_PARAMETERS_WEEE_FULLFactoryComponent,
            IDD_WEEEASSOCIAFactoryComponent,
            IDD_WEEEENTRY_FULLFactoryComponent,
            IDD_WEEECTG_FULLFactoryComponent,
            IDD_WEEE_CALCULATIONFactoryComponent,
    ],
    entryComponents: [
            IDD_PARAMETERS_WEEE_FULLComponent,
            IDD_WEEEASSOCIAComponent,
            IDD_WEEEENTRY_FULLComponent,
            IDD_WEEECTG_FULLComponent,
            IDD_WEEE_CALCULATIONComponent,
    ]
})


export class WEEEModule { };