import { IDD_PRODUCTLINESComponent, IDD_PRODUCTLINESFactoryComponent } from './productlines/IDD_PRODUCTLINES.component';
import { IDD_PRODUCTLINEGROUPSComponent, IDD_PRODUCTLINEGROUPSFactoryComponent } from './productlinegroups/IDD_PRODUCTLINEGROUPS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_PRODUCTLINES', component: IDD_PRODUCTLINESFactoryComponent },
            { path: 'IDD_PRODUCTLINEGROUPS', component: IDD_PRODUCTLINEGROUPSFactoryComponent },
        ])],
    declarations: [
            IDD_PRODUCTLINESComponent, IDD_PRODUCTLINESFactoryComponent,
            IDD_PRODUCTLINEGROUPSComponent, IDD_PRODUCTLINEGROUPSFactoryComponent,
    ],
    exports: [
            IDD_PRODUCTLINESFactoryComponent,
            IDD_PRODUCTLINEGROUPSFactoryComponent,
    ],
    entryComponents: [
            IDD_PRODUCTLINESComponent,
            IDD_PRODUCTLINEGROUPSComponent,
    ]
})


export class ProductLinesModule { };