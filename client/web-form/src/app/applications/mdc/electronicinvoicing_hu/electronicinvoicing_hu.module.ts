import { IDD_EI_HU_TRASMISSIONComponent, IDD_EI_HU_TRASMISSIONFactoryComponent } from './eihutrasmission/IDD_EI_HU_TRASMISSION.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_EI_HU_TRASMISSION', component: IDD_EI_HU_TRASMISSIONFactoryComponent },
        ])],
    declarations: [
            IDD_EI_HU_TRASMISSIONComponent, IDD_EI_HU_TRASMISSIONFactoryComponent,
    ],
    exports: [
            IDD_EI_HU_TRASMISSIONFactoryComponent,
    ],
    entryComponents: [
            IDD_EI_HU_TRASMISSIONComponent,
    ]
})


export class ElectronicInvoicing_HUModule { };