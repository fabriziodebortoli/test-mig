import { IDD_TRAVELAGENCY_TAXComponent, IDD_TRAVELAGENCY_TAXFactoryComponent } from './travelagencytax/IDD_TRAVELAGENCY_TAX.component';
import { IDD_DATA_PLAFOND_TAXComponent, IDD_DATA_PLAFOND_TAXFactoryComponent } from './taxplafond/IDD_DATA_PLAFOND_TAX.component';
import { IDD_PRORATAComponent, IDD_PRORATAFactoryComponent } from './prorataprint/IDD_PRORATA.component';
import { IDD_MARGIN_TAXComponent, IDD_MARGIN_TAXFactoryComponent } from './margintax/IDD_MARGIN_TAX.component';
import { IDD_DECLARATION_OF_INTENT_NUMBERSComponent, IDD_DECLARATION_OF_INTENT_NUMBERSFactoryComponent } from './declarationofintentnumbers/IDD_DECLARATION_OF_INTENT_NUMBERS.component';
import { IDD_DECLARATION_OF_INTENT_DELETEComponent, IDD_DECLARATION_OF_INTENT_DELETEFactoryComponent } from './uideclarationofintentdeleting/IDD_DECLARATION_OF_INTENT_DELETE.component';
import { IDD_DECLARATION_OF_INTENTComponent, IDD_DECLARATION_OF_INTENTFactoryComponent } from './uideclarationofintent/IDD_DECLARATION_OF_INTENT.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TRAVELAGENCY_TAX', component: IDD_TRAVELAGENCY_TAXFactoryComponent },
            { path: 'IDD_DATA_PLAFOND_TAX', component: IDD_DATA_PLAFOND_TAXFactoryComponent },
            { path: 'IDD_PRORATA', component: IDD_PRORATAFactoryComponent },
            { path: 'IDD_MARGIN_TAX', component: IDD_MARGIN_TAXFactoryComponent },
            { path: 'IDD_DECLARATION_OF_INTENT_NUMBERS', component: IDD_DECLARATION_OF_INTENT_NUMBERSFactoryComponent },
            { path: 'IDD_DECLARATION_OF_INTENT_DELETE', component: IDD_DECLARATION_OF_INTENT_DELETEFactoryComponent },
            { path: 'IDD_DECLARATION_OF_INTENT', component: IDD_DECLARATION_OF_INTENTFactoryComponent },
        ])],
    declarations: [
            IDD_TRAVELAGENCY_TAXComponent, IDD_TRAVELAGENCY_TAXFactoryComponent,
            IDD_DATA_PLAFOND_TAXComponent, IDD_DATA_PLAFOND_TAXFactoryComponent,
            IDD_PRORATAComponent, IDD_PRORATAFactoryComponent,
            IDD_MARGIN_TAXComponent, IDD_MARGIN_TAXFactoryComponent,
            IDD_DECLARATION_OF_INTENT_NUMBERSComponent, IDD_DECLARATION_OF_INTENT_NUMBERSFactoryComponent,
            IDD_DECLARATION_OF_INTENT_DELETEComponent, IDD_DECLARATION_OF_INTENT_DELETEFactoryComponent,
            IDD_DECLARATION_OF_INTENTComponent, IDD_DECLARATION_OF_INTENTFactoryComponent,
    ],
    exports: [
            IDD_TRAVELAGENCY_TAXFactoryComponent,
            IDD_DATA_PLAFOND_TAXFactoryComponent,
            IDD_PRORATAFactoryComponent,
            IDD_MARGIN_TAXFactoryComponent,
            IDD_DECLARATION_OF_INTENT_NUMBERSFactoryComponent,
            IDD_DECLARATION_OF_INTENT_DELETEFactoryComponent,
            IDD_DECLARATION_OF_INTENTFactoryComponent,
    ],
    entryComponents: [
            IDD_TRAVELAGENCY_TAXComponent,
            IDD_DATA_PLAFOND_TAXComponent,
            IDD_PRORATAComponent,
            IDD_MARGIN_TAXComponent,
            IDD_DECLARATION_OF_INTENT_NUMBERSComponent,
            IDD_DECLARATION_OF_INTENT_DELETEComponent,
            IDD_DECLARATION_OF_INTENTComponent,
    ]
})


export class SpecialTaxModule { };