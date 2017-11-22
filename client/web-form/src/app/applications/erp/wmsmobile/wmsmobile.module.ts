import { IDD_PARAMETERS_WMS_TOS_TRANSFERComponent, IDD_PARAMETERS_WMS_TOS_TRANSFERFactoryComponent } from './wtparametersinprogrset/IDD_PARAMETERS_WMS_TOS_TRANSFER.component';
import { IDD_WT_ON_BOARD_DB_CREATORComponent, IDD_WT_ON_BOARD_DB_CREATORFactoryComponent } from './wtonboarddbcreator/IDD_WT_ON_BOARD_DB_CREATOR.component';
import { IDD_TD_MONITOR_GRComponent, IDD_TD_MONITOR_GRFactoryComponent } from './wtmonitorgr/IDD_TD_MONITOR_GR.component';
import { IDD_WT_MNG_WIZARDComponent, IDD_WT_MNG_WIZARDFactoryComponent } from './wtmanagement/IDD_WT_MNG_WIZARD.component';
import { IDD_WEBSERVICESLISTComponent, IDD_WEBSERVICESLISTFactoryComponent } from './wmsmobilewebservices/IDD_WEBSERVICESLIST.component';
import { IDD_WT_PARAMETERSComponent, IDD_WT_PARAMETERSFactoryComponent } from './wmsmobileparameters/IDD_WT_PARAMETERS.component';
import { IDD_WMS_INVENTORYBIN_PLANNING_FRAMEComponent, IDD_WMS_INVENTORYBIN_PLANNING_FRAMEFactoryComponent } from './wmsinventorybinplanning/IDD_WMS_INVENTORYBIN_PLANNING_FRAME.component';
import { IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTIONComponent, IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTIONFactoryComponent } from './wmsinventorybinplanning/IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTION.component';
import { IDD_WMS_INVENTORYBIN_MANAGEMENT_FRAMEComponent, IDD_WMS_INVENTORYBIN_MANAGEMENT_FRAMEFactoryComponent } from './wmsinventorybinmanagement/IDD_WMS_INVENTORYBIN_MANAGEMENT_FRAME.component';
import { IDD_MOBILE_CONFIGURATIONComponent, IDD_MOBILE_CONFIGURATIONFactoryComponent } from './mobileconfiguration/IDD_MOBILE_CONFIGURATION.component';
import { IDD_INVENTRY_LOAD_UNLOADComponent, IDD_INVENTRY_LOAD_UNLOADFactoryComponent } from './inventryfromloadunloadto/IDD_INVENTRY_LOAD_UNLOAD.component';
import { IDD_CASH_AND_CARRY_IMPORT_ITEMSComponent, IDD_CASH_AND_CARRY_IMPORT_ITEMSFactoryComponent } from './cashandcarryimportitems/IDD_CASH_AND_CARRY_IMPORT_ITEMS.component';
import { IDD_WTMONITORComponent, IDD_WTMONITORFactoryComponent } from './uiwtmonitor/IDD_WTMONITOR.component';
import { IDD_WT_DELETE_PENDING_TOComponent, IDD_WT_DELETE_PENDING_TOFactoryComponent } from './uiwtmonitor/IDD_WT_DELETE_PENDING_TO.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_PARAMETERS_WMS_TOS_TRANSFER', component: IDD_PARAMETERS_WMS_TOS_TRANSFERFactoryComponent },
            { path: 'IDD_WT_ON_BOARD_DB_CREATOR', component: IDD_WT_ON_BOARD_DB_CREATORFactoryComponent },
            { path: 'IDD_TD_MONITOR_GR', component: IDD_TD_MONITOR_GRFactoryComponent },
            { path: 'IDD_WT_MNG_WIZARD', component: IDD_WT_MNG_WIZARDFactoryComponent },
            { path: 'IDD_WEBSERVICESLIST', component: IDD_WEBSERVICESLISTFactoryComponent },
            { path: 'IDD_WT_PARAMETERS', component: IDD_WT_PARAMETERSFactoryComponent },
            { path: 'IDD_WMS_INVENTORYBIN_PLANNING_FRAME', component: IDD_WMS_INVENTORYBIN_PLANNING_FRAMEFactoryComponent },
            { path: 'IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTION', component: IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTIONFactoryComponent },
            { path: 'IDD_WMS_INVENTORYBIN_MANAGEMENT_FRAME', component: IDD_WMS_INVENTORYBIN_MANAGEMENT_FRAMEFactoryComponent },
            { path: 'IDD_MOBILE_CONFIGURATION', component: IDD_MOBILE_CONFIGURATIONFactoryComponent },
            { path: 'IDD_INVENTRY_LOAD_UNLOAD', component: IDD_INVENTRY_LOAD_UNLOADFactoryComponent },
            { path: 'IDD_CASH_AND_CARRY_IMPORT_ITEMS', component: IDD_CASH_AND_CARRY_IMPORT_ITEMSFactoryComponent },
            { path: 'IDD_WTMONITOR', component: IDD_WTMONITORFactoryComponent },
            { path: 'IDD_WT_DELETE_PENDING_TO', component: IDD_WT_DELETE_PENDING_TOFactoryComponent },
        ])],
    declarations: [
            IDD_PARAMETERS_WMS_TOS_TRANSFERComponent, IDD_PARAMETERS_WMS_TOS_TRANSFERFactoryComponent,
            IDD_WT_ON_BOARD_DB_CREATORComponent, IDD_WT_ON_BOARD_DB_CREATORFactoryComponent,
            IDD_TD_MONITOR_GRComponent, IDD_TD_MONITOR_GRFactoryComponent,
            IDD_WT_MNG_WIZARDComponent, IDD_WT_MNG_WIZARDFactoryComponent,
            IDD_WEBSERVICESLISTComponent, IDD_WEBSERVICESLISTFactoryComponent,
            IDD_WT_PARAMETERSComponent, IDD_WT_PARAMETERSFactoryComponent,
            IDD_WMS_INVENTORYBIN_PLANNING_FRAMEComponent, IDD_WMS_INVENTORYBIN_PLANNING_FRAMEFactoryComponent,
            IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTIONComponent, IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTIONFactoryComponent,
            IDD_WMS_INVENTORYBIN_MANAGEMENT_FRAMEComponent, IDD_WMS_INVENTORYBIN_MANAGEMENT_FRAMEFactoryComponent,
            IDD_MOBILE_CONFIGURATIONComponent, IDD_MOBILE_CONFIGURATIONFactoryComponent,
            IDD_INVENTRY_LOAD_UNLOADComponent, IDD_INVENTRY_LOAD_UNLOADFactoryComponent,
            IDD_CASH_AND_CARRY_IMPORT_ITEMSComponent, IDD_CASH_AND_CARRY_IMPORT_ITEMSFactoryComponent,
            IDD_WTMONITORComponent, IDD_WTMONITORFactoryComponent,
            IDD_WT_DELETE_PENDING_TOComponent, IDD_WT_DELETE_PENDING_TOFactoryComponent,
    ],
    exports: [
            IDD_PARAMETERS_WMS_TOS_TRANSFERFactoryComponent,
            IDD_WT_ON_BOARD_DB_CREATORFactoryComponent,
            IDD_TD_MONITOR_GRFactoryComponent,
            IDD_WT_MNG_WIZARDFactoryComponent,
            IDD_WEBSERVICESLISTFactoryComponent,
            IDD_WT_PARAMETERSFactoryComponent,
            IDD_WMS_INVENTORYBIN_PLANNING_FRAMEFactoryComponent,
            IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTIONFactoryComponent,
            IDD_WMS_INVENTORYBIN_MANAGEMENT_FRAMEFactoryComponent,
            IDD_MOBILE_CONFIGURATIONFactoryComponent,
            IDD_INVENTRY_LOAD_UNLOADFactoryComponent,
            IDD_CASH_AND_CARRY_IMPORT_ITEMSFactoryComponent,
            IDD_WTMONITORFactoryComponent,
            IDD_WT_DELETE_PENDING_TOFactoryComponent,
    ],
    entryComponents: [
            IDD_PARAMETERS_WMS_TOS_TRANSFERComponent,
            IDD_WT_ON_BOARD_DB_CREATORComponent,
            IDD_TD_MONITOR_GRComponent,
            IDD_WT_MNG_WIZARDComponent,
            IDD_WEBSERVICESLISTComponent,
            IDD_WT_PARAMETERSComponent,
            IDD_WMS_INVENTORYBIN_PLANNING_FRAMEComponent,
            IDD_WMS_INVENTORYBIN_PLANNING_ASSIGNZONE_SELECTIONComponent,
            IDD_WMS_INVENTORYBIN_MANAGEMENT_FRAMEComponent,
            IDD_MOBILE_CONFIGURATIONComponent,
            IDD_INVENTRY_LOAD_UNLOADComponent,
            IDD_CASH_AND_CARRY_IMPORT_ITEMSComponent,
            IDD_WTMONITORComponent,
            IDD_WT_DELETE_PENDING_TOComponent,
    ]
})


export class WMSMobileModule { };