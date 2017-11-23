import { IDD_IMPORT_SCHEMA_RECLASSIFIED_COAComponent, IDD_IMPORT_SCHEMA_RECLASSIFIED_COAFactoryComponent } from './importreclassification/IDD_IMPORT_SCHEMA_RECLASSIFIED_COA.component';
import { IDD_BASELIIEXCELComponent, IDD_BASELIIEXCELFactoryComponent } from './baseliiexcel/IDD_BASELIIEXCEL.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_IMPORT_SCHEMA_RECLASSIFIED_COA', component: IDD_IMPORT_SCHEMA_RECLASSIFIED_COAFactoryComponent },
            { path: 'IDD_BASELIIEXCEL', component: IDD_BASELIIEXCELFactoryComponent },
        ])],
    declarations: [
            IDD_IMPORT_SCHEMA_RECLASSIFIED_COAComponent, IDD_IMPORT_SCHEMA_RECLASSIFIED_COAFactoryComponent,
            IDD_BASELIIEXCELComponent, IDD_BASELIIEXCELFactoryComponent,
    ],
    exports: [
            IDD_IMPORT_SCHEMA_RECLASSIFIED_COAFactoryComponent,
            IDD_BASELIIEXCELFactoryComponent,
    ],
    entryComponents: [
            IDD_IMPORT_SCHEMA_RECLASSIFIED_COAComponent,
            IDD_BASELIIEXCELComponent,
    ]
})


export class Basel_IIModule { };