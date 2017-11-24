import { IDD_TAXDOCUMENTSUPDATEComponent, IDD_TAXDOCUMENTSUPDATEFactoryComponent } from './taxdocumentsupdate/IDD_TAXDOCUMENTSUPDATE.component';
import { IDD_TAXDOCUMENTSSENDINGComponent, IDD_TAXDOCUMENTSSENDINGFactoryComponent } from './taxdocumentssending/IDD_TAXDOCUMENTSSENDING.component';
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
            { path: 'IDD_TAXDOCUMENTSSENDING', component: IDD_TAXDOCUMENTSSENDINGFactoryComponent },
        ])],
    declarations: [
            IDD_TAXDOCUMENTSUPDATEComponent, IDD_TAXDOCUMENTSUPDATEFactoryComponent,
            IDD_TAXDOCUMENTSSENDINGComponent, IDD_TAXDOCUMENTSSENDINGFactoryComponent,
    ],
    exports: [
            IDD_TAXDOCUMENTSUPDATEFactoryComponent,
            IDD_TAXDOCUMENTSSENDINGFactoryComponent,
    ],
    entryComponents: [
            IDD_TAXDOCUMENTSUPDATEComponent,
            IDD_TAXDOCUMENTSSENDINGComponent,
    ]
})


export class TaxDocuments_ITModule { };