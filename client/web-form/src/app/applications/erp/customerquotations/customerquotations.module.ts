import { IDD_LOAD_QUOTATIONComponent, IDD_LOAD_QUOTATIONFactoryComponent } from './custquotaloading/IDD_LOAD_QUOTATION.component';
import { IDD_QUOTATIONS_FULLComponent, IDD_QUOTATIONS_FULLFactoryComponent } from './custquota/IDD_QUOTATIONS_FULL.component';
import { IDD_PRINT_QUOTATIONSComponent, IDD_PRINT_QUOTATIONSFactoryComponent } from './customerquotationsprint/IDD_PRINT_QUOTATIONS.component';
import { IDD_DELETE_QUOTATIONSComponent, IDD_DELETE_QUOTATIONSFactoryComponent } from './customerquotationsdeleting/IDD_DELETE_QUOTATIONS.component';
import { IDD_COPY_CUST_CONFIRMComponent, IDD_COPY_CUST_CONFIRMFactoryComponent } from './codeconfirm/IDD_COPY_CUST_CONFIRM.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_LOAD_QUOTATION', component: IDD_LOAD_QUOTATIONFactoryComponent },
            { path: 'IDD_QUOTATIONS_FULL', component: IDD_QUOTATIONS_FULLFactoryComponent },
            { path: 'IDD_PRINT_QUOTATIONS', component: IDD_PRINT_QUOTATIONSFactoryComponent },
            { path: 'IDD_DELETE_QUOTATIONS', component: IDD_DELETE_QUOTATIONSFactoryComponent },
            { path: 'IDD_COPY_CUST_CONFIRM', component: IDD_COPY_CUST_CONFIRMFactoryComponent },
        ])],
    declarations: [
            IDD_LOAD_QUOTATIONComponent, IDD_LOAD_QUOTATIONFactoryComponent,
            IDD_QUOTATIONS_FULLComponent, IDD_QUOTATIONS_FULLFactoryComponent,
            IDD_PRINT_QUOTATIONSComponent, IDD_PRINT_QUOTATIONSFactoryComponent,
            IDD_DELETE_QUOTATIONSComponent, IDD_DELETE_QUOTATIONSFactoryComponent,
            IDD_COPY_CUST_CONFIRMComponent, IDD_COPY_CUST_CONFIRMFactoryComponent,
    ],
    exports: [
            IDD_LOAD_QUOTATIONFactoryComponent,
            IDD_QUOTATIONS_FULLFactoryComponent,
            IDD_PRINT_QUOTATIONSFactoryComponent,
            IDD_DELETE_QUOTATIONSFactoryComponent,
            IDD_COPY_CUST_CONFIRMFactoryComponent,
    ],
    entryComponents: [
            IDD_LOAD_QUOTATIONComponent,
            IDD_QUOTATIONS_FULLComponent,
            IDD_PRINT_QUOTATIONSComponent,
            IDD_DELETE_QUOTATIONSComponent,
            IDD_COPY_CUST_CONFIRMComponent,
    ]
})


export class CustomerQuotationsModule { };