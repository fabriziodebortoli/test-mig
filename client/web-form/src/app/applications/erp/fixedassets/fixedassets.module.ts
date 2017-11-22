import { IDD_LOCATIONSComponent, IDD_LOCATIONSFactoryComponent } from './locations/IDD_LOCATIONS.component';
import { IDD_RECALENTRY_FIXEDASSETSComponent, IDD_RECALENTRY_FIXEDASSETSFactoryComponent } from './fixedassetsrebuilding/IDD_RECALENTRY_FIXEDASSETS.component';
import { IDD_FIXASSETSREASONSComponent, IDD_FIXASSETSREASONSFactoryComponent } from './fixedassetsreasons/IDD_FIXASSETSREASONS.component';
import { IDD_ENTRYRSNComponent, IDD_ENTRYRSNFactoryComponent } from './fixedassetsreasons/IDD_ENTRYRSN.component';
import { IDD_PARAMETERSComponent, IDD_PARAMETERSFactoryComponent } from './fixedassetsparameters/IDD_PARAMETERS.component';
import { IDD_FIXASSGRAPHComponent, IDD_FIXASSGRAPHFactoryComponent } from './fixedassetsnavigation/IDD_FIXASSGRAPH.component';
import { IDD_FIXEDASSETSJOURNALComponent, IDD_FIXEDASSETSJOURNALFactoryComponent } from './fixedassetsjournal/IDD_FIXEDASSETSJOURNAL.component';
import { IDD_FAENTRYComponent, IDD_FAENTRYFactoryComponent } from './fixedassetsentries/IDD_FAENTRY.component';
import { IDD_FA_PERIODDATAComponent, IDD_FA_PERIODDATAFactoryComponent } from './fixassetsperioddata/IDD_FA_PERIODDATA.component';
import { IDD_DISPOSALENTRYComponent, IDD_DISPOSALENTRYFactoryComponent } from './disposalingrid/IDD_DISPOSALENTRY.component';
import { IDD_FIXEDASSETSDEPRTPLComponent, IDD_FIXEDASSETSDEPRTPLFactoryComponent } from './deprtemplates/IDD_FIXEDASSETSDEPRTPL.component';
import { IDD_DEPRECIATIONDELETEComponent, IDD_DEPRECIATIONDELETEFactoryComponent } from './depreciationdeleting/IDD_DEPRECIATIONDELETE.component';
import { IDD_CLASSESComponent, IDD_CLASSESFactoryComponent } from './classes/IDD_CLASSES.component';
import { IDD_FIXEDASSETS_REDUCEDComponent, IDD_FIXEDASSETS_REDUCEDFactoryComponent } from './uifixedassets/IDD_FIXEDASSETS_REDUCED.component';
import { IDD_FIXEDASSETSComponent, IDD_FIXEDASSETSFactoryComponent } from './uifixedassets/IDD_FIXEDASSETS.component';
import { IDD_INITIALVALUESPOSTINGComponent, IDD_INITIALVALUESPOSTINGFactoryComponent } from './uidepreciation/IDD_INITIALVALUESPOSTING.component';
import { IDD_DEPRECIATIONINFORECASTACCOUNTINGComponent, IDD_DEPRECIATIONINFORECASTACCOUNTINGFactoryComponent } from './uidepreciation/IDD_DEPRECIATIONINFORECASTACCOUNTING.component';
import { IDD_DEPRECIATIONComponent, IDD_DEPRECIATIONFactoryComponent } from './uidepreciation/IDD_DEPRECIATION.component';
import { IDD_BALANCEVALUESRESUMEComponent, IDD_BALANCEVALUESRESUMEFactoryComponent } from './uidepreciation/IDD_BALANCEVALUESRESUME.component';
import { IDD_ALIGNMENTComponent, IDD_ALIGNMENTFactoryComponent } from './uidepreciation/IDD_ALIGNMENT.component';
import { IDD_CATEGORIESComponent, IDD_CATEGORIESFactoryComponent } from './uicategories/IDD_CATEGORIES.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_LOCATIONS', component: IDD_LOCATIONSFactoryComponent },
            { path: 'IDD_RECALENTRY_FIXEDASSETS', component: IDD_RECALENTRY_FIXEDASSETSFactoryComponent },
            { path: 'IDD_FIXASSETSREASONS', component: IDD_FIXASSETSREASONSFactoryComponent },
            { path: 'IDD_ENTRYRSN', component: IDD_ENTRYRSNFactoryComponent },
            { path: 'IDD_PARAMETERS', component: IDD_PARAMETERSFactoryComponent },
            { path: 'IDD_FIXASSGRAPH', component: IDD_FIXASSGRAPHFactoryComponent },
            { path: 'IDD_FIXEDASSETSJOURNAL', component: IDD_FIXEDASSETSJOURNALFactoryComponent },
            { path: 'IDD_FAENTRY', component: IDD_FAENTRYFactoryComponent },
            { path: 'IDD_FA_PERIODDATA', component: IDD_FA_PERIODDATAFactoryComponent },
            { path: 'IDD_DISPOSALENTRY', component: IDD_DISPOSALENTRYFactoryComponent },
            { path: 'IDD_FIXEDASSETSDEPRTPL', component: IDD_FIXEDASSETSDEPRTPLFactoryComponent },
            { path: 'IDD_DEPRECIATIONDELETE', component: IDD_DEPRECIATIONDELETEFactoryComponent },
            { path: 'IDD_CLASSES', component: IDD_CLASSESFactoryComponent },
            { path: 'IDD_FIXEDASSETS_REDUCED', component: IDD_FIXEDASSETS_REDUCEDFactoryComponent },
            { path: 'IDD_FIXEDASSETS', component: IDD_FIXEDASSETSFactoryComponent },
            { path: 'IDD_INITIALVALUESPOSTING', component: IDD_INITIALVALUESPOSTINGFactoryComponent },
            { path: 'IDD_DEPRECIATIONINFORECASTACCOUNTING', component: IDD_DEPRECIATIONINFORECASTACCOUNTINGFactoryComponent },
            { path: 'IDD_DEPRECIATION', component: IDD_DEPRECIATIONFactoryComponent },
            { path: 'IDD_BALANCEVALUESRESUME', component: IDD_BALANCEVALUESRESUMEFactoryComponent },
            { path: 'IDD_ALIGNMENT', component: IDD_ALIGNMENTFactoryComponent },
            { path: 'IDD_CATEGORIES', component: IDD_CATEGORIESFactoryComponent },
        ])],
    declarations: [
            IDD_LOCATIONSComponent, IDD_LOCATIONSFactoryComponent,
            IDD_RECALENTRY_FIXEDASSETSComponent, IDD_RECALENTRY_FIXEDASSETSFactoryComponent,
            IDD_FIXASSETSREASONSComponent, IDD_FIXASSETSREASONSFactoryComponent,
            IDD_ENTRYRSNComponent, IDD_ENTRYRSNFactoryComponent,
            IDD_PARAMETERSComponent, IDD_PARAMETERSFactoryComponent,
            IDD_FIXASSGRAPHComponent, IDD_FIXASSGRAPHFactoryComponent,
            IDD_FIXEDASSETSJOURNALComponent, IDD_FIXEDASSETSJOURNALFactoryComponent,
            IDD_FAENTRYComponent, IDD_FAENTRYFactoryComponent,
            IDD_FA_PERIODDATAComponent, IDD_FA_PERIODDATAFactoryComponent,
            IDD_DISPOSALENTRYComponent, IDD_DISPOSALENTRYFactoryComponent,
            IDD_FIXEDASSETSDEPRTPLComponent, IDD_FIXEDASSETSDEPRTPLFactoryComponent,
            IDD_DEPRECIATIONDELETEComponent, IDD_DEPRECIATIONDELETEFactoryComponent,
            IDD_CLASSESComponent, IDD_CLASSESFactoryComponent,
            IDD_FIXEDASSETS_REDUCEDComponent, IDD_FIXEDASSETS_REDUCEDFactoryComponent,
            IDD_FIXEDASSETSComponent, IDD_FIXEDASSETSFactoryComponent,
            IDD_INITIALVALUESPOSTINGComponent, IDD_INITIALVALUESPOSTINGFactoryComponent,
            IDD_DEPRECIATIONINFORECASTACCOUNTINGComponent, IDD_DEPRECIATIONINFORECASTACCOUNTINGFactoryComponent,
            IDD_DEPRECIATIONComponent, IDD_DEPRECIATIONFactoryComponent,
            IDD_BALANCEVALUESRESUMEComponent, IDD_BALANCEVALUESRESUMEFactoryComponent,
            IDD_ALIGNMENTComponent, IDD_ALIGNMENTFactoryComponent,
            IDD_CATEGORIESComponent, IDD_CATEGORIESFactoryComponent,
    ],
    exports: [
            IDD_LOCATIONSFactoryComponent,
            IDD_RECALENTRY_FIXEDASSETSFactoryComponent,
            IDD_FIXASSETSREASONSFactoryComponent,
            IDD_ENTRYRSNFactoryComponent,
            IDD_PARAMETERSFactoryComponent,
            IDD_FIXASSGRAPHFactoryComponent,
            IDD_FIXEDASSETSJOURNALFactoryComponent,
            IDD_FAENTRYFactoryComponent,
            IDD_FA_PERIODDATAFactoryComponent,
            IDD_DISPOSALENTRYFactoryComponent,
            IDD_FIXEDASSETSDEPRTPLFactoryComponent,
            IDD_DEPRECIATIONDELETEFactoryComponent,
            IDD_CLASSESFactoryComponent,
            IDD_FIXEDASSETS_REDUCEDFactoryComponent,
            IDD_FIXEDASSETSFactoryComponent,
            IDD_INITIALVALUESPOSTINGFactoryComponent,
            IDD_DEPRECIATIONINFORECASTACCOUNTINGFactoryComponent,
            IDD_DEPRECIATIONFactoryComponent,
            IDD_BALANCEVALUESRESUMEFactoryComponent,
            IDD_ALIGNMENTFactoryComponent,
            IDD_CATEGORIESFactoryComponent,
    ],
    entryComponents: [
            IDD_LOCATIONSComponent,
            IDD_RECALENTRY_FIXEDASSETSComponent,
            IDD_FIXASSETSREASONSComponent,
            IDD_ENTRYRSNComponent,
            IDD_PARAMETERSComponent,
            IDD_FIXASSGRAPHComponent,
            IDD_FIXEDASSETSJOURNALComponent,
            IDD_FAENTRYComponent,
            IDD_FA_PERIODDATAComponent,
            IDD_DISPOSALENTRYComponent,
            IDD_FIXEDASSETSDEPRTPLComponent,
            IDD_DEPRECIATIONDELETEComponent,
            IDD_CLASSESComponent,
            IDD_FIXEDASSETS_REDUCEDComponent,
            IDD_FIXEDASSETSComponent,
            IDD_INITIALVALUESPOSTINGComponent,
            IDD_DEPRECIATIONINFORECASTACCOUNTINGComponent,
            IDD_DEPRECIATIONComponent,
            IDD_BALANCEVALUESRESUMEComponent,
            IDD_ALIGNMENTComponent,
            IDD_CATEGORIESComponent,
    ]
})


export class FixedAssetsModule { };