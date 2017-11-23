import { IDD_TAXDOCUMENTSUPDATEComponent, IDD_TAXDOCUMENTSUPDATEFactoryComponent } from './taxdocumentsupdate/IDD_TAXDOCUMENTSUPDATE.component';
import { IDD_TAXDOCUMENTSSETUPComponent, IDD_TAXDOCUMENTSSETUPFactoryComponent } from './taxdocumentssetup/IDD_TAXDOCUMENTSSETUP.component';
import { IDD_TAXDOCUMENTSSENDINGComponent, IDD_TAXDOCUMENTSSENDINGFactoryComponent } from './taxdocumentssending/IDD_TAXDOCUMENTSSENDING.component';
import { IDD_TAXDOCSENDINGSComponent, IDD_TAXDOCSENDINGSFactoryComponent } from './taxdocsendings/IDD_TAXDOCSENDINGS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TAXDOCUMENTSUPDATE', component: IDD_TAXDOCUMENTSUPDATEFactoryComponent },
            { path: 'IDD_TAXDOCUMENTSSETUP', component: IDD_TAXDOCUMENTSSETUPFactoryComponent },
            { path: 'IDD_TAXDOCUMENTSSENDING', component: IDD_TAXDOCUMENTSSENDINGFactoryComponent },
            { path: 'IDD_TAXDOCSENDINGS', component: IDD_TAXDOCSENDINGSFactoryComponent },
        ])],
    declarations: [
            IDD_TAXDOCUMENTSUPDATEComponent, IDD_TAXDOCUMENTSUPDATEFactoryComponent,
            IDD_TAXDOCUMENTSSETUPComponent, IDD_TAXDOCUMENTSSETUPFactoryComponent,
            IDD_TAXDOCUMENTSSENDINGComponent, IDD_TAXDOCUMENTSSENDINGFactoryComponent,
            IDD_TAXDOCSENDINGSComponent, IDD_TAXDOCSENDINGSFactoryComponent,
    ],
    exports: [
            IDD_TAXDOCUMENTSUPDATEFactoryComponent,
            IDD_TAXDOCUMENTSSETUPFactoryComponent,
            IDD_TAXDOCUMENTSSENDINGFactoryComponent,
            IDD_TAXDOCSENDINGSFactoryComponent,
    ],
    entryComponents: [
            IDD_TAXDOCUMENTSUPDATEComponent,
            IDD_TAXDOCUMENTSSETUPComponent,
            IDD_TAXDOCUMENTSSENDINGComponent,
            IDD_TAXDOCSENDINGSComponent,
    ]
})


export class TaxDocuments_ITModule { };