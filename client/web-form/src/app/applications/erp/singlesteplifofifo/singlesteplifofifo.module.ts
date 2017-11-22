import { IDD_RECEIPTSBATCHComponent, IDD_RECEIPTSBATCHFactoryComponent } from './liforeceipts/IDD_RECEIPTSBATCH.component';
import { IDD_LF_CLOSING_TO_DATEComponent, IDD_LF_CLOSING_TO_DATEFactoryComponent } from './lfclosingtodate/IDD_LF_CLOSING_TO_DATE.component';
import { IDD_LF_ANTI_CLOSING_TO_DATEComponent, IDD_LF_ANTI_CLOSING_TO_DATEFactoryComponent } from './lfanticlosingtodate/IDD_LF_ANTI_CLOSING_TO_DATE.component';
import { IDD_LIFOFIFO_DELETE_ORPHANSComponent, IDD_LIFOFIFO_DELETE_ORPHANSFactoryComponent } from './deleteorphans/IDD_LIFOFIFO_DELETE_ORPHANS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_RECEIPTSBATCH', component: IDD_RECEIPTSBATCHFactoryComponent },
            { path: 'IDD_LF_CLOSING_TO_DATE', component: IDD_LF_CLOSING_TO_DATEFactoryComponent },
            { path: 'IDD_LF_ANTI_CLOSING_TO_DATE', component: IDD_LF_ANTI_CLOSING_TO_DATEFactoryComponent },
            { path: 'IDD_LIFOFIFO_DELETE_ORPHANS', component: IDD_LIFOFIFO_DELETE_ORPHANSFactoryComponent },
        ])],
    declarations: [
            IDD_RECEIPTSBATCHComponent, IDD_RECEIPTSBATCHFactoryComponent,
            IDD_LF_CLOSING_TO_DATEComponent, IDD_LF_CLOSING_TO_DATEFactoryComponent,
            IDD_LF_ANTI_CLOSING_TO_DATEComponent, IDD_LF_ANTI_CLOSING_TO_DATEFactoryComponent,
            IDD_LIFOFIFO_DELETE_ORPHANSComponent, IDD_LIFOFIFO_DELETE_ORPHANSFactoryComponent,
    ],
    exports: [
            IDD_RECEIPTSBATCHFactoryComponent,
            IDD_LF_CLOSING_TO_DATEFactoryComponent,
            IDD_LF_ANTI_CLOSING_TO_DATEFactoryComponent,
            IDD_LIFOFIFO_DELETE_ORPHANSFactoryComponent,
    ],
    entryComponents: [
            IDD_RECEIPTSBATCHComponent,
            IDD_LF_CLOSING_TO_DATEComponent,
            IDD_LF_ANTI_CLOSING_TO_DATEComponent,
            IDD_LIFOFIFO_DELETE_ORPHANSComponent,
    ]
})


export class SingleStepLifoFifoModule { };