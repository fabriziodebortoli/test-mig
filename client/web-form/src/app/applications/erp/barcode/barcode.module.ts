import { IDD_BARCODE_STRUCTURE_COPYComponent, IDD_BARCODE_STRUCTURE_COPYFactoryComponent } from './barcodestructurecopy/IDD_BARCODE_STRUCTURE_COPY.component';
import { IDD_BARCODE_PARAMETERSComponent, IDD_BARCODE_PARAMETERSFactoryComponent } from './barcodeparameters/IDD_BARCODE_PARAMETERS.component';
import { IDD_BARCODE_LABELComponent, IDD_BARCODE_LABELFactoryComponent } from './barcodelabel/IDD_BARCODE_LABEL.component';
import { IDD_BARCODESTRUCTUREComponent, IDD_BARCODESTRUCTUREFactoryComponent } from './uibarcodestructure/IDD_BARCODESTRUCTURE.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_BARCODE_STRUCTURE_COPY', component: IDD_BARCODE_STRUCTURE_COPYFactoryComponent },
            { path: 'IDD_BARCODE_PARAMETERS', component: IDD_BARCODE_PARAMETERSFactoryComponent },
            { path: 'IDD_BARCODE_LABEL', component: IDD_BARCODE_LABELFactoryComponent },
            { path: 'IDD_BARCODESTRUCTURE', component: IDD_BARCODESTRUCTUREFactoryComponent },
        ])],
    declarations: [
            IDD_BARCODE_STRUCTURE_COPYComponent, IDD_BARCODE_STRUCTURE_COPYFactoryComponent,
            IDD_BARCODE_PARAMETERSComponent, IDD_BARCODE_PARAMETERSFactoryComponent,
            IDD_BARCODE_LABELComponent, IDD_BARCODE_LABELFactoryComponent,
            IDD_BARCODESTRUCTUREComponent, IDD_BARCODESTRUCTUREFactoryComponent,
    ],
    exports: [
            IDD_BARCODE_STRUCTURE_COPYFactoryComponent,
            IDD_BARCODE_PARAMETERSFactoryComponent,
            IDD_BARCODE_LABELFactoryComponent,
            IDD_BARCODESTRUCTUREFactoryComponent,
    ],
    entryComponents: [
            IDD_BARCODE_STRUCTURE_COPYComponent,
            IDD_BARCODE_PARAMETERSComponent,
            IDD_BARCODE_LABELComponent,
            IDD_BARCODESTRUCTUREComponent,
    ]
})


export class BarcodeModule { };