import { IDD_CHART_REFRESHComponent, IDD_CHART_REFRESHFactoryComponent } from './wcchart/IDD_CHART_REFRESH.component';
import { IDD_CHART_MAINComponent, IDD_CHART_MAINFactoryComponent } from './wcchart/IDD_CHART_MAIN.component';
import { IDD_GANTT_STEPComponent, IDD_GANTT_STEPFactoryComponent } from './rtgstepsgantt/IDD_GANTT_STEP.component';
import { IDD_GANTT_MOComponent, IDD_GANTT_MOFactoryComponent } from './mogantt/IDD_GANTT_MO.component';
import { IDD_LOAD_COMPOSITIONComponent, IDD_LOAD_COMPOSITIONFactoryComponent } from './loadcomposition/IDD_LOAD_COMPOSITION.component';
import { IDD_CRPComponent, IDD_CRPFactoryComponent } from './crp/IDD_CRP.component';
import { IDD_CRP_CONFIRMATIONComponent, IDD_CRP_CONFIRMATIONFactoryComponent } from './confirmationcrpmo/IDD_CRP_CONFIRMATION.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_CHART_REFRESH', component: IDD_CHART_REFRESHFactoryComponent },
            { path: 'IDD_CHART_MAIN', component: IDD_CHART_MAINFactoryComponent },
            { path: 'IDD_GANTT_STEP', component: IDD_GANTT_STEPFactoryComponent },
            { path: 'IDD_GANTT_MO', component: IDD_GANTT_MOFactoryComponent },
            { path: 'IDD_LOAD_COMPOSITION', component: IDD_LOAD_COMPOSITIONFactoryComponent },
            { path: 'IDD_CRP', component: IDD_CRPFactoryComponent },
            { path: 'IDD_CRP_CONFIRMATION', component: IDD_CRP_CONFIRMATIONFactoryComponent },
        ])],
    declarations: [
            IDD_CHART_REFRESHComponent, IDD_CHART_REFRESHFactoryComponent,
            IDD_CHART_MAINComponent, IDD_CHART_MAINFactoryComponent,
            IDD_GANTT_STEPComponent, IDD_GANTT_STEPFactoryComponent,
            IDD_GANTT_MOComponent, IDD_GANTT_MOFactoryComponent,
            IDD_LOAD_COMPOSITIONComponent, IDD_LOAD_COMPOSITIONFactoryComponent,
            IDD_CRPComponent, IDD_CRPFactoryComponent,
            IDD_CRP_CONFIRMATIONComponent, IDD_CRP_CONFIRMATIONFactoryComponent,
    ],
    exports: [
            IDD_CHART_REFRESHFactoryComponent,
            IDD_CHART_MAINFactoryComponent,
            IDD_GANTT_STEPFactoryComponent,
            IDD_GANTT_MOFactoryComponent,
            IDD_LOAD_COMPOSITIONFactoryComponent,
            IDD_CRPFactoryComponent,
            IDD_CRP_CONFIRMATIONFactoryComponent,
    ],
    entryComponents: [
            IDD_CHART_REFRESHComponent,
            IDD_CHART_MAINComponent,
            IDD_GANTT_STEPComponent,
            IDD_GANTT_MOComponent,
            IDD_LOAD_COMPOSITIONComponent,
            IDD_CRPComponent,
            IDD_CRP_CONFIRMATIONComponent,
    ]
})


export class CRPModule { };