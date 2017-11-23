import { IDD_DS_PROVIDERSComponent, IDD_DS_PROVIDERSFactoryComponent } from './providers/IDD_DS_PROVIDERS.component';
import { IDD_DATAVALIDATION_MONITORComponent, IDD_DATAVALIDATION_MONITORFactoryComponent } from './dvalidationmonitor/IDD_DATAVALIDATION_MONITOR.component';
import { IDD_DATASYNCHROComponent, IDD_DATASYNCHROFactoryComponent } from './uinotification/IDD_DATASYNCHRO.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_DS_PROVIDERS', component: IDD_DS_PROVIDERSFactoryComponent },
            { path: 'IDD_DATAVALIDATION_MONITOR', component: IDD_DATAVALIDATION_MONITORFactoryComponent },
            { path: 'IDD_DATASYNCHRO', component: IDD_DATASYNCHROFactoryComponent },
        ])],
    declarations: [
            IDD_DS_PROVIDERSComponent, IDD_DS_PROVIDERSFactoryComponent,
            IDD_DATAVALIDATION_MONITORComponent, IDD_DATAVALIDATION_MONITORFactoryComponent,
            IDD_DATASYNCHROComponent, IDD_DATASYNCHROFactoryComponent,
    ],
    exports: [
            IDD_DS_PROVIDERSFactoryComponent,
            IDD_DATAVALIDATION_MONITORFactoryComponent,
            IDD_DATASYNCHROFactoryComponent,
    ],
    entryComponents: [
            IDD_DS_PROVIDERSComponent,
            IDD_DATAVALIDATION_MONITORComponent,
            IDD_DATASYNCHROComponent,
    ]
})


export class TbDataSynchroClientModule { };