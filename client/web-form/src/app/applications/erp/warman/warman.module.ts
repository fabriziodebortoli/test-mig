import { IDD_TRASF_REQComponent, IDD_TRASF_REQFactoryComponent } from './transferrequest/IDD_TRASF_REQ.component';
import { IDD_WMS_MAT_REORDERComponent, IDD_WMS_MAT_REORDERFactoryComponent } from './bdwmsmaterialsreorder/IDD_WMS_MAT_REORDER.component';
import { IDD_WARMAN_INTERIM_CREATE_TOComponent, IDD_WARMAN_INTERIM_CREATE_TOFactoryComponent } from './bdwarmaninterimanalysis/IDD_WARMAN_INTERIM_CREATE_TO.component';
import { IDD_WARMAN_INTERIM_ANALYSISComponent, IDD_WARMAN_INTERIM_ANALYSISFactoryComponent } from './bdwarmaninterimanalysis/IDD_WARMAN_INTERIM_ANALYSIS.component';
import { IDD_TR_RUNComponent, IDD_TR_RUNFactoryComponent } from './bdtrrun/IDD_TR_RUN.component';
import { IDD_RUN_QTY_TO_PRODComponent, IDD_RUN_QTY_TO_PRODFactoryComponent } from './bdtrrun/IDD_RUN_QTY_TO_PROD.component';
import { IDD_TO_GENERATIONComponent, IDD_TO_GENERATIONFactoryComponent } from './bdtogeneration/IDD_TO_GENERATION.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TRASF_REQ', component: IDD_TRASF_REQFactoryComponent },
            { path: 'IDD_WMS_MAT_REORDER', component: IDD_WMS_MAT_REORDERFactoryComponent },
            { path: 'IDD_WARMAN_INTERIM_CREATE_TO', component: IDD_WARMAN_INTERIM_CREATE_TOFactoryComponent },
            { path: 'IDD_WARMAN_INTERIM_ANALYSIS', component: IDD_WARMAN_INTERIM_ANALYSISFactoryComponent },
            { path: 'IDD_TR_RUN', component: IDD_TR_RUNFactoryComponent },
            { path: 'IDD_RUN_QTY_TO_PROD', component: IDD_RUN_QTY_TO_PRODFactoryComponent },
            { path: 'IDD_TO_GENERATION', component: IDD_TO_GENERATIONFactoryComponent },
        ])],
    declarations: [
            IDD_TRASF_REQComponent, IDD_TRASF_REQFactoryComponent,
            IDD_WMS_MAT_REORDERComponent, IDD_WMS_MAT_REORDERFactoryComponent,
            IDD_WARMAN_INTERIM_CREATE_TOComponent, IDD_WARMAN_INTERIM_CREATE_TOFactoryComponent,
            IDD_WARMAN_INTERIM_ANALYSISComponent, IDD_WARMAN_INTERIM_ANALYSISFactoryComponent,
            IDD_TR_RUNComponent, IDD_TR_RUNFactoryComponent,
            IDD_RUN_QTY_TO_PRODComponent, IDD_RUN_QTY_TO_PRODFactoryComponent,
            IDD_TO_GENERATIONComponent, IDD_TO_GENERATIONFactoryComponent,
    ],
    exports: [
            IDD_TRASF_REQFactoryComponent,
            IDD_WMS_MAT_REORDERFactoryComponent,
            IDD_WARMAN_INTERIM_CREATE_TOFactoryComponent,
            IDD_WARMAN_INTERIM_ANALYSISFactoryComponent,
            IDD_TR_RUNFactoryComponent,
            IDD_RUN_QTY_TO_PRODFactoryComponent,
            IDD_TO_GENERATIONFactoryComponent,
    ],
    entryComponents: [
            IDD_TRASF_REQComponent,
            IDD_WMS_MAT_REORDERComponent,
            IDD_WARMAN_INTERIM_CREATE_TOComponent,
            IDD_WARMAN_INTERIM_ANALYSISComponent,
            IDD_TR_RUNComponent,
            IDD_RUN_QTY_TO_PRODComponent,
            IDD_TO_GENERATIONComponent,
    ]
})


export class WarManModule { };