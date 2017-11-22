import { IDD_PARAMETERS_SALEORDERS_FULLComponent, IDD_PARAMETERS_SALEORDERS_FULLFactoryComponent } from './salesordparameters/IDD_PARAMETERS_SALEORDERS_FULL.component';
import { IDD_PRINT_SALEORDComponent, IDD_PRINT_SALEORDFactoryComponent } from './saleordersprint/IDD_PRINT_SALEORD.component';
import { IDD_DELETE_SALEORDComponent, IDD_DELETE_SALEORDFactoryComponent } from './saleordersdeleting/IDD_DELETE_SALEORD.component';
import { IDD_SALE_ORD_DEALLOCATIONComponent, IDD_SALE_ORD_DEALLOCATIONFactoryComponent } from './saleordersdeallocation/IDD_SALE_ORD_DEALLOCATION.component';
import { IDD_SALESORD_LOADComponent, IDD_SALESORD_LOADFactoryComponent } from './saleorderloading/IDD_SALESORD_LOAD.component';
import { IDD_SALEORD_ACTUALComponent, IDD_SALEORD_ACTUALFactoryComponent } from './saleordactualrebuilding/IDD_SALEORD_ACTUAL.component';
import { IDD_SALESORDERComponent, IDD_SALESORDERFactoryComponent } from './saleord/IDD_SALESORDER.component';
import { IDD_REBUILD_RESERVEDComponent, IDD_REBUILD_RESERVEDFactoryComponent } from './rebuildreserved/IDD_REBUILD_RESERVED.component';
import { IDD_REBUILD_ALLOCATEDComponent, IDD_REBUILD_ALLOCATEDFactoryComponent } from './rebuildallocated/IDD_REBUILD_ALLOCATED.component';
import { IDD_ALLOCATION_AREA_FULLComponent, IDD_ALLOCATION_AREA_FULLFactoryComponent } from './allocationarea/IDD_ALLOCATION_AREA_FULL.component';
import { IDD_SINGLE_ORDER_ALLOCATIONComponent, IDD_SINGLE_ORDER_ALLOCATIONFactoryComponent } from './uisaleordersallocation/IDD_SINGLE_ORDER_ALLOCATION.component';
import { IDD_SALE_ORDER_ALLOCATION_DETAILComponent, IDD_SALE_ORDER_ALLOCATION_DETAILFactoryComponent } from './uisaleordersallocation/IDD_SALE_ORDER_ALLOCATION_DETAIL.component';
import { IDD_SALE_ORDER_ALLOCATIONComponent, IDD_SALE_ORDER_ALLOCATIONFactoryComponent } from './uisaleordersallocation/IDD_SALE_ORDER_ALLOCATION.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_PARAMETERS_SALEORDERS_FULL', component: IDD_PARAMETERS_SALEORDERS_FULLFactoryComponent },
            { path: 'IDD_PRINT_SALEORD', component: IDD_PRINT_SALEORDFactoryComponent },
            { path: 'IDD_DELETE_SALEORD', component: IDD_DELETE_SALEORDFactoryComponent },
            { path: 'IDD_SALE_ORD_DEALLOCATION', component: IDD_SALE_ORD_DEALLOCATIONFactoryComponent },
            { path: 'IDD_SALESORD_LOAD', component: IDD_SALESORD_LOADFactoryComponent },
            { path: 'IDD_SALEORD_ACTUAL', component: IDD_SALEORD_ACTUALFactoryComponent },
            { path: 'IDD_SALESORDER', component: IDD_SALESORDERFactoryComponent },
            { path: 'IDD_REBUILD_RESERVED', component: IDD_REBUILD_RESERVEDFactoryComponent },
            { path: 'IDD_REBUILD_ALLOCATED', component: IDD_REBUILD_ALLOCATEDFactoryComponent },
            { path: 'IDD_ALLOCATION_AREA_FULL', component: IDD_ALLOCATION_AREA_FULLFactoryComponent },
            { path: 'IDD_SINGLE_ORDER_ALLOCATION', component: IDD_SINGLE_ORDER_ALLOCATIONFactoryComponent },
            { path: 'IDD_SALE_ORDER_ALLOCATION_DETAIL', component: IDD_SALE_ORDER_ALLOCATION_DETAILFactoryComponent },
            { path: 'IDD_SALE_ORDER_ALLOCATION', component: IDD_SALE_ORDER_ALLOCATIONFactoryComponent },
        ])],
    declarations: [
            IDD_PARAMETERS_SALEORDERS_FULLComponent, IDD_PARAMETERS_SALEORDERS_FULLFactoryComponent,
            IDD_PRINT_SALEORDComponent, IDD_PRINT_SALEORDFactoryComponent,
            IDD_DELETE_SALEORDComponent, IDD_DELETE_SALEORDFactoryComponent,
            IDD_SALE_ORD_DEALLOCATIONComponent, IDD_SALE_ORD_DEALLOCATIONFactoryComponent,
            IDD_SALESORD_LOADComponent, IDD_SALESORD_LOADFactoryComponent,
            IDD_SALEORD_ACTUALComponent, IDD_SALEORD_ACTUALFactoryComponent,
            IDD_SALESORDERComponent, IDD_SALESORDERFactoryComponent,
            IDD_REBUILD_RESERVEDComponent, IDD_REBUILD_RESERVEDFactoryComponent,
            IDD_REBUILD_ALLOCATEDComponent, IDD_REBUILD_ALLOCATEDFactoryComponent,
            IDD_ALLOCATION_AREA_FULLComponent, IDD_ALLOCATION_AREA_FULLFactoryComponent,
            IDD_SINGLE_ORDER_ALLOCATIONComponent, IDD_SINGLE_ORDER_ALLOCATIONFactoryComponent,
            IDD_SALE_ORDER_ALLOCATION_DETAILComponent, IDD_SALE_ORDER_ALLOCATION_DETAILFactoryComponent,
            IDD_SALE_ORDER_ALLOCATIONComponent, IDD_SALE_ORDER_ALLOCATIONFactoryComponent,
    ],
    exports: [
            IDD_PARAMETERS_SALEORDERS_FULLFactoryComponent,
            IDD_PRINT_SALEORDFactoryComponent,
            IDD_DELETE_SALEORDFactoryComponent,
            IDD_SALE_ORD_DEALLOCATIONFactoryComponent,
            IDD_SALESORD_LOADFactoryComponent,
            IDD_SALEORD_ACTUALFactoryComponent,
            IDD_SALESORDERFactoryComponent,
            IDD_REBUILD_RESERVEDFactoryComponent,
            IDD_REBUILD_ALLOCATEDFactoryComponent,
            IDD_ALLOCATION_AREA_FULLFactoryComponent,
            IDD_SINGLE_ORDER_ALLOCATIONFactoryComponent,
            IDD_SALE_ORDER_ALLOCATION_DETAILFactoryComponent,
            IDD_SALE_ORDER_ALLOCATIONFactoryComponent,
    ],
    entryComponents: [
            IDD_PARAMETERS_SALEORDERS_FULLComponent,
            IDD_PRINT_SALEORDComponent,
            IDD_DELETE_SALEORDComponent,
            IDD_SALE_ORD_DEALLOCATIONComponent,
            IDD_SALESORD_LOADComponent,
            IDD_SALEORD_ACTUALComponent,
            IDD_SALESORDERComponent,
            IDD_REBUILD_RESERVEDComponent,
            IDD_REBUILD_ALLOCATEDComponent,
            IDD_ALLOCATION_AREA_FULLComponent,
            IDD_SINGLE_ORDER_ALLOCATIONComponent,
            IDD_SALE_ORDER_ALLOCATION_DETAILComponent,
            IDD_SALE_ORDER_ALLOCATIONComponent,
    ]
})


export class SaleOrdersModule { };