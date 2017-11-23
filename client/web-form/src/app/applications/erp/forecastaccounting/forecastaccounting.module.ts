import { IDD_FORECASTACC_SIMUCOUNTComponent, IDD_FORECASTACC_SIMUCOUNTFactoryComponent } from './accountingsimulations/IDD_FORECASTACC_SIMUCOUNT.component';
import { IDD_ACCOUNTINGSIMULATIONSSELECTIONComponent, IDD_ACCOUNTINGSIMULATIONSSELECTIONFactoryComponent } from './uiaccountingsimulationsselection/IDD_ACCOUNTINGSIMULATIONSSELECTION.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_FORECASTACC_SIMUCOUNT', component: IDD_FORECASTACC_SIMUCOUNTFactoryComponent },
            { path: 'IDD_ACCOUNTINGSIMULATIONSSELECTION', component: IDD_ACCOUNTINGSIMULATIONSSELECTIONFactoryComponent },
        ])],
    declarations: [
            IDD_FORECASTACC_SIMUCOUNTComponent, IDD_FORECASTACC_SIMUCOUNTFactoryComponent,
            IDD_ACCOUNTINGSIMULATIONSSELECTIONComponent, IDD_ACCOUNTINGSIMULATIONSSELECTIONFactoryComponent,
    ],
    exports: [
            IDD_FORECASTACC_SIMUCOUNTFactoryComponent,
            IDD_ACCOUNTINGSIMULATIONSSELECTIONFactoryComponent,
    ],
    entryComponents: [
            IDD_FORECASTACC_SIMUCOUNTComponent,
            IDD_ACCOUNTINGSIMULATIONSSELECTIONComponent,
    ]
})


export class ForecastAccountingModule { };