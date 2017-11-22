import { IDD_MBPROD_REORDComponent, IDD_MBPROD_REORDFactoryComponent } from './mbproductionreorder/IDD_MBPROD_REORD.component';
import { IDD_MBMONITORComponent, IDD_MBMONITORFactoryComponent } from './mbmonitor/IDD_MBMONITOR.component';
import { IDD_MAN_MOBILE_PARAMComponent, IDD_MAN_MOBILE_PARAMFactoryComponent } from './manmobileparameters/IDD_MAN_MOBILE_PARAM.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_MBPROD_REORD', component: IDD_MBPROD_REORDFactoryComponent },
            { path: 'IDD_MBMONITOR', component: IDD_MBMONITORFactoryComponent },
            { path: 'IDD_MAN_MOBILE_PARAM', component: IDD_MAN_MOBILE_PARAMFactoryComponent },
        ])],
    declarations: [
            IDD_MBPROD_REORDComponent, IDD_MBPROD_REORDFactoryComponent,
            IDD_MBMONITORComponent, IDD_MBMONITORFactoryComponent,
            IDD_MAN_MOBILE_PARAMComponent, IDD_MAN_MOBILE_PARAMFactoryComponent,
    ],
    exports: [
            IDD_MBPROD_REORDFactoryComponent,
            IDD_MBMONITORFactoryComponent,
            IDD_MAN_MOBILE_PARAMFactoryComponent,
    ],
    entryComponents: [
            IDD_MBPROD_REORDComponent,
            IDD_MBMONITORComponent,
            IDD_MAN_MOBILE_PARAMComponent,
    ]
})


export class ManufacturingMobileModule { };