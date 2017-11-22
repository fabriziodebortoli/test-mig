import { IDD_UOM_HEADERComponent, IDD_UOM_HEADERFactoryComponent } from './unitsofmeasure/IDD_UOM_HEADER.component';
import { IDD_TAX_CODE_ASSIGNComponent, IDD_TAX_CODE_ASSIGNFactoryComponent } from './taxcodeassignment/IDD_TAX_CODE_ASSIGN.component';
import { IDD_ITEM_STANDARDCOST_HISTORYComponent, IDD_ITEM_STANDARDCOST_HISTORYFactoryComponent } from './standardcosthistory/IDD_ITEM_STANDARDCOST_HISTORY.component';
import { IDD_CTG_PRODUCT_HEADERComponent, IDD_CTG_PRODUCT_HEADERFactoryComponent } from './productctg/IDD_CTG_PRODUCT_HEADER.component';
import { IDD_PRODUCERS_COMPLETEComponent, IDD_PRODUCERS_COMPLETEFactoryComponent } from './producers/IDD_PRODUCERS_COMPLETE.component';
import { IDD_ITMTYP_HEADERComponent, IDD_ITMTYP_HEADERFactoryComponent } from './itemtype/IDD_ITMTYP_HEADER.component';
import { IDD_ITEMS_SUBSTITUTEComponent, IDD_ITEMS_SUBSTITUTEFactoryComponent } from './itemssubstitute/IDD_ITEMS_SUBSTITUTE.component';
import { IDD_ITEM_SETTINGSComponent, IDD_ITEM_SETTINGSFactoryComponent } from './itemssettings/IDD_ITEM_SETTINGS.component';
import { IDD_RESEARCH_MAN_ITEMComponent, IDD_RESEARCH_MAN_ITEMFactoryComponent } from './itemssearchbyproducers/IDD_RESEARCH_MAN_ITEM.component';
import { IDD_ITEM_PARAMETERSComponent, IDD_ITEM_PARAMETERSFactoryComponent } from './itemsparameters/IDD_ITEM_PARAMETERS.component';
import { IDD_KIT_ITEM_COPYComponent, IDD_KIT_ITEM_COPYFactoryComponent } from './itemskit/IDD_KIT_ITEM_COPY.component';
import { IDD_KIT_ITEMComponent, IDD_KIT_ITEMFactoryComponent } from './itemskit/IDD_KIT_ITEM.component';
import { IDD_ITEMS_GRAPHIC_NAVIGComponent, IDD_ITEMS_GRAPHIC_NAVIGFactoryComponent } from './itemsgraphicnavigation/IDD_ITEMS_GRAPHIC_NAVIG.component';
import { IDD_ITEM_COPYComponent, IDD_ITEM_COPYFactoryComponent } from './itemscopy/IDD_ITEM_COPY.component';
import { IDD_ITEM_BUDGET_COPYComponent, IDD_ITEM_BUDGET_COPYFactoryComponent } from './itemscopy/IDD_ITEM_BUDGET_COPY.component';
import { IDD_ITEMS_SUMMARIZEDComponent, IDD_ITEMS_SUMMARIZEDFactoryComponent } from './items/IDD_ITEMS_SUMMARIZED.component';
import { IDD_ITEMSComponent, IDD_ITEMSFactoryComponent } from './items/IDD_ITEMS.component';
import { IDD_HOMOGCTGComponent, IDD_HOMOGCTGFactoryComponent } from './homogeneousctg/IDD_HOMOGCTG.component';
import { IDD_IMPORT_EXPORT_DETAIL_DOCUMENTComponent, IDD_IMPORT_EXPORT_DETAIL_DOCUMENTFactoryComponent } from './documentdetailimportexport/IDD_IMPORT_EXPORT_DETAIL_DOCUMENT.component';
import { IDD_DEPARTMENTS_FULLComponent, IDD_DEPARTMENTS_FULLFactoryComponent } from './departments/IDD_DEPARTMENTS_FULL.component';
import { IDD_CTGCOMMODITYComponent, IDD_CTGCOMMODITYFactoryComponent } from './commodityctg/IDD_CTGCOMMODITY.component';
import { IDD_ITEM_CHOOSE_IMPORTComponent, IDD_ITEM_CHOOSE_IMPORTFactoryComponent } from './bditemsmultiselectionforimport/IDD_ITEM_CHOOSE_IMPORT.component';
import { IDD_ACCOUNTING_TYPEComponent, IDD_ACCOUNTING_TYPEFactoryComponent } from './accountingtype/IDD_ACCOUNTING_TYPE.component';
import { IDD_SUPPCTG_COMMODITYCTG_FULLComponent, IDD_SUPPCTG_COMMODITYCTG_FULLFactoryComponent } from './uiitemsandcategoriescustsupp/IDD_SUPPCTG_COMMODITYCTG_FULL.component';
import { IDD_SUPP_ITEMTYPE_FULLComponent, IDD_SUPP_ITEMTYPE_FULLFactoryComponent } from './uiitemsandcategoriescustsupp/IDD_SUPP_ITEMTYPE_FULL.component';
import { IDD_SUPP_CTGCOMMODITY_FULLComponent, IDD_SUPP_CTGCOMMODITY_FULLFactoryComponent } from './uiitemsandcategoriescustsupp/IDD_SUPP_CTGCOMMODITY_FULL.component';
import { IDD_ITEMSUPP_FULLComponent, IDD_ITEMSUPP_FULLFactoryComponent } from './uiitemsandcategoriescustsupp/IDD_ITEMSUPP_FULL.component';
import { IDD_ITEMCUSTOMERS_FULLComponent, IDD_ITEMCUSTOMERS_FULLFactoryComponent } from './uiitemsandcategoriescustsupp/IDD_ITEMCUSTOMERS_FULL.component';
import { IDD_CUSTCTG_COMMODITYCTG_FULLComponent, IDD_CUSTCTG_COMMODITYCTG_FULLFactoryComponent } from './uiitemsandcategoriescustsupp/IDD_CUSTCTG_COMMODITYCTG_FULL.component';
import { IDD_CUST_ITEMTYPE_FULLComponent, IDD_CUST_ITEMTYPE_FULLFactoryComponent } from './uiitemsandcategoriescustsupp/IDD_CUST_ITEMTYPE_FULL.component';
import { IDD_CUST_CTGCOMMODITY_FULLComponent, IDD_CUST_CTGCOMMODITY_FULLFactoryComponent } from './uiitemsandcategoriescustsupp/IDD_CUST_CTGCOMMODITY_FULL.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_UOM_HEADER', component: IDD_UOM_HEADERFactoryComponent },
            { path: 'IDD_TAX_CODE_ASSIGN', component: IDD_TAX_CODE_ASSIGNFactoryComponent },
            { path: 'IDD_ITEM_STANDARDCOST_HISTORY', component: IDD_ITEM_STANDARDCOST_HISTORYFactoryComponent },
            { path: 'IDD_CTG_PRODUCT_HEADER', component: IDD_CTG_PRODUCT_HEADERFactoryComponent },
            { path: 'IDD_PRODUCERS_COMPLETE', component: IDD_PRODUCERS_COMPLETEFactoryComponent },
            { path: 'IDD_ITMTYP_HEADER', component: IDD_ITMTYP_HEADERFactoryComponent },
            { path: 'IDD_ITEMS_SUBSTITUTE', component: IDD_ITEMS_SUBSTITUTEFactoryComponent },
            { path: 'IDD_ITEM_SETTINGS', component: IDD_ITEM_SETTINGSFactoryComponent },
            { path: 'IDD_RESEARCH_MAN_ITEM', component: IDD_RESEARCH_MAN_ITEMFactoryComponent },
            { path: 'IDD_ITEM_PARAMETERS', component: IDD_ITEM_PARAMETERSFactoryComponent },
            { path: 'IDD_KIT_ITEM_COPY', component: IDD_KIT_ITEM_COPYFactoryComponent },
            { path: 'IDD_KIT_ITEM', component: IDD_KIT_ITEMFactoryComponent },
            { path: 'IDD_ITEMS_GRAPHIC_NAVIG', component: IDD_ITEMS_GRAPHIC_NAVIGFactoryComponent },
            { path: 'IDD_ITEM_COPY', component: IDD_ITEM_COPYFactoryComponent },
            { path: 'IDD_ITEM_BUDGET_COPY', component: IDD_ITEM_BUDGET_COPYFactoryComponent },
            { path: 'IDD_ITEMS_SUMMARIZED', component: IDD_ITEMS_SUMMARIZEDFactoryComponent },
            { path: 'IDD_ITEMS', component: IDD_ITEMSFactoryComponent },
            { path: 'IDD_HOMOGCTG', component: IDD_HOMOGCTGFactoryComponent },
            { path: 'IDD_IMPORT_EXPORT_DETAIL_DOCUMENT', component: IDD_IMPORT_EXPORT_DETAIL_DOCUMENTFactoryComponent },
            { path: 'IDD_DEPARTMENTS_FULL', component: IDD_DEPARTMENTS_FULLFactoryComponent },
            { path: 'IDD_CTGCOMMODITY', component: IDD_CTGCOMMODITYFactoryComponent },
            { path: 'IDD_ITEM_CHOOSE_IMPORT', component: IDD_ITEM_CHOOSE_IMPORTFactoryComponent },
            { path: 'IDD_ACCOUNTING_TYPE', component: IDD_ACCOUNTING_TYPEFactoryComponent },
            { path: 'IDD_SUPPCTG_COMMODITYCTG_FULL', component: IDD_SUPPCTG_COMMODITYCTG_FULLFactoryComponent },
            { path: 'IDD_SUPP_ITEMTYPE_FULL', component: IDD_SUPP_ITEMTYPE_FULLFactoryComponent },
            { path: 'IDD_SUPP_CTGCOMMODITY_FULL', component: IDD_SUPP_CTGCOMMODITY_FULLFactoryComponent },
            { path: 'IDD_ITEMSUPP_FULL', component: IDD_ITEMSUPP_FULLFactoryComponent },
            { path: 'IDD_ITEMCUSTOMERS_FULL', component: IDD_ITEMCUSTOMERS_FULLFactoryComponent },
            { path: 'IDD_CUSTCTG_COMMODITYCTG_FULL', component: IDD_CUSTCTG_COMMODITYCTG_FULLFactoryComponent },
            { path: 'IDD_CUST_ITEMTYPE_FULL', component: IDD_CUST_ITEMTYPE_FULLFactoryComponent },
            { path: 'IDD_CUST_CTGCOMMODITY_FULL', component: IDD_CUST_CTGCOMMODITY_FULLFactoryComponent },
        ])],
    declarations: [
            IDD_UOM_HEADERComponent, IDD_UOM_HEADERFactoryComponent,
            IDD_TAX_CODE_ASSIGNComponent, IDD_TAX_CODE_ASSIGNFactoryComponent,
            IDD_ITEM_STANDARDCOST_HISTORYComponent, IDD_ITEM_STANDARDCOST_HISTORYFactoryComponent,
            IDD_CTG_PRODUCT_HEADERComponent, IDD_CTG_PRODUCT_HEADERFactoryComponent,
            IDD_PRODUCERS_COMPLETEComponent, IDD_PRODUCERS_COMPLETEFactoryComponent,
            IDD_ITMTYP_HEADERComponent, IDD_ITMTYP_HEADERFactoryComponent,
            IDD_ITEMS_SUBSTITUTEComponent, IDD_ITEMS_SUBSTITUTEFactoryComponent,
            IDD_ITEM_SETTINGSComponent, IDD_ITEM_SETTINGSFactoryComponent,
            IDD_RESEARCH_MAN_ITEMComponent, IDD_RESEARCH_MAN_ITEMFactoryComponent,
            IDD_ITEM_PARAMETERSComponent, IDD_ITEM_PARAMETERSFactoryComponent,
            IDD_KIT_ITEM_COPYComponent, IDD_KIT_ITEM_COPYFactoryComponent,
            IDD_KIT_ITEMComponent, IDD_KIT_ITEMFactoryComponent,
            IDD_ITEMS_GRAPHIC_NAVIGComponent, IDD_ITEMS_GRAPHIC_NAVIGFactoryComponent,
            IDD_ITEM_COPYComponent, IDD_ITEM_COPYFactoryComponent,
            IDD_ITEM_BUDGET_COPYComponent, IDD_ITEM_BUDGET_COPYFactoryComponent,
            IDD_ITEMS_SUMMARIZEDComponent, IDD_ITEMS_SUMMARIZEDFactoryComponent,
            IDD_ITEMSComponent, IDD_ITEMSFactoryComponent,
            IDD_HOMOGCTGComponent, IDD_HOMOGCTGFactoryComponent,
            IDD_IMPORT_EXPORT_DETAIL_DOCUMENTComponent, IDD_IMPORT_EXPORT_DETAIL_DOCUMENTFactoryComponent,
            IDD_DEPARTMENTS_FULLComponent, IDD_DEPARTMENTS_FULLFactoryComponent,
            IDD_CTGCOMMODITYComponent, IDD_CTGCOMMODITYFactoryComponent,
            IDD_ITEM_CHOOSE_IMPORTComponent, IDD_ITEM_CHOOSE_IMPORTFactoryComponent,
            IDD_ACCOUNTING_TYPEComponent, IDD_ACCOUNTING_TYPEFactoryComponent,
            IDD_SUPPCTG_COMMODITYCTG_FULLComponent, IDD_SUPPCTG_COMMODITYCTG_FULLFactoryComponent,
            IDD_SUPP_ITEMTYPE_FULLComponent, IDD_SUPP_ITEMTYPE_FULLFactoryComponent,
            IDD_SUPP_CTGCOMMODITY_FULLComponent, IDD_SUPP_CTGCOMMODITY_FULLFactoryComponent,
            IDD_ITEMSUPP_FULLComponent, IDD_ITEMSUPP_FULLFactoryComponent,
            IDD_ITEMCUSTOMERS_FULLComponent, IDD_ITEMCUSTOMERS_FULLFactoryComponent,
            IDD_CUSTCTG_COMMODITYCTG_FULLComponent, IDD_CUSTCTG_COMMODITYCTG_FULLFactoryComponent,
            IDD_CUST_ITEMTYPE_FULLComponent, IDD_CUST_ITEMTYPE_FULLFactoryComponent,
            IDD_CUST_CTGCOMMODITY_FULLComponent, IDD_CUST_CTGCOMMODITY_FULLFactoryComponent,
    ],
    exports: [
            IDD_UOM_HEADERFactoryComponent,
            IDD_TAX_CODE_ASSIGNFactoryComponent,
            IDD_ITEM_STANDARDCOST_HISTORYFactoryComponent,
            IDD_CTG_PRODUCT_HEADERFactoryComponent,
            IDD_PRODUCERS_COMPLETEFactoryComponent,
            IDD_ITMTYP_HEADERFactoryComponent,
            IDD_ITEMS_SUBSTITUTEFactoryComponent,
            IDD_ITEM_SETTINGSFactoryComponent,
            IDD_RESEARCH_MAN_ITEMFactoryComponent,
            IDD_ITEM_PARAMETERSFactoryComponent,
            IDD_KIT_ITEM_COPYFactoryComponent,
            IDD_KIT_ITEMFactoryComponent,
            IDD_ITEMS_GRAPHIC_NAVIGFactoryComponent,
            IDD_ITEM_COPYFactoryComponent,
            IDD_ITEM_BUDGET_COPYFactoryComponent,
            IDD_ITEMS_SUMMARIZEDFactoryComponent,
            IDD_ITEMSFactoryComponent,
            IDD_HOMOGCTGFactoryComponent,
            IDD_IMPORT_EXPORT_DETAIL_DOCUMENTFactoryComponent,
            IDD_DEPARTMENTS_FULLFactoryComponent,
            IDD_CTGCOMMODITYFactoryComponent,
            IDD_ITEM_CHOOSE_IMPORTFactoryComponent,
            IDD_ACCOUNTING_TYPEFactoryComponent,
            IDD_SUPPCTG_COMMODITYCTG_FULLFactoryComponent,
            IDD_SUPP_ITEMTYPE_FULLFactoryComponent,
            IDD_SUPP_CTGCOMMODITY_FULLFactoryComponent,
            IDD_ITEMSUPP_FULLFactoryComponent,
            IDD_ITEMCUSTOMERS_FULLFactoryComponent,
            IDD_CUSTCTG_COMMODITYCTG_FULLFactoryComponent,
            IDD_CUST_ITEMTYPE_FULLFactoryComponent,
            IDD_CUST_CTGCOMMODITY_FULLFactoryComponent,
    ],
    entryComponents: [
            IDD_UOM_HEADERComponent,
            IDD_TAX_CODE_ASSIGNComponent,
            IDD_ITEM_STANDARDCOST_HISTORYComponent,
            IDD_CTG_PRODUCT_HEADERComponent,
            IDD_PRODUCERS_COMPLETEComponent,
            IDD_ITMTYP_HEADERComponent,
            IDD_ITEMS_SUBSTITUTEComponent,
            IDD_ITEM_SETTINGSComponent,
            IDD_RESEARCH_MAN_ITEMComponent,
            IDD_ITEM_PARAMETERSComponent,
            IDD_KIT_ITEM_COPYComponent,
            IDD_KIT_ITEMComponent,
            IDD_ITEMS_GRAPHIC_NAVIGComponent,
            IDD_ITEM_COPYComponent,
            IDD_ITEM_BUDGET_COPYComponent,
            IDD_ITEMS_SUMMARIZEDComponent,
            IDD_ITEMSComponent,
            IDD_HOMOGCTGComponent,
            IDD_IMPORT_EXPORT_DETAIL_DOCUMENTComponent,
            IDD_DEPARTMENTS_FULLComponent,
            IDD_CTGCOMMODITYComponent,
            IDD_ITEM_CHOOSE_IMPORTComponent,
            IDD_ACCOUNTING_TYPEComponent,
            IDD_SUPPCTG_COMMODITYCTG_FULLComponent,
            IDD_SUPP_ITEMTYPE_FULLComponent,
            IDD_SUPP_CTGCOMMODITY_FULLComponent,
            IDD_ITEMSUPP_FULLComponent,
            IDD_ITEMCUSTOMERS_FULLComponent,
            IDD_CUSTCTG_COMMODITYCTG_FULLComponent,
            IDD_CUST_ITEMTYPE_FULLComponent,
            IDD_CUST_CTGCOMMODITY_FULLComponent,
    ]
})


export class ItemsModule { };