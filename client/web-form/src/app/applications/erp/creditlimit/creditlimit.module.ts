import { IDD_SALE_ORDERS_UNBLOCKComponent, IDD_SALE_ORDERS_UNBLOCKFactoryComponent } from './saleordersunblock/IDD_SALE_ORDERS_UNBLOCK.component';
import { IDD_CREDIT_PARAMETERSComponent, IDD_CREDIT_PARAMETERSFactoryComponent } from './creditparameters/IDD_CREDIT_PARAMETERS.component';
import { IDD_CREDIT_LIMIT_VIEWERComponent, IDD_CREDIT_LIMIT_VIEWERFactoryComponent } from './creditlimitviewer/IDD_CREDIT_LIMIT_VIEWER.component';
import { IDD_CREDIT_LIMIT_STARTComponent, IDD_CREDIT_LIMIT_STARTFactoryComponent } from './creditlimitstart/IDD_CREDIT_LIMIT_START.component';
import { IDD_CREDITLIMIT_REBUILDComponent, IDD_CREDITLIMIT_REBUILDFactoryComponent } from './creditlimitrebuilding/IDD_CREDITLIMIT_REBUILD.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_SALE_ORDERS_UNBLOCK', component: IDD_SALE_ORDERS_UNBLOCKFactoryComponent },
            { path: 'IDD_CREDIT_PARAMETERS', component: IDD_CREDIT_PARAMETERSFactoryComponent },
            { path: 'IDD_CREDIT_LIMIT_VIEWER', component: IDD_CREDIT_LIMIT_VIEWERFactoryComponent },
            { path: 'IDD_CREDIT_LIMIT_START', component: IDD_CREDIT_LIMIT_STARTFactoryComponent },
            { path: 'IDD_CREDITLIMIT_REBUILD', component: IDD_CREDITLIMIT_REBUILDFactoryComponent },
        ])],
    declarations: [
            IDD_SALE_ORDERS_UNBLOCKComponent, IDD_SALE_ORDERS_UNBLOCKFactoryComponent,
            IDD_CREDIT_PARAMETERSComponent, IDD_CREDIT_PARAMETERSFactoryComponent,
            IDD_CREDIT_LIMIT_VIEWERComponent, IDD_CREDIT_LIMIT_VIEWERFactoryComponent,
            IDD_CREDIT_LIMIT_STARTComponent, IDD_CREDIT_LIMIT_STARTFactoryComponent,
            IDD_CREDITLIMIT_REBUILDComponent, IDD_CREDITLIMIT_REBUILDFactoryComponent,
    ],
    exports: [
            IDD_SALE_ORDERS_UNBLOCKFactoryComponent,
            IDD_CREDIT_PARAMETERSFactoryComponent,
            IDD_CREDIT_LIMIT_VIEWERFactoryComponent,
            IDD_CREDIT_LIMIT_STARTFactoryComponent,
            IDD_CREDITLIMIT_REBUILDFactoryComponent,
    ],
    entryComponents: [
            IDD_SALE_ORDERS_UNBLOCKComponent,
            IDD_CREDIT_PARAMETERSComponent,
            IDD_CREDIT_LIMIT_VIEWERComponent,
            IDD_CREDIT_LIMIT_STARTComponent,
            IDD_CREDITLIMIT_REBUILDComponent,
    ]
})


export class CreditLimitModule { };