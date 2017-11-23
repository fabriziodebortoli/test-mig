import { IDD_USER_DEF_WMSComponent, IDD_USER_DEF_WMSFactoryComponent } from './userdefaultwms/IDD_USER_DEF_WMS.component';
import { IDD_USER_DEFAULT_SALES_CODES_SALES_FULLComponent, IDD_USER_DEFAULT_SALES_CODES_SALES_FULLFactoryComponent } from './userdefaultsales/IDD_USER_DEFAULT_SALES_CODES_SALES_FULL.component';
import { IDD_USER_DEF_SALEORD_TAB_MAN_FULLComponent, IDD_USER_DEF_SALEORD_TAB_MAN_FULLFactoryComponent } from './userdefaultsaleorders/IDD_USER_DEF_SALEORD_TAB_MAN_FULL.component';
import { IDD_USER_DEF_PURCH_CODES_PURCHASESComponent, IDD_USER_DEF_PURCH_CODES_PURCHASESFactoryComponent } from './userdefaultpurchases/IDD_USER_DEF_PURCH_CODES_PURCHASES.component';
import { IDD_USER_DEF_PURCH_ORD_TAB_MAN_FULLComponent, IDD_USER_DEF_PURCH_ORD_TAB_MAN_FULLFactoryComponent } from './userdefaultpurchaseorders/IDD_USER_DEF_PURCH_ORD_TAB_MAN_FULL.component';
import { IDD_USER_DEF_INVENTORY_FULLComponent, IDD_USER_DEF_INVENTORY_FULLFactoryComponent } from './userdefaultinventory/IDD_USER_DEF_INVENTORY_FULL.component';
import { IDD_USER_DEFAULT_COPYComponent, IDD_USER_DEFAULT_COPYFactoryComponent } from './uiuserdefaultcopy/IDD_USER_DEFAULT_COPY.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_USER_DEF_WMS', component: IDD_USER_DEF_WMSFactoryComponent },
            { path: 'IDD_USER_DEFAULT_SALES_CODES_SALES_FULL', component: IDD_USER_DEFAULT_SALES_CODES_SALES_FULLFactoryComponent },
            { path: 'IDD_USER_DEF_SALEORD_TAB_MAN_FULL', component: IDD_USER_DEF_SALEORD_TAB_MAN_FULLFactoryComponent },
            { path: 'IDD_USER_DEF_PURCH_CODES_PURCHASES', component: IDD_USER_DEF_PURCH_CODES_PURCHASESFactoryComponent },
            { path: 'IDD_USER_DEF_PURCH_ORD_TAB_MAN_FULL', component: IDD_USER_DEF_PURCH_ORD_TAB_MAN_FULLFactoryComponent },
            { path: 'IDD_USER_DEF_INVENTORY_FULL', component: IDD_USER_DEF_INVENTORY_FULLFactoryComponent },
            { path: 'IDD_USER_DEFAULT_COPY', component: IDD_USER_DEFAULT_COPYFactoryComponent },
        ])],
    declarations: [
            IDD_USER_DEF_WMSComponent, IDD_USER_DEF_WMSFactoryComponent,
            IDD_USER_DEFAULT_SALES_CODES_SALES_FULLComponent, IDD_USER_DEFAULT_SALES_CODES_SALES_FULLFactoryComponent,
            IDD_USER_DEF_SALEORD_TAB_MAN_FULLComponent, IDD_USER_DEF_SALEORD_TAB_MAN_FULLFactoryComponent,
            IDD_USER_DEF_PURCH_CODES_PURCHASESComponent, IDD_USER_DEF_PURCH_CODES_PURCHASESFactoryComponent,
            IDD_USER_DEF_PURCH_ORD_TAB_MAN_FULLComponent, IDD_USER_DEF_PURCH_ORD_TAB_MAN_FULLFactoryComponent,
            IDD_USER_DEF_INVENTORY_FULLComponent, IDD_USER_DEF_INVENTORY_FULLFactoryComponent,
            IDD_USER_DEFAULT_COPYComponent, IDD_USER_DEFAULT_COPYFactoryComponent,
    ],
    exports: [
            IDD_USER_DEF_WMSFactoryComponent,
            IDD_USER_DEFAULT_SALES_CODES_SALES_FULLFactoryComponent,
            IDD_USER_DEF_SALEORD_TAB_MAN_FULLFactoryComponent,
            IDD_USER_DEF_PURCH_CODES_PURCHASESFactoryComponent,
            IDD_USER_DEF_PURCH_ORD_TAB_MAN_FULLFactoryComponent,
            IDD_USER_DEF_INVENTORY_FULLFactoryComponent,
            IDD_USER_DEFAULT_COPYFactoryComponent,
    ],
    entryComponents: [
            IDD_USER_DEF_WMSComponent,
            IDD_USER_DEFAULT_SALES_CODES_SALES_FULLComponent,
            IDD_USER_DEF_SALEORD_TAB_MAN_FULLComponent,
            IDD_USER_DEF_PURCH_CODES_PURCHASESComponent,
            IDD_USER_DEF_PURCH_ORD_TAB_MAN_FULLComponent,
            IDD_USER_DEF_INVENTORY_FULLComponent,
            IDD_USER_DEFAULT_COPYComponent,
    ]
})


export class UserDefaultModule { };