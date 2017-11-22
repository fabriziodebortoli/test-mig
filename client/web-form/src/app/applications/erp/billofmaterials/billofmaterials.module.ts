import { IDD_LOAD_PRODPLANComponent, IDD_LOAD_PRODPLANFactoryComponent } from './searchproductionplan/IDD_LOAD_PRODPLAN.component';
import { IDD_REORDER_MATERIALSComponent, IDD_REORDER_MATERIALSFactoryComponent } from './reordermaterialstosupplier/IDD_REORDER_MATERIALS.component';
import { IDD_RECALCULATERESERVEDORDEREDComponent, IDD_RECALCULATERESERVEDORDEREDFactoryComponent } from './recalculatereservedordered/IDD_RECALCULATERESERVEDORDERED.component';
import { IDD_REASSIGNSUBIDComponent, IDD_REASSIGNSUBIDFactoryComponent } from './reassignsubid/IDD_REASSIGNSUBID.component';
import { IDD_PRODPLAN_GENERATIONComponent, IDD_PRODPLAN_GENERATIONFactoryComponent } from './productionplangeneration/IDD_PRODPLAN_GENERATION.component';
import { IDD_PRODPLAN_COPYComponent, IDD_PRODPLAN_COPYFactoryComponent } from './productionplan/IDD_PRODPLAN_COPY.component';
import { IDD_PRODPLANComponent, IDD_PRODPLANFactoryComponent } from './productionplan/IDD_PRODPLAN.component';
import { IDD_PRODUCIBILITY_ANALYSISComponent, IDD_PRODUCIBILITY_ANALYSISFactoryComponent } from './producibilityanalysis/IDD_PRODUCIBILITY_ANALYSIS.component';
import { IDD_ORDERED_PROD_CALCULATIONComponent, IDD_ORDERED_PROD_CALCULATIONFactoryComponent } from './orderedprodcalculation/IDD_ORDERED_PROD_CALCULATION.component';
import { IDD_DELETE_SIMBOMCOSTComponent, IDD_DELETE_SIMBOMCOSTFactoryComponent } from './bomsimulationcostdeletion/IDD_DELETE_SIMBOMCOST.component';
import { IDD_BOM_PROD_DATAComponent, IDD_BOM_PROD_DATAFactoryComponent } from './bomproduction/IDD_BOM_PROD_DATA.component';
import { IDD_PARAM_BILLOFMATERIALSComponent, IDD_PARAM_BILLOFMATERIALSFactoryComponent } from './bomparameters/IDD_PARAM_BILLOFMATERIALS.component';
import { IDD_BOMGRAPH_FIND_COMPComponent, IDD_BOMGRAPH_FIND_COMPFactoryComponent } from './bomnavigation/IDD_BOMGRAPH_FIND_COMP.component';
import { IDD_BOMGRAPHComponent, IDD_BOMGRAPHFactoryComponent } from './bomnavigation/IDD_BOMGRAPH.component';
import { IDD_LOAD_BOMComponent, IDD_LOAD_BOMFactoryComponent } from './bomloading/IDD_LOAD_BOM.component';
import { IDD_BOMIMPLOSIONComponent, IDD_BOMIMPLOSIONFactoryComponent } from './bomimplosion/IDD_BOMIMPLOSION.component';
import { IDD_BOMESPLOSIONComponent, IDD_BOMESPLOSIONFactoryComponent } from './bomexplosion/IDD_BOMESPLOSION.component';
import { IDD_BOMCOSTSIMULATIONSComponent, IDD_BOMCOSTSIMULATIONSFactoryComponent } from './bomcostsimulations/IDD_BOMCOSTSIMULATIONS.component';
import { IDD_COSTINGComponent, IDD_COSTINGFactoryComponent } from './bomcosting/IDD_COSTING.component';
import { IDD_BOM_CHECKDATEComponent, IDD_BOM_CHECKDATEFactoryComponent } from './bomcomponentsvaliditydate/IDD_BOM_CHECKDATE.component';
import { IDD_SUBSBOMITEMComponent, IDD_SUBSBOMITEMFactoryComponent } from './bomcomponentsreplacement/IDD_SUBSBOMITEM.component';
import { IDD_BOM_COPYComponent, IDD_BOM_COPYFactoryComponent } from './billofmaterials/IDD_BOM_COPY.component';
import { IDD_BOM_ADD_REFComponent, IDD_BOM_ADD_REFFactoryComponent } from './billofmaterials/IDD_BOM_ADD_REF.component';
import { IDD_BOMComponent, IDD_BOMFactoryComponent } from './billofmaterials/IDD_BOM.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_LOAD_PRODPLAN', component: IDD_LOAD_PRODPLANFactoryComponent },
            { path: 'IDD_REORDER_MATERIALS', component: IDD_REORDER_MATERIALSFactoryComponent },
            { path: 'IDD_RECALCULATERESERVEDORDERED', component: IDD_RECALCULATERESERVEDORDEREDFactoryComponent },
            { path: 'IDD_REASSIGNSUBID', component: IDD_REASSIGNSUBIDFactoryComponent },
            { path: 'IDD_PRODPLAN_GENERATION', component: IDD_PRODPLAN_GENERATIONFactoryComponent },
            { path: 'IDD_PRODPLAN_COPY', component: IDD_PRODPLAN_COPYFactoryComponent },
            { path: 'IDD_PRODPLAN', component: IDD_PRODPLANFactoryComponent },
            { path: 'IDD_PRODUCIBILITY_ANALYSIS', component: IDD_PRODUCIBILITY_ANALYSISFactoryComponent },
            { path: 'IDD_ORDERED_PROD_CALCULATION', component: IDD_ORDERED_PROD_CALCULATIONFactoryComponent },
            { path: 'IDD_DELETE_SIMBOMCOST', component: IDD_DELETE_SIMBOMCOSTFactoryComponent },
            { path: 'IDD_BOM_PROD_DATA', component: IDD_BOM_PROD_DATAFactoryComponent },
            { path: 'IDD_PARAM_BILLOFMATERIALS', component: IDD_PARAM_BILLOFMATERIALSFactoryComponent },
            { path: 'IDD_BOMGRAPH_FIND_COMP', component: IDD_BOMGRAPH_FIND_COMPFactoryComponent },
            { path: 'IDD_BOMGRAPH', component: IDD_BOMGRAPHFactoryComponent },
            { path: 'IDD_LOAD_BOM', component: IDD_LOAD_BOMFactoryComponent },
            { path: 'IDD_BOMIMPLOSION', component: IDD_BOMIMPLOSIONFactoryComponent },
            { path: 'IDD_BOMESPLOSION', component: IDD_BOMESPLOSIONFactoryComponent },
            { path: 'IDD_BOMCOSTSIMULATIONS', component: IDD_BOMCOSTSIMULATIONSFactoryComponent },
            { path: 'IDD_COSTING', component: IDD_COSTINGFactoryComponent },
            { path: 'IDD_BOM_CHECKDATE', component: IDD_BOM_CHECKDATEFactoryComponent },
            { path: 'IDD_SUBSBOMITEM', component: IDD_SUBSBOMITEMFactoryComponent },
            { path: 'IDD_BOM_COPY', component: IDD_BOM_COPYFactoryComponent },
            { path: 'IDD_BOM_ADD_REF', component: IDD_BOM_ADD_REFFactoryComponent },
            { path: 'IDD_BOM', component: IDD_BOMFactoryComponent },
        ])],
    declarations: [
            IDD_LOAD_PRODPLANComponent, IDD_LOAD_PRODPLANFactoryComponent,
            IDD_REORDER_MATERIALSComponent, IDD_REORDER_MATERIALSFactoryComponent,
            IDD_RECALCULATERESERVEDORDEREDComponent, IDD_RECALCULATERESERVEDORDEREDFactoryComponent,
            IDD_REASSIGNSUBIDComponent, IDD_REASSIGNSUBIDFactoryComponent,
            IDD_PRODPLAN_GENERATIONComponent, IDD_PRODPLAN_GENERATIONFactoryComponent,
            IDD_PRODPLAN_COPYComponent, IDD_PRODPLAN_COPYFactoryComponent,
            IDD_PRODPLANComponent, IDD_PRODPLANFactoryComponent,
            IDD_PRODUCIBILITY_ANALYSISComponent, IDD_PRODUCIBILITY_ANALYSISFactoryComponent,
            IDD_ORDERED_PROD_CALCULATIONComponent, IDD_ORDERED_PROD_CALCULATIONFactoryComponent,
            IDD_DELETE_SIMBOMCOSTComponent, IDD_DELETE_SIMBOMCOSTFactoryComponent,
            IDD_BOM_PROD_DATAComponent, IDD_BOM_PROD_DATAFactoryComponent,
            IDD_PARAM_BILLOFMATERIALSComponent, IDD_PARAM_BILLOFMATERIALSFactoryComponent,
            IDD_BOMGRAPH_FIND_COMPComponent, IDD_BOMGRAPH_FIND_COMPFactoryComponent,
            IDD_BOMGRAPHComponent, IDD_BOMGRAPHFactoryComponent,
            IDD_LOAD_BOMComponent, IDD_LOAD_BOMFactoryComponent,
            IDD_BOMIMPLOSIONComponent, IDD_BOMIMPLOSIONFactoryComponent,
            IDD_BOMESPLOSIONComponent, IDD_BOMESPLOSIONFactoryComponent,
            IDD_BOMCOSTSIMULATIONSComponent, IDD_BOMCOSTSIMULATIONSFactoryComponent,
            IDD_COSTINGComponent, IDD_COSTINGFactoryComponent,
            IDD_BOM_CHECKDATEComponent, IDD_BOM_CHECKDATEFactoryComponent,
            IDD_SUBSBOMITEMComponent, IDD_SUBSBOMITEMFactoryComponent,
            IDD_BOM_COPYComponent, IDD_BOM_COPYFactoryComponent,
            IDD_BOM_ADD_REFComponent, IDD_BOM_ADD_REFFactoryComponent,
            IDD_BOMComponent, IDD_BOMFactoryComponent,
    ],
    exports: [
            IDD_LOAD_PRODPLANFactoryComponent,
            IDD_REORDER_MATERIALSFactoryComponent,
            IDD_RECALCULATERESERVEDORDEREDFactoryComponent,
            IDD_REASSIGNSUBIDFactoryComponent,
            IDD_PRODPLAN_GENERATIONFactoryComponent,
            IDD_PRODPLAN_COPYFactoryComponent,
            IDD_PRODPLANFactoryComponent,
            IDD_PRODUCIBILITY_ANALYSISFactoryComponent,
            IDD_ORDERED_PROD_CALCULATIONFactoryComponent,
            IDD_DELETE_SIMBOMCOSTFactoryComponent,
            IDD_BOM_PROD_DATAFactoryComponent,
            IDD_PARAM_BILLOFMATERIALSFactoryComponent,
            IDD_BOMGRAPH_FIND_COMPFactoryComponent,
            IDD_BOMGRAPHFactoryComponent,
            IDD_LOAD_BOMFactoryComponent,
            IDD_BOMIMPLOSIONFactoryComponent,
            IDD_BOMESPLOSIONFactoryComponent,
            IDD_BOMCOSTSIMULATIONSFactoryComponent,
            IDD_COSTINGFactoryComponent,
            IDD_BOM_CHECKDATEFactoryComponent,
            IDD_SUBSBOMITEMFactoryComponent,
            IDD_BOM_COPYFactoryComponent,
            IDD_BOM_ADD_REFFactoryComponent,
            IDD_BOMFactoryComponent,
    ],
    entryComponents: [
            IDD_LOAD_PRODPLANComponent,
            IDD_REORDER_MATERIALSComponent,
            IDD_RECALCULATERESERVEDORDEREDComponent,
            IDD_REASSIGNSUBIDComponent,
            IDD_PRODPLAN_GENERATIONComponent,
            IDD_PRODPLAN_COPYComponent,
            IDD_PRODPLANComponent,
            IDD_PRODUCIBILITY_ANALYSISComponent,
            IDD_ORDERED_PROD_CALCULATIONComponent,
            IDD_DELETE_SIMBOMCOSTComponent,
            IDD_BOM_PROD_DATAComponent,
            IDD_PARAM_BILLOFMATERIALSComponent,
            IDD_BOMGRAPH_FIND_COMPComponent,
            IDD_BOMGRAPHComponent,
            IDD_LOAD_BOMComponent,
            IDD_BOMIMPLOSIONComponent,
            IDD_BOMESPLOSIONComponent,
            IDD_BOMCOSTSIMULATIONSComponent,
            IDD_COSTINGComponent,
            IDD_BOM_CHECKDATEComponent,
            IDD_SUBSBOMITEMComponent,
            IDD_BOM_COPYComponent,
            IDD_BOM_ADD_REFComponent,
            IDD_BOMComponent,
    ]
})


export class BillOfMaterialsModule { };