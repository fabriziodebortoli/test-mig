import { IDD_MAN_TESTComponent, IDD_MAN_TESTFactoryComponent } from './test/IDD_MAN_TEST.component';
import { IDD_PRINTER_DOCComponent, IDD_PRINTER_DOCFactoryComponent } from './shoppapersprint/IDD_PRINTER_DOC.component';
import { IDD_SHOPPAPERSDELComponent, IDD_SHOPPAPERSDELFactoryComponent } from './shoppapersdeleting/IDD_SHOPPAPERSDEL.component';
import { IDD_MOMATERIALREQUIREMENTSComponent, IDD_MOMATERIALREQUIREMENTSFactoryComponent } from './reordermaterialstoproduction/IDD_MOMATERIALREQUIREMENTS.component';
import { IDD_PROD_RUNComponent, IDD_PROD_RUNFactoryComponent } from './productionrun/IDD_PROD_RUN.component';
import { IDD_PRODPLAN_SELECTIONS_ORD_CUSTComponent, IDD_PRODPLAN_SELECTIONS_ORD_CUSTFactoryComponent } from './productionplan/IDD_PRODPLAN_SELECTIONS_ORD_CUST.component';
import { IDD_MODIFY_LOTSComponent, IDD_MODIFY_LOTSFactoryComponent } from './productionlotedit/IDD_MODIFY_LOTS.component';
import { IDD_PROD_DEV_ANALYSISComponent, IDD_PROD_DEV_ANALYSISFactoryComponent } from './productiondevelopment/IDD_PROD_DEV_ANALYSIS.component';
import { IDD_PROD_DEVComponent, IDD_PROD_DEVFactoryComponent } from './productiondevelopment/IDD_PROD_DEV.component';
import { IDD_PICK_MISSINGSComponent, IDD_PICK_MISSINGSFactoryComponent } from './pickingmissingmaterials/IDD_PICK_MISSINGS.component';
import { IDD_PL_IMPORT_XMLComponent, IDD_PL_IMPORT_XMLFactoryComponent } from './pickinglist/IDD_PL_IMPORT_XML.component';
import { IDD_PICKING_MATERIALS_QTY_CLOSUREComponent, IDD_PICKING_MATERIALS_QTY_CLOSUREFactoryComponent } from './pickinglist/IDD_PICKING_MATERIALS_QTY_CLOSURE.component';
import { IDD_PICKING_MATERIALSComponent, IDD_PICKING_MATERIALSFactoryComponent } from './pickinglist/IDD_PICKING_MATERIALS.component';
import { IDD_PD_PICKING_MATERIALS_GO_TO_DETAILComponent, IDD_PD_PICKING_MATERIALS_GO_TO_DETAILFactoryComponent } from './pickinglist/IDD_PD_PICKING_MATERIALS_GO_TO_DETAIL.component';
import { IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENTComponent, IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENTFactoryComponent } from './pickinglist/IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENT.component';
import { IDD_ROLLBACKComponent, IDD_ROLLBACKFactoryComponent } from './morollback/IDD_ROLLBACK.component';
import { IDD_MODIFY_PROCESSINGComponent, IDD_MODIFY_PROCESSINGFactoryComponent } from './momodification/IDD_MODIFY_PROCESSING.component';
import { IDD_MO_MAINTENANCEComponent, IDD_MO_MAINTENANCEFactoryComponent } from './momaintenance/IDD_MO_MAINTENANCE.component';
import { IDD_UNLOAD_IEComponent, IDD_UNLOAD_IEFactoryComponent } from './moconfirmationunloadie/IDD_UNLOAD_IE.component';
import { IDD_MOCONFIRMATION_LIST_BOLComponent, IDD_MOCONFIRMATION_LIST_BOLFactoryComponent } from './moconfirmationbollist/IDD_MOCONFIRMATION_LIST_BOL.component';
import { IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATEComponent, IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATEFactoryComponent } from './moconfirmation/IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATE.component';
import { IDD_MOCONFIRMATIONComponent, IDD_MOCONFIRMATIONFactoryComponent } from './moconfirmation/IDD_MOCONFIRMATION.component';
import { IDD_MOCONFIRM_IMPORT_XMLComponent, IDD_MOCONFIRM_IMPORT_XMLFactoryComponent } from './moconfirmation/IDD_MOCONFIRM_IMPORT_XML.component';
import { IDD_SUBST_JOLLY_COMPONENTSComponent, IDD_SUBST_JOLLY_COMPONENTSFactoryComponent } from './mocomponentsreplacement/IDD_SUBST_JOLLY_COMPONENTS.component';
import { IDD_MO_COMP_REPLComponent, IDD_MO_COMP_REPLFactoryComponent } from './mocomponentsreplacement/IDD_MO_COMP_REPL.component';
import { IDD_MIGRATION_40Component, IDD_MIGRATION_40FactoryComponent } from './migrator40/IDD_MIGRATION_40.component';
import { IDD_GEN_NOPICKINGLISTSComponent, IDD_GEN_NOPICKINGLISTSFactoryComponent } from './materialrequirementspicking/IDD_GEN_NOPICKINGLISTS.component';
import { IDD_MANUFACT_PARAMETERSComponent, IDD_MANUFACT_PARAMETERSFactoryComponent } from './manufacturingparameters/IDD_MANUFACT_PARAMETERS.component';
import { IDD_INPROC_RAW_MATComponent, IDD_INPROC_RAW_MATFactoryComponent } from './inprocessingrawmaterials/IDD_INPROC_RAW_MAT.component';
import { IDD_CALC_PROD_LTComponent, IDD_CALC_PROD_LTFactoryComponent } from './calculateproductionleadtime/IDD_CALC_PROD_LT.component';
import { IDD_POSTING_DELETIONComponent, IDD_POSTING_DELETIONFactoryComponent } from './bompostingdeletion/IDD_POSTING_DELETION.component';
import { IDD_BOM_POSTINGComponent, IDD_BOM_POSTINGFactoryComponent } from './bomposting/IDD_BOM_POSTING.component';
import { IDD_BOM_MO_COMPComponent, IDD_BOM_MO_COMPFactoryComponent } from './bom_mocomparison/IDD_BOM_MO_COMP.component';
import { IDD_ACTUAL_COSTS_CALCComponent, IDD_ACTUAL_COSTS_CALCFactoryComponent } from './actualcosts/IDD_ACTUAL_COSTS_CALC.component';
import { IDD_MAN_ORD_MRPComponent, IDD_MAN_ORD_MRPFactoryComponent } from './uimo/IDD_MAN_ORD_MRP.component';
import { IDD_MAN_ORDComponent, IDD_MAN_ORDFactoryComponent } from './uimo/IDD_MAN_ORD.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_MAN_TEST', component: IDD_MAN_TESTFactoryComponent },
            { path: 'IDD_PRINTER_DOC', component: IDD_PRINTER_DOCFactoryComponent },
            { path: 'IDD_SHOPPAPERSDEL', component: IDD_SHOPPAPERSDELFactoryComponent },
            { path: 'IDD_MOMATERIALREQUIREMENTS', component: IDD_MOMATERIALREQUIREMENTSFactoryComponent },
            { path: 'IDD_PROD_RUN', component: IDD_PROD_RUNFactoryComponent },
            { path: 'IDD_PRODPLAN_SELECTIONS_ORD_CUST', component: IDD_PRODPLAN_SELECTIONS_ORD_CUSTFactoryComponent },
            { path: 'IDD_MODIFY_LOTS', component: IDD_MODIFY_LOTSFactoryComponent },
            { path: 'IDD_PROD_DEV_ANALYSIS', component: IDD_PROD_DEV_ANALYSISFactoryComponent },
            { path: 'IDD_PROD_DEV', component: IDD_PROD_DEVFactoryComponent },
            { path: 'IDD_PICK_MISSINGS', component: IDD_PICK_MISSINGSFactoryComponent },
            { path: 'IDD_PL_IMPORT_XML', component: IDD_PL_IMPORT_XMLFactoryComponent },
            { path: 'IDD_PICKING_MATERIALS_QTY_CLOSURE', component: IDD_PICKING_MATERIALS_QTY_CLOSUREFactoryComponent },
            { path: 'IDD_PICKING_MATERIALS', component: IDD_PICKING_MATERIALSFactoryComponent },
            { path: 'IDD_PD_PICKING_MATERIALS_GO_TO_DETAIL', component: IDD_PD_PICKING_MATERIALS_GO_TO_DETAILFactoryComponent },
            { path: 'IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENT', component: IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENTFactoryComponent },
            { path: 'IDD_ROLLBACK', component: IDD_ROLLBACKFactoryComponent },
            { path: 'IDD_MODIFY_PROCESSING', component: IDD_MODIFY_PROCESSINGFactoryComponent },
            { path: 'IDD_MO_MAINTENANCE', component: IDD_MO_MAINTENANCEFactoryComponent },
            { path: 'IDD_UNLOAD_IE', component: IDD_UNLOAD_IEFactoryComponent },
            { path: 'IDD_MOCONFIRMATION_LIST_BOL', component: IDD_MOCONFIRMATION_LIST_BOLFactoryComponent },
            { path: 'IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATE', component: IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATEFactoryComponent },
            { path: 'IDD_MOCONFIRMATION', component: IDD_MOCONFIRMATIONFactoryComponent },
            { path: 'IDD_MOCONFIRM_IMPORT_XML', component: IDD_MOCONFIRM_IMPORT_XMLFactoryComponent },
            { path: 'IDD_SUBST_JOLLY_COMPONENTS', component: IDD_SUBST_JOLLY_COMPONENTSFactoryComponent },
            { path: 'IDD_MO_COMP_REPL', component: IDD_MO_COMP_REPLFactoryComponent },
            { path: 'IDD_MIGRATION_40', component: IDD_MIGRATION_40FactoryComponent },
            { path: 'IDD_GEN_NOPICKINGLISTS', component: IDD_GEN_NOPICKINGLISTSFactoryComponent },
            { path: 'IDD_MANUFACT_PARAMETERS', component: IDD_MANUFACT_PARAMETERSFactoryComponent },
            { path: 'IDD_INPROC_RAW_MAT', component: IDD_INPROC_RAW_MATFactoryComponent },
            { path: 'IDD_CALC_PROD_LT', component: IDD_CALC_PROD_LTFactoryComponent },
            { path: 'IDD_POSTING_DELETION', component: IDD_POSTING_DELETIONFactoryComponent },
            { path: 'IDD_BOM_POSTING', component: IDD_BOM_POSTINGFactoryComponent },
            { path: 'IDD_BOM_MO_COMP', component: IDD_BOM_MO_COMPFactoryComponent },
            { path: 'IDD_ACTUAL_COSTS_CALC', component: IDD_ACTUAL_COSTS_CALCFactoryComponent },
            { path: 'IDD_MAN_ORD_MRP', component: IDD_MAN_ORD_MRPFactoryComponent },
            { path: 'IDD_MAN_ORD', component: IDD_MAN_ORDFactoryComponent },
        ])],
    declarations: [
            IDD_MAN_TESTComponent, IDD_MAN_TESTFactoryComponent,
            IDD_PRINTER_DOCComponent, IDD_PRINTER_DOCFactoryComponent,
            IDD_SHOPPAPERSDELComponent, IDD_SHOPPAPERSDELFactoryComponent,
            IDD_MOMATERIALREQUIREMENTSComponent, IDD_MOMATERIALREQUIREMENTSFactoryComponent,
            IDD_PROD_RUNComponent, IDD_PROD_RUNFactoryComponent,
            IDD_PRODPLAN_SELECTIONS_ORD_CUSTComponent, IDD_PRODPLAN_SELECTIONS_ORD_CUSTFactoryComponent,
            IDD_MODIFY_LOTSComponent, IDD_MODIFY_LOTSFactoryComponent,
            IDD_PROD_DEV_ANALYSISComponent, IDD_PROD_DEV_ANALYSISFactoryComponent,
            IDD_PROD_DEVComponent, IDD_PROD_DEVFactoryComponent,
            IDD_PICK_MISSINGSComponent, IDD_PICK_MISSINGSFactoryComponent,
            IDD_PL_IMPORT_XMLComponent, IDD_PL_IMPORT_XMLFactoryComponent,
            IDD_PICKING_MATERIALS_QTY_CLOSUREComponent, IDD_PICKING_MATERIALS_QTY_CLOSUREFactoryComponent,
            IDD_PICKING_MATERIALSComponent, IDD_PICKING_MATERIALSFactoryComponent,
            IDD_PD_PICKING_MATERIALS_GO_TO_DETAILComponent, IDD_PD_PICKING_MATERIALS_GO_TO_DETAILFactoryComponent,
            IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENTComponent, IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENTFactoryComponent,
            IDD_ROLLBACKComponent, IDD_ROLLBACKFactoryComponent,
            IDD_MODIFY_PROCESSINGComponent, IDD_MODIFY_PROCESSINGFactoryComponent,
            IDD_MO_MAINTENANCEComponent, IDD_MO_MAINTENANCEFactoryComponent,
            IDD_UNLOAD_IEComponent, IDD_UNLOAD_IEFactoryComponent,
            IDD_MOCONFIRMATION_LIST_BOLComponent, IDD_MOCONFIRMATION_LIST_BOLFactoryComponent,
            IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATEComponent, IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATEFactoryComponent,
            IDD_MOCONFIRMATIONComponent, IDD_MOCONFIRMATIONFactoryComponent,
            IDD_MOCONFIRM_IMPORT_XMLComponent, IDD_MOCONFIRM_IMPORT_XMLFactoryComponent,
            IDD_SUBST_JOLLY_COMPONENTSComponent, IDD_SUBST_JOLLY_COMPONENTSFactoryComponent,
            IDD_MO_COMP_REPLComponent, IDD_MO_COMP_REPLFactoryComponent,
            IDD_MIGRATION_40Component, IDD_MIGRATION_40FactoryComponent,
            IDD_GEN_NOPICKINGLISTSComponent, IDD_GEN_NOPICKINGLISTSFactoryComponent,
            IDD_MANUFACT_PARAMETERSComponent, IDD_MANUFACT_PARAMETERSFactoryComponent,
            IDD_INPROC_RAW_MATComponent, IDD_INPROC_RAW_MATFactoryComponent,
            IDD_CALC_PROD_LTComponent, IDD_CALC_PROD_LTFactoryComponent,
            IDD_POSTING_DELETIONComponent, IDD_POSTING_DELETIONFactoryComponent,
            IDD_BOM_POSTINGComponent, IDD_BOM_POSTINGFactoryComponent,
            IDD_BOM_MO_COMPComponent, IDD_BOM_MO_COMPFactoryComponent,
            IDD_ACTUAL_COSTS_CALCComponent, IDD_ACTUAL_COSTS_CALCFactoryComponent,
            IDD_MAN_ORD_MRPComponent, IDD_MAN_ORD_MRPFactoryComponent,
            IDD_MAN_ORDComponent, IDD_MAN_ORDFactoryComponent,
    ],
    exports: [
            IDD_MAN_TESTFactoryComponent,
            IDD_PRINTER_DOCFactoryComponent,
            IDD_SHOPPAPERSDELFactoryComponent,
            IDD_MOMATERIALREQUIREMENTSFactoryComponent,
            IDD_PROD_RUNFactoryComponent,
            IDD_PRODPLAN_SELECTIONS_ORD_CUSTFactoryComponent,
            IDD_MODIFY_LOTSFactoryComponent,
            IDD_PROD_DEV_ANALYSISFactoryComponent,
            IDD_PROD_DEVFactoryComponent,
            IDD_PICK_MISSINGSFactoryComponent,
            IDD_PL_IMPORT_XMLFactoryComponent,
            IDD_PICKING_MATERIALS_QTY_CLOSUREFactoryComponent,
            IDD_PICKING_MATERIALSFactoryComponent,
            IDD_PD_PICKING_MATERIALS_GO_TO_DETAILFactoryComponent,
            IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENTFactoryComponent,
            IDD_ROLLBACKFactoryComponent,
            IDD_MODIFY_PROCESSINGFactoryComponent,
            IDD_MO_MAINTENANCEFactoryComponent,
            IDD_UNLOAD_IEFactoryComponent,
            IDD_MOCONFIRMATION_LIST_BOLFactoryComponent,
            IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATEFactoryComponent,
            IDD_MOCONFIRMATIONFactoryComponent,
            IDD_MOCONFIRM_IMPORT_XMLFactoryComponent,
            IDD_SUBST_JOLLY_COMPONENTSFactoryComponent,
            IDD_MO_COMP_REPLFactoryComponent,
            IDD_MIGRATION_40FactoryComponent,
            IDD_GEN_NOPICKINGLISTSFactoryComponent,
            IDD_MANUFACT_PARAMETERSFactoryComponent,
            IDD_INPROC_RAW_MATFactoryComponent,
            IDD_CALC_PROD_LTFactoryComponent,
            IDD_POSTING_DELETIONFactoryComponent,
            IDD_BOM_POSTINGFactoryComponent,
            IDD_BOM_MO_COMPFactoryComponent,
            IDD_ACTUAL_COSTS_CALCFactoryComponent,
            IDD_MAN_ORD_MRPFactoryComponent,
            IDD_MAN_ORDFactoryComponent,
    ],
    entryComponents: [
            IDD_MAN_TESTComponent,
            IDD_PRINTER_DOCComponent,
            IDD_SHOPPAPERSDELComponent,
            IDD_MOMATERIALREQUIREMENTSComponent,
            IDD_PROD_RUNComponent,
            IDD_PRODPLAN_SELECTIONS_ORD_CUSTComponent,
            IDD_MODIFY_LOTSComponent,
            IDD_PROD_DEV_ANALYSISComponent,
            IDD_PROD_DEVComponent,
            IDD_PICK_MISSINGSComponent,
            IDD_PL_IMPORT_XMLComponent,
            IDD_PICKING_MATERIALS_QTY_CLOSUREComponent,
            IDD_PICKING_MATERIALSComponent,
            IDD_PD_PICKING_MATERIALS_GO_TO_DETAILComponent,
            IDD_PD_PICKING_MATERIALS_GO_TO_COMPONENTComponent,
            IDD_ROLLBACKComponent,
            IDD_MODIFY_PROCESSINGComponent,
            IDD_MO_MAINTENANCEComponent,
            IDD_UNLOAD_IEComponent,
            IDD_MOCONFIRMATION_LIST_BOLComponent,
            IDD_PD_MOCONFIRMATION_LIST_BOL_GENERATEComponent,
            IDD_MOCONFIRMATIONComponent,
            IDD_MOCONFIRM_IMPORT_XMLComponent,
            IDD_SUBST_JOLLY_COMPONENTSComponent,
            IDD_MO_COMP_REPLComponent,
            IDD_MIGRATION_40Component,
            IDD_GEN_NOPICKINGLISTSComponent,
            IDD_MANUFACT_PARAMETERSComponent,
            IDD_INPROC_RAW_MATComponent,
            IDD_CALC_PROD_LTComponent,
            IDD_POSTING_DELETIONComponent,
            IDD_BOM_POSTINGComponent,
            IDD_BOM_MO_COMPComponent,
            IDD_ACTUAL_COSTS_CALCComponent,
            IDD_MAN_ORD_MRPComponent,
            IDD_MAN_ORDComponent,
    ]
})


export class ManufacturingModule { };