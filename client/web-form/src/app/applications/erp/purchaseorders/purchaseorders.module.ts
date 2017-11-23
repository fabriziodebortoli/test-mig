import { IDD_SUPPLIER_REORDERComponent, IDD_SUPPLIER_REORDERFactoryComponent } from './supplierreorder/IDD_SUPPLIER_REORDER.component';
import { IDD_REBUILD_ORDEREDComponent, IDD_REBUILD_ORDEREDFactoryComponent } from './rebuildingordered/IDD_REBUILD_ORDERED.component';
import { IDD_PRINT_PURCHORDComponent, IDD_PRINT_PURCHORDFactoryComponent } from './purchaseordersprint/IDD_PRINT_PURCHORD.component';
import { IDD_PURCHASE_ORDER_PARAMETERSComponent, IDD_PURCHASE_ORDER_PARAMETERSFactoryComponent } from './purchaseordersparameters/IDD_PURCHASE_ORDER_PARAMETERS.component';
import { IDD_DELETE_PURCHORDComponent, IDD_DELETE_PURCHORDFactoryComponent } from './purchaseordersdeleting/IDD_DELETE_PURCHORD.component';
import { IDD_PURCHORD_LOADComponent, IDD_PURCHORD_LOADFactoryComponent } from './purchaseorderloading/IDD_PURCHORD_LOAD.component';
import { IDD_PURCHORD_ACTUALComponent, IDD_PURCHORD_ACTUALFactoryComponent } from './purchaseordactualrebuilding/IDD_PURCHORD_ACTUAL.component';
import { IDD_PURCHASEORDER_SELECTIONS_ORDERSComponent, IDD_PURCHASEORDER_SELECTIONS_ORDERSFactoryComponent } from './purchaseord/IDD_PURCHASEORDER_SELECTIONS_ORDERS.component';
import { IDD_PURCHASE_ORDERComponent, IDD_PURCHASE_ORDERFactoryComponent } from './purchaseord/IDD_PURCHASE_ORDER.component';
import { IDD_PURCH_ORD_CONFComponent, IDD_PURCH_ORD_CONFFactoryComponent } from './poconfirmation/IDD_PURCH_ORD_CONF.component';
import { IDD_COPY_SUPP_CONFIRMComponent, IDD_COPY_SUPP_CONFIRMFactoryComponent } from './codeconfirm/IDD_COPY_SUPP_CONFIRM.component';
import { IDD_BOLComponent, IDD_BOLFactoryComponent } from './billofladingposting/IDD_BOL.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_SUPPLIER_REORDER', component: IDD_SUPPLIER_REORDERFactoryComponent },
            { path: 'IDD_REBUILD_ORDERED', component: IDD_REBUILD_ORDEREDFactoryComponent },
            { path: 'IDD_PRINT_PURCHORD', component: IDD_PRINT_PURCHORDFactoryComponent },
            { path: 'IDD_PURCHASE_ORDER_PARAMETERS', component: IDD_PURCHASE_ORDER_PARAMETERSFactoryComponent },
            { path: 'IDD_DELETE_PURCHORD', component: IDD_DELETE_PURCHORDFactoryComponent },
            { path: 'IDD_PURCHORD_LOAD', component: IDD_PURCHORD_LOADFactoryComponent },
            { path: 'IDD_PURCHORD_ACTUAL', component: IDD_PURCHORD_ACTUALFactoryComponent },
            { path: 'IDD_PURCHASEORDER_SELECTIONS_ORDERS', component: IDD_PURCHASEORDER_SELECTIONS_ORDERSFactoryComponent },
            { path: 'IDD_PURCHASE_ORDER', component: IDD_PURCHASE_ORDERFactoryComponent },
            { path: 'IDD_PURCH_ORD_CONF', component: IDD_PURCH_ORD_CONFFactoryComponent },
            { path: 'IDD_COPY_SUPP_CONFIRM', component: IDD_COPY_SUPP_CONFIRMFactoryComponent },
            { path: 'IDD_BOL', component: IDD_BOLFactoryComponent },
        ])],
    declarations: [
            IDD_SUPPLIER_REORDERComponent, IDD_SUPPLIER_REORDERFactoryComponent,
            IDD_REBUILD_ORDEREDComponent, IDD_REBUILD_ORDEREDFactoryComponent,
            IDD_PRINT_PURCHORDComponent, IDD_PRINT_PURCHORDFactoryComponent,
            IDD_PURCHASE_ORDER_PARAMETERSComponent, IDD_PURCHASE_ORDER_PARAMETERSFactoryComponent,
            IDD_DELETE_PURCHORDComponent, IDD_DELETE_PURCHORDFactoryComponent,
            IDD_PURCHORD_LOADComponent, IDD_PURCHORD_LOADFactoryComponent,
            IDD_PURCHORD_ACTUALComponent, IDD_PURCHORD_ACTUALFactoryComponent,
            IDD_PURCHASEORDER_SELECTIONS_ORDERSComponent, IDD_PURCHASEORDER_SELECTIONS_ORDERSFactoryComponent,
            IDD_PURCHASE_ORDERComponent, IDD_PURCHASE_ORDERFactoryComponent,
            IDD_PURCH_ORD_CONFComponent, IDD_PURCH_ORD_CONFFactoryComponent,
            IDD_COPY_SUPP_CONFIRMComponent, IDD_COPY_SUPP_CONFIRMFactoryComponent,
            IDD_BOLComponent, IDD_BOLFactoryComponent,
    ],
    exports: [
            IDD_SUPPLIER_REORDERFactoryComponent,
            IDD_REBUILD_ORDEREDFactoryComponent,
            IDD_PRINT_PURCHORDFactoryComponent,
            IDD_PURCHASE_ORDER_PARAMETERSFactoryComponent,
            IDD_DELETE_PURCHORDFactoryComponent,
            IDD_PURCHORD_LOADFactoryComponent,
            IDD_PURCHORD_ACTUALFactoryComponent,
            IDD_PURCHASEORDER_SELECTIONS_ORDERSFactoryComponent,
            IDD_PURCHASE_ORDERFactoryComponent,
            IDD_PURCH_ORD_CONFFactoryComponent,
            IDD_COPY_SUPP_CONFIRMFactoryComponent,
            IDD_BOLFactoryComponent,
    ],
    entryComponents: [
            IDD_SUPPLIER_REORDERComponent,
            IDD_REBUILD_ORDEREDComponent,
            IDD_PRINT_PURCHORDComponent,
            IDD_PURCHASE_ORDER_PARAMETERSComponent,
            IDD_DELETE_PURCHORDComponent,
            IDD_PURCHORD_LOADComponent,
            IDD_PURCHORD_ACTUALComponent,
            IDD_PURCHASEORDER_SELECTIONS_ORDERSComponent,
            IDD_PURCHASE_ORDERComponent,
            IDD_PURCH_ORD_CONFComponent,
            IDD_COPY_SUPP_CONFIRMComponent,
            IDD_BOLComponent,
    ]
})


export class PurchaseOrdersModule { };