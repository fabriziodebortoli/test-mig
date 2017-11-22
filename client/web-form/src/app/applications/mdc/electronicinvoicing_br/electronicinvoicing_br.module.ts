import { IDD_TD_BR_UNUSED_NUMBERSComponent, IDD_TD_BR_UNUSED_NUMBERSFactoryComponent } from './brunusednumbers/IDD_TD_BR_UNUSED_NUMBERS.component';
import { IDD_BR_DEL_UNUSED_NUMBERSComponent, IDD_BR_DEL_UNUSED_NUMBERSFactoryComponent } from './brdeletingunusednumbers/IDD_BR_DEL_UNUSED_NUMBERS.component';
import { IDD_BR_DELETING_NFEComponent, IDD_BR_DELETING_NFEFactoryComponent } from './brdeletingnfe/IDD_BR_DELETING_NFE.component';
import { IDD_TD_CONTINGENCY_FS_DAComponent, IDD_TD_CONTINGENCY_FS_DAFactoryComponent } from './brcontingencyfsda/IDD_TD_CONTINGENCY_FS_DA.component';
import { IDD_TD_BR_IMPORTNFEComponent, IDD_TD_BR_IMPORTNFEFactoryComponent } from './uibrimportnfe/IDD_TD_BR_IMPORTNFE.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TD_BR_UNUSED_NUMBERS', component: IDD_TD_BR_UNUSED_NUMBERSFactoryComponent },
            { path: 'IDD_BR_DEL_UNUSED_NUMBERS', component: IDD_BR_DEL_UNUSED_NUMBERSFactoryComponent },
            { path: 'IDD_BR_DELETING_NFE', component: IDD_BR_DELETING_NFEFactoryComponent },
            { path: 'IDD_TD_CONTINGENCY_FS_DA', component: IDD_TD_CONTINGENCY_FS_DAFactoryComponent },
            { path: 'IDD_TD_BR_IMPORTNFE', component: IDD_TD_BR_IMPORTNFEFactoryComponent },
        ])],
    declarations: [
            IDD_TD_BR_UNUSED_NUMBERSComponent, IDD_TD_BR_UNUSED_NUMBERSFactoryComponent,
            IDD_BR_DEL_UNUSED_NUMBERSComponent, IDD_BR_DEL_UNUSED_NUMBERSFactoryComponent,
            IDD_BR_DELETING_NFEComponent, IDD_BR_DELETING_NFEFactoryComponent,
            IDD_TD_CONTINGENCY_FS_DAComponent, IDD_TD_CONTINGENCY_FS_DAFactoryComponent,
            IDD_TD_BR_IMPORTNFEComponent, IDD_TD_BR_IMPORTNFEFactoryComponent,
    ],
    exports: [
            IDD_TD_BR_UNUSED_NUMBERSFactoryComponent,
            IDD_BR_DEL_UNUSED_NUMBERSFactoryComponent,
            IDD_BR_DELETING_NFEFactoryComponent,
            IDD_TD_CONTINGENCY_FS_DAFactoryComponent,
            IDD_TD_BR_IMPORTNFEFactoryComponent,
    ],
    entryComponents: [
            IDD_TD_BR_UNUSED_NUMBERSComponent,
            IDD_BR_DEL_UNUSED_NUMBERSComponent,
            IDD_BR_DELETING_NFEComponent,
            IDD_TD_CONTINGENCY_FS_DAComponent,
            IDD_TD_BR_IMPORTNFEComponent,
    ]
})


export class ElectronicInvoicing_BRModule { };