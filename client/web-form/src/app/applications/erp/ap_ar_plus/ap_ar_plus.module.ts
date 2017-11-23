import { IDD_OUTSTANDINGBILLSComponent, IDD_OUTSTANDINGBILLSFactoryComponent } from './uioutstandingbills/IDD_OUTSTANDINGBILLS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_OUTSTANDINGBILLS', component: IDD_OUTSTANDINGBILLSFactoryComponent },
        ])],
    declarations: [
            IDD_OUTSTANDINGBILLSComponent, IDD_OUTSTANDINGBILLSFactoryComponent,
    ],
    exports: [
            IDD_OUTSTANDINGBILLSFactoryComponent,
    ],
    entryComponents: [
            IDD_OUTSTANDINGBILLSComponent,
    ]
})


export class AP_AR_PlusModule { };