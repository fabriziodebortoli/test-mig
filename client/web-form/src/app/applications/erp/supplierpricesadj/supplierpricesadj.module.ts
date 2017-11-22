import { IDD_ALF_CONFIGURATIONComponent, IDD_ALF_CONFIGURATIONFactoryComponent } from './supplierpricesadjconfig/IDD_ALF_CONFIGURATION.component';
import { IDD_ALF_MANAGEMENTSComponent, IDD_ALF_MANAGEMENTSFactoryComponent } from './supplierpricesadj/IDD_ALF_MANAGEMENTS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_ALF_CONFIGURATION', component: IDD_ALF_CONFIGURATIONFactoryComponent },
            { path: 'IDD_ALF_MANAGEMENTS', component: IDD_ALF_MANAGEMENTSFactoryComponent },
        ])],
    declarations: [
            IDD_ALF_CONFIGURATIONComponent, IDD_ALF_CONFIGURATIONFactoryComponent,
            IDD_ALF_MANAGEMENTSComponent, IDD_ALF_MANAGEMENTSFactoryComponent,
    ],
    exports: [
            IDD_ALF_CONFIGURATIONFactoryComponent,
            IDD_ALF_MANAGEMENTSFactoryComponent,
    ],
    entryComponents: [
            IDD_ALF_CONFIGURATIONComponent,
            IDD_ALF_MANAGEMENTSComponent,
    ]
})


export class SupplierPricesAdjModule { };