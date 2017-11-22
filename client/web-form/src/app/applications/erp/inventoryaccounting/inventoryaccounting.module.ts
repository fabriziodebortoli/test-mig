import { IDD_RETAIL_PRICE_CHANGEComponent, IDD_RETAIL_PRICE_CHANGEFactoryComponent } from './retailpricechange/IDD_RETAIL_PRICE_CHANGE.component';
import { IDD_INVACCPARAMSComponent, IDD_INVACCPARAMSFactoryComponent } from './invacctransparameters/IDD_INVACCPARAMS.component';
import { IDD_INVENTORY_ACC_DEFAULTSComponent, IDD_INVENTORY_ACC_DEFAULTSFactoryComponent } from './invaccdefaults/IDD_INVENTORY_ACC_DEFAULTS.component';
import { IDD_EXTACCTEMPLATE_COPYComponent, IDD_EXTACCTEMPLATE_COPYFactoryComponent } from './extaccountingtemplate/IDD_EXTACCTEMPLATE_COPY.component';
import { IDD_EXTACCTEMPLComponent, IDD_EXTACCTEMPLFactoryComponent } from './extaccountingtemplate/IDD_EXTACCTEMPL.component';
import { IDD_CHANGERETAILDATAComponent, IDD_CHANGERETAILDATAFactoryComponent } from './changeretaildata/IDD_CHANGERETAILDATA.component';
import { IDD_EXTACCFORMULAMNGComponent, IDD_EXTACCFORMULAMNGFactoryComponent } from './uiextaccountingformulamng/IDD_EXTACCFORMULAMNG.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_RETAIL_PRICE_CHANGE', component: IDD_RETAIL_PRICE_CHANGEFactoryComponent },
            { path: 'IDD_INVACCPARAMS', component: IDD_INVACCPARAMSFactoryComponent },
            { path: 'IDD_INVENTORY_ACC_DEFAULTS', component: IDD_INVENTORY_ACC_DEFAULTSFactoryComponent },
            { path: 'IDD_EXTACCTEMPLATE_COPY', component: IDD_EXTACCTEMPLATE_COPYFactoryComponent },
            { path: 'IDD_EXTACCTEMPL', component: IDD_EXTACCTEMPLFactoryComponent },
            { path: 'IDD_CHANGERETAILDATA', component: IDD_CHANGERETAILDATAFactoryComponent },
            { path: 'IDD_EXTACCFORMULAMNG', component: IDD_EXTACCFORMULAMNGFactoryComponent },
        ])],
    declarations: [
            IDD_RETAIL_PRICE_CHANGEComponent, IDD_RETAIL_PRICE_CHANGEFactoryComponent,
            IDD_INVACCPARAMSComponent, IDD_INVACCPARAMSFactoryComponent,
            IDD_INVENTORY_ACC_DEFAULTSComponent, IDD_INVENTORY_ACC_DEFAULTSFactoryComponent,
            IDD_EXTACCTEMPLATE_COPYComponent, IDD_EXTACCTEMPLATE_COPYFactoryComponent,
            IDD_EXTACCTEMPLComponent, IDD_EXTACCTEMPLFactoryComponent,
            IDD_CHANGERETAILDATAComponent, IDD_CHANGERETAILDATAFactoryComponent,
            IDD_EXTACCFORMULAMNGComponent, IDD_EXTACCFORMULAMNGFactoryComponent,
    ],
    exports: [
            IDD_RETAIL_PRICE_CHANGEFactoryComponent,
            IDD_INVACCPARAMSFactoryComponent,
            IDD_INVENTORY_ACC_DEFAULTSFactoryComponent,
            IDD_EXTACCTEMPLATE_COPYFactoryComponent,
            IDD_EXTACCTEMPLFactoryComponent,
            IDD_CHANGERETAILDATAFactoryComponent,
            IDD_EXTACCFORMULAMNGFactoryComponent,
    ],
    entryComponents: [
            IDD_RETAIL_PRICE_CHANGEComponent,
            IDD_INVACCPARAMSComponent,
            IDD_INVENTORY_ACC_DEFAULTSComponent,
            IDD_EXTACCTEMPLATE_COPYComponent,
            IDD_EXTACCTEMPLComponent,
            IDD_CHANGERETAILDATAComponent,
            IDD_EXTACCFORMULAMNGComponent,
    ]
})


export class InventoryAccountingModule { };