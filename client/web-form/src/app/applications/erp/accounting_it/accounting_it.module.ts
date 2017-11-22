import { IDD_TAX_COMMUNICATION_GROUPSComponent, IDD_TAX_COMMUNICATION_GROUPSFactoryComponent } from './taxcommunicationgroup/IDD_TAX_COMMUNICATION_GROUPS.component';
import { IDD_PRINT_ISSDECLINTENTComponent, IDD_PRINT_ISSDECLINTENTFactoryComponent } from './issueddeclarationofintentfile/IDD_PRINT_ISSDECLINTENT.component';
import { IDD_F24_ONLINEComponent, IDD_F24_ONLINEFactoryComponent } from './f24online/IDD_F24_ONLINE.component';
import { IDD_INTENTJOURNALComponent, IDD_INTENTJOURNALFactoryComponent } from './declarationintentjournal/IDD_INTENTJOURNAL.component';
import { IDD_BUILD_COMMUNICATIONComponent, IDD_BUILD_COMMUNICATIONFactoryComponent } from './buildtaxcommunication/IDD_BUILD_COMMUNICATION.component';
import { IDD_BLACKLIST2014Component, IDD_BLACKLIST2014FactoryComponent } from './blacklist2014/IDD_BLACKLIST2014.component';
import { IDD_ANNTAXComponent, IDD_ANNTAXFactoryComponent } from './annualtaxreporting/IDD_ANNTAX.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TAX_COMMUNICATION_GROUPS', component: IDD_TAX_COMMUNICATION_GROUPSFactoryComponent },
            { path: 'IDD_PRINT_ISSDECLINTENT', component: IDD_PRINT_ISSDECLINTENTFactoryComponent },
            { path: 'IDD_F24_ONLINE', component: IDD_F24_ONLINEFactoryComponent },
            { path: 'IDD_INTENTJOURNAL', component: IDD_INTENTJOURNALFactoryComponent },
            { path: 'IDD_BUILD_COMMUNICATION', component: IDD_BUILD_COMMUNICATIONFactoryComponent },
            { path: 'IDD_BLACKLIST2014', component: IDD_BLACKLIST2014FactoryComponent },
            { path: 'IDD_ANNTAX', component: IDD_ANNTAXFactoryComponent },
        ])],
    declarations: [
            IDD_TAX_COMMUNICATION_GROUPSComponent, IDD_TAX_COMMUNICATION_GROUPSFactoryComponent,
            IDD_PRINT_ISSDECLINTENTComponent, IDD_PRINT_ISSDECLINTENTFactoryComponent,
            IDD_F24_ONLINEComponent, IDD_F24_ONLINEFactoryComponent,
            IDD_INTENTJOURNALComponent, IDD_INTENTJOURNALFactoryComponent,
            IDD_BUILD_COMMUNICATIONComponent, IDD_BUILD_COMMUNICATIONFactoryComponent,
            IDD_BLACKLIST2014Component, IDD_BLACKLIST2014FactoryComponent,
            IDD_ANNTAXComponent, IDD_ANNTAXFactoryComponent,
    ],
    exports: [
            IDD_TAX_COMMUNICATION_GROUPSFactoryComponent,
            IDD_PRINT_ISSDECLINTENTFactoryComponent,
            IDD_F24_ONLINEFactoryComponent,
            IDD_INTENTJOURNALFactoryComponent,
            IDD_BUILD_COMMUNICATIONFactoryComponent,
            IDD_BLACKLIST2014FactoryComponent,
            IDD_ANNTAXFactoryComponent,
    ],
    entryComponents: [
            IDD_TAX_COMMUNICATION_GROUPSComponent,
            IDD_PRINT_ISSDECLINTENTComponent,
            IDD_F24_ONLINEComponent,
            IDD_INTENTJOURNALComponent,
            IDD_BUILD_COMMUNICATIONComponent,
            IDD_BLACKLIST2014Component,
            IDD_ANNTAXComponent,
    ]
})


export class Accounting_ITModule { };