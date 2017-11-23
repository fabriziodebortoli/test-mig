import { IDD_ECODELETIONComponent, IDD_ECODELETIONFactoryComponent } from './ecodeletion/IDD_ECODELETION.component';
import { IDD_ECOCONFIRMATIONComponent, IDD_ECOCONFIRMATIONFactoryComponent } from './ecoconfirmation/IDD_ECOCONFIRMATION.component';
import { IDD_ECO_REFERENCEComponent, IDD_ECO_REFERENCEFactoryComponent } from './eco/IDD_ECO_REFERENCE.component';
import { IDD_ECOComponent, IDD_ECOFactoryComponent } from './eco/IDD_ECO.component';
import { IDD_BOM_ECO_VARComponent, IDD_BOM_ECO_VARFactoryComponent } from './bomwitheco/IDD_BOM_ECO_VAR.component';
import { IDD_LOAD_ECOComponent, IDD_LOAD_ECOFactoryComponent } from './bomloadingforeco/IDD_LOAD_ECO.component';
import { IDD_PD_ECO_IMPORT_XMLComponent, IDD_PD_ECO_IMPORT_XMLFactoryComponent } from './uiimportxml/IDD_PD_ECO_IMPORT_XML.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_ECODELETION', component: IDD_ECODELETIONFactoryComponent },
            { path: 'IDD_ECOCONFIRMATION', component: IDD_ECOCONFIRMATIONFactoryComponent },
            { path: 'IDD_ECO_REFERENCE', component: IDD_ECO_REFERENCEFactoryComponent },
            { path: 'IDD_ECO', component: IDD_ECOFactoryComponent },
            { path: 'IDD_BOM_ECO_VAR', component: IDD_BOM_ECO_VARFactoryComponent },
            { path: 'IDD_LOAD_ECO', component: IDD_LOAD_ECOFactoryComponent },
            { path: 'IDD_PD_ECO_IMPORT_XML', component: IDD_PD_ECO_IMPORT_XMLFactoryComponent },
        ])],
    declarations: [
            IDD_ECODELETIONComponent, IDD_ECODELETIONFactoryComponent,
            IDD_ECOCONFIRMATIONComponent, IDD_ECOCONFIRMATIONFactoryComponent,
            IDD_ECO_REFERENCEComponent, IDD_ECO_REFERENCEFactoryComponent,
            IDD_ECOComponent, IDD_ECOFactoryComponent,
            IDD_BOM_ECO_VARComponent, IDD_BOM_ECO_VARFactoryComponent,
            IDD_LOAD_ECOComponent, IDD_LOAD_ECOFactoryComponent,
            IDD_PD_ECO_IMPORT_XMLComponent, IDD_PD_ECO_IMPORT_XMLFactoryComponent,
    ],
    exports: [
            IDD_ECODELETIONFactoryComponent,
            IDD_ECOCONFIRMATIONFactoryComponent,
            IDD_ECO_REFERENCEFactoryComponent,
            IDD_ECOFactoryComponent,
            IDD_BOM_ECO_VARFactoryComponent,
            IDD_LOAD_ECOFactoryComponent,
            IDD_PD_ECO_IMPORT_XMLFactoryComponent,
    ],
    entryComponents: [
            IDD_ECODELETIONComponent,
            IDD_ECOCONFIRMATIONComponent,
            IDD_ECO_REFERENCEComponent,
            IDD_ECOComponent,
            IDD_BOM_ECO_VARComponent,
            IDD_LOAD_ECOComponent,
            IDD_PD_ECO_IMPORT_XMLComponent,
    ]
})


export class BillOfMaterialsPlusModule { };