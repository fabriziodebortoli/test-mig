import { IDD_STD_COST_UPDComponent, IDD_STD_COST_UPDFactoryComponent } from './standardcostrebuilding/IDD_STD_COST_UPD.component';
import { IDD_WTD_PRICELISTSComponent, IDD_WTD_PRICELISTSFactoryComponent } from './pricelistswizard/IDD_WTD_PRICELISTS.component';
import { IDD_PRICE_LISTS_DELETEComponent, IDD_PRICE_LISTS_DELETEFactoryComponent } from './pricelistsdeleting/IDD_PRICE_LISTS_DELETE.component';
import { IDD_PRICELISTS_FULLComponent, IDD_PRICELISTS_FULLFactoryComponent } from './pricelists/IDD_PRICELISTS_FULL.component';
import { IDD_ITM_LISComponent, IDD_ITM_LISFactoryComponent } from './itemspricelists/IDD_ITM_LIS.component';
import { IDD_CUST_PRICE_UPDATEComponent, IDD_CUST_PRICE_UPDATEFactoryComponent } from './customerpricelistupdate/IDD_CUST_PRICE_UPDATE.component';
import { IDD_SALE_PRICE_POLICIES_PARAMETERSComponent, IDD_SALE_PRICE_POLICIES_PARAMETERSFactoryComponent } from './uipricespolicies/IDD_SALE_PRICE_POLICIES_PARAMETERS.component';
import { IDD_PURCHASE_PRICE_POLICIES_PARAMETERSComponent, IDD_PURCHASE_PRICE_POLICIES_PARAMETERSFactoryComponent } from './uipricespolicies/IDD_PURCHASE_PRICE_POLICIES_PARAMETERS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_STD_COST_UPD', component: IDD_STD_COST_UPDFactoryComponent },
            { path: 'IDD_WTD_PRICELISTS', component: IDD_WTD_PRICELISTSFactoryComponent },
            { path: 'IDD_PRICE_LISTS_DELETE', component: IDD_PRICE_LISTS_DELETEFactoryComponent },
            { path: 'IDD_PRICELISTS_FULL', component: IDD_PRICELISTS_FULLFactoryComponent },
            { path: 'IDD_ITM_LIS', component: IDD_ITM_LISFactoryComponent },
            { path: 'IDD_CUST_PRICE_UPDATE', component: IDD_CUST_PRICE_UPDATEFactoryComponent },
            { path: 'IDD_SALE_PRICE_POLICIES_PARAMETERS', component: IDD_SALE_PRICE_POLICIES_PARAMETERSFactoryComponent },
            { path: 'IDD_PURCHASE_PRICE_POLICIES_PARAMETERS', component: IDD_PURCHASE_PRICE_POLICIES_PARAMETERSFactoryComponent },
        ])],
    declarations: [
            IDD_STD_COST_UPDComponent, IDD_STD_COST_UPDFactoryComponent,
            IDD_WTD_PRICELISTSComponent, IDD_WTD_PRICELISTSFactoryComponent,
            IDD_PRICE_LISTS_DELETEComponent, IDD_PRICE_LISTS_DELETEFactoryComponent,
            IDD_PRICELISTS_FULLComponent, IDD_PRICELISTS_FULLFactoryComponent,
            IDD_ITM_LISComponent, IDD_ITM_LISFactoryComponent,
            IDD_CUST_PRICE_UPDATEComponent, IDD_CUST_PRICE_UPDATEFactoryComponent,
            IDD_SALE_PRICE_POLICIES_PARAMETERSComponent, IDD_SALE_PRICE_POLICIES_PARAMETERSFactoryComponent,
            IDD_PURCHASE_PRICE_POLICIES_PARAMETERSComponent, IDD_PURCHASE_PRICE_POLICIES_PARAMETERSFactoryComponent,
    ],
    exports: [
            IDD_STD_COST_UPDFactoryComponent,
            IDD_WTD_PRICELISTSFactoryComponent,
            IDD_PRICE_LISTS_DELETEFactoryComponent,
            IDD_PRICELISTS_FULLFactoryComponent,
            IDD_ITM_LISFactoryComponent,
            IDD_CUST_PRICE_UPDATEFactoryComponent,
            IDD_SALE_PRICE_POLICIES_PARAMETERSFactoryComponent,
            IDD_PURCHASE_PRICE_POLICIES_PARAMETERSFactoryComponent,
    ],
    entryComponents: [
            IDD_STD_COST_UPDComponent,
            IDD_WTD_PRICELISTSComponent,
            IDD_PRICE_LISTS_DELETEComponent,
            IDD_PRICELISTS_FULLComponent,
            IDD_ITM_LISComponent,
            IDD_CUST_PRICE_UPDATEComponent,
            IDD_SALE_PRICE_POLICIES_PARAMETERSComponent,
            IDD_PURCHASE_PRICE_POLICIES_PARAMETERSComponent,
    ]
})


export class PricePoliciesModule { };