import { IDD_TRANSPORTMODEComponent, IDD_TRANSPORTMODEFactoryComponent } from './transport/IDD_TRANSPORTMODE.component';
import { IDD_SHIPPING_REASONSComponent, IDD_SHIPPING_REASONSFactoryComponent } from './shippingreasons/IDD_SHIPPING_REASONS.component';
import { IDD_SHIPPING_BYComponent, IDD_SHIPPING_BYFactoryComponent } from './shippingby/IDD_SHIPPING_BY.component';
import { IDD_RETURNREASONComponent, IDD_RETURNREASONFactoryComponent } from './returnreason/IDD_RETURNREASON.component';
import { IDD_PORTSComponent, IDD_PORTSFactoryComponent } from './ports/IDD_PORTS.component';
import { IDD_PACKAGESComponent, IDD_PACKAGESFactoryComponent } from './packages/IDD_PACKAGES.component';
import { IDD_GOODS_APPEARANCEComponent, IDD_GOODS_APPEARANCEFactoryComponent } from './goodsappearance/IDD_GOODS_APPEARANCE.component';
import { IDD_CARRIERSComponent, IDD_CARRIERSFactoryComponent } from './carriers/IDD_CARRIERS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TRANSPORTMODE', component: IDD_TRANSPORTMODEFactoryComponent },
            { path: 'IDD_SHIPPING_REASONS', component: IDD_SHIPPING_REASONSFactoryComponent },
            { path: 'IDD_SHIPPING_BY', component: IDD_SHIPPING_BYFactoryComponent },
            { path: 'IDD_RETURNREASON', component: IDD_RETURNREASONFactoryComponent },
            { path: 'IDD_PORTS', component: IDD_PORTSFactoryComponent },
            { path: 'IDD_PACKAGES', component: IDD_PACKAGESFactoryComponent },
            { path: 'IDD_GOODS_APPEARANCE', component: IDD_GOODS_APPEARANCEFactoryComponent },
            { path: 'IDD_CARRIERS', component: IDD_CARRIERSFactoryComponent },
        ])],
    declarations: [
            IDD_TRANSPORTMODEComponent, IDD_TRANSPORTMODEFactoryComponent,
            IDD_SHIPPING_REASONSComponent, IDD_SHIPPING_REASONSFactoryComponent,
            IDD_SHIPPING_BYComponent, IDD_SHIPPING_BYFactoryComponent,
            IDD_RETURNREASONComponent, IDD_RETURNREASONFactoryComponent,
            IDD_PORTSComponent, IDD_PORTSFactoryComponent,
            IDD_PACKAGESComponent, IDD_PACKAGESFactoryComponent,
            IDD_GOODS_APPEARANCEComponent, IDD_GOODS_APPEARANCEFactoryComponent,
            IDD_CARRIERSComponent, IDD_CARRIERSFactoryComponent,
    ],
    exports: [
            IDD_TRANSPORTMODEFactoryComponent,
            IDD_SHIPPING_REASONSFactoryComponent,
            IDD_SHIPPING_BYFactoryComponent,
            IDD_RETURNREASONFactoryComponent,
            IDD_PORTSFactoryComponent,
            IDD_PACKAGESFactoryComponent,
            IDD_GOODS_APPEARANCEFactoryComponent,
            IDD_CARRIERSFactoryComponent,
    ],
    entryComponents: [
            IDD_TRANSPORTMODEComponent,
            IDD_SHIPPING_REASONSComponent,
            IDD_SHIPPING_BYComponent,
            IDD_RETURNREASONComponent,
            IDD_PORTSComponent,
            IDD_PACKAGESComponent,
            IDD_GOODS_APPEARANCEComponent,
            IDD_CARRIERSComponent,
    ]
})


export class ShippingsModule { };