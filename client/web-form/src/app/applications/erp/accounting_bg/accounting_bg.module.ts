import { IDD_TAXJOURNAL_DEKLARComponent, IDD_TAXJOURNAL_DEKLARFactoryComponent } from './taxjournalsanddeklar/IDD_TAXJOURNAL_DEKLAR.component';
import { IDD_VAT_DOCUMENT_TYPEComponent, IDD_VAT_DOCUMENT_TYPEFactoryComponent } from './taxdocumenttype/IDD_VAT_DOCUMENT_TYPE.component';
import { IDD_ACCTRANSFERTPLComponent, IDD_ACCTRANSFERTPLFactoryComponent } from './acctransfertpl/IDD_ACCTRANSFERTPL.component';
import { IDD_ACCTRANSFERComponent, IDD_ACCTRANSFERFactoryComponent } from './accountingtransfer/IDD_ACCTRANSFER.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TAXJOURNAL_DEKLAR', component: IDD_TAXJOURNAL_DEKLARFactoryComponent },
            { path: 'IDD_VAT_DOCUMENT_TYPE', component: IDD_VAT_DOCUMENT_TYPEFactoryComponent },
            { path: 'IDD_ACCTRANSFERTPL', component: IDD_ACCTRANSFERTPLFactoryComponent },
            { path: 'IDD_ACCTRANSFER', component: IDD_ACCTRANSFERFactoryComponent },
        ])],
    declarations: [
            IDD_TAXJOURNAL_DEKLARComponent, IDD_TAXJOURNAL_DEKLARFactoryComponent,
            IDD_VAT_DOCUMENT_TYPEComponent, IDD_VAT_DOCUMENT_TYPEFactoryComponent,
            IDD_ACCTRANSFERTPLComponent, IDD_ACCTRANSFERTPLFactoryComponent,
            IDD_ACCTRANSFERComponent, IDD_ACCTRANSFERFactoryComponent,
    ],
    exports: [
            IDD_TAXJOURNAL_DEKLARFactoryComponent,
            IDD_VAT_DOCUMENT_TYPEFactoryComponent,
            IDD_ACCTRANSFERTPLFactoryComponent,
            IDD_ACCTRANSFERFactoryComponent,
    ],
    entryComponents: [
            IDD_TAXJOURNAL_DEKLARComponent,
            IDD_VAT_DOCUMENT_TYPEComponent,
            IDD_ACCTRANSFERTPLComponent,
            IDD_ACCTRANSFERComponent,
    ]
})


export class Accounting_BGModule { };