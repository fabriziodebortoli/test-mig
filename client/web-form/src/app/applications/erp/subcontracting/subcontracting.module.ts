import { IDD_GENERATE_SUBCONTRACTINGORDERComponent, IDD_GENERATE_SUBCONTRACTINGORDERFactoryComponent } from './subcontractorordgeneration/IDD_GENERATE_SUBCONTRACTINGORDER.component';
import { IDD_SUBCONTRACTOR_ORDComponent, IDD_SUBCONTRACTOR_ORDFactoryComponent } from './subcontractorord/IDD_SUBCONTRACTOR_ORD.component';
import { IDD_SUBCNTDNGENERATIONComponent, IDD_SUBCNTDNGENERATIONFactoryComponent } from './subcontractordngeneration/IDD_SUBCNTDNGENERATION.component';
import { IDD_SUBCONTRACTORComponent, IDD_SUBCONTRACTORFactoryComponent } from './subcontractordn/IDD_SUBCONTRACTOR.component';
import { IDD_SUBCONTRACTOR_BOLComponent, IDD_SUBCONTRACTOR_BOLFactoryComponent } from './subcontractorbol/IDD_SUBCONTRACTOR_BOL.component';
import { IDD_MO_STEPS_OUTSOURCEDComponent, IDD_MO_STEPS_OUTSOURCEDFactoryComponent } from './subcntordoutsourcedmosteps/IDD_MO_STEPS_OUTSOURCED.component';
import { IDD_DOC_SUBCONTRACTINGComponent, IDD_DOC_SUBCONTRACTINGFactoryComponent } from './subcntbolshoppaperslist/IDD_DOC_SUBCONTRACTING.component';
import { IDD_MO_FROM_CONFIRMComponent, IDD_MO_FROM_CONFIRMFactoryComponent } from './subcntbolmosteptoprocesslist/IDD_MO_FROM_CONFIRM.component';
import { IDD_DOC_SUBCONTRACTING_COMPComponent, IDD_DOC_SUBCONTRACTING_COMPFactoryComponent } from './subcntbolmocomponentslist/IDD_DOC_SUBCONTRACTING_COMP.component';
import { IDD_MO_LISTComponent, IDD_MO_LISTFactoryComponent } from './moslist/IDD_MO_LIST.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_GENERATE_SUBCONTRACTINGORDER', component: IDD_GENERATE_SUBCONTRACTINGORDERFactoryComponent },
            { path: 'IDD_SUBCONTRACTOR_ORD', component: IDD_SUBCONTRACTOR_ORDFactoryComponent },
            { path: 'IDD_SUBCNTDNGENERATION', component: IDD_SUBCNTDNGENERATIONFactoryComponent },
            { path: 'IDD_SUBCONTRACTOR', component: IDD_SUBCONTRACTORFactoryComponent },
            { path: 'IDD_SUBCONTRACTOR_BOL', component: IDD_SUBCONTRACTOR_BOLFactoryComponent },
            { path: 'IDD_MO_STEPS_OUTSOURCED', component: IDD_MO_STEPS_OUTSOURCEDFactoryComponent },
            { path: 'IDD_DOC_SUBCONTRACTING', component: IDD_DOC_SUBCONTRACTINGFactoryComponent },
            { path: 'IDD_MO_FROM_CONFIRM', component: IDD_MO_FROM_CONFIRMFactoryComponent },
            { path: 'IDD_DOC_SUBCONTRACTING_COMP', component: IDD_DOC_SUBCONTRACTING_COMPFactoryComponent },
            { path: 'IDD_MO_LIST', component: IDD_MO_LISTFactoryComponent },
        ])],
    declarations: [
            IDD_GENERATE_SUBCONTRACTINGORDERComponent, IDD_GENERATE_SUBCONTRACTINGORDERFactoryComponent,
            IDD_SUBCONTRACTOR_ORDComponent, IDD_SUBCONTRACTOR_ORDFactoryComponent,
            IDD_SUBCNTDNGENERATIONComponent, IDD_SUBCNTDNGENERATIONFactoryComponent,
            IDD_SUBCONTRACTORComponent, IDD_SUBCONTRACTORFactoryComponent,
            IDD_SUBCONTRACTOR_BOLComponent, IDD_SUBCONTRACTOR_BOLFactoryComponent,
            IDD_MO_STEPS_OUTSOURCEDComponent, IDD_MO_STEPS_OUTSOURCEDFactoryComponent,
            IDD_DOC_SUBCONTRACTINGComponent, IDD_DOC_SUBCONTRACTINGFactoryComponent,
            IDD_MO_FROM_CONFIRMComponent, IDD_MO_FROM_CONFIRMFactoryComponent,
            IDD_DOC_SUBCONTRACTING_COMPComponent, IDD_DOC_SUBCONTRACTING_COMPFactoryComponent,
            IDD_MO_LISTComponent, IDD_MO_LISTFactoryComponent,
    ],
    exports: [
            IDD_GENERATE_SUBCONTRACTINGORDERFactoryComponent,
            IDD_SUBCONTRACTOR_ORDFactoryComponent,
            IDD_SUBCNTDNGENERATIONFactoryComponent,
            IDD_SUBCONTRACTORFactoryComponent,
            IDD_SUBCONTRACTOR_BOLFactoryComponent,
            IDD_MO_STEPS_OUTSOURCEDFactoryComponent,
            IDD_DOC_SUBCONTRACTINGFactoryComponent,
            IDD_MO_FROM_CONFIRMFactoryComponent,
            IDD_DOC_SUBCONTRACTING_COMPFactoryComponent,
            IDD_MO_LISTFactoryComponent,
    ],
    entryComponents: [
            IDD_GENERATE_SUBCONTRACTINGORDERComponent,
            IDD_SUBCONTRACTOR_ORDComponent,
            IDD_SUBCNTDNGENERATIONComponent,
            IDD_SUBCONTRACTORComponent,
            IDD_SUBCONTRACTOR_BOLComponent,
            IDD_MO_STEPS_OUTSOURCEDComponent,
            IDD_DOC_SUBCONTRACTINGComponent,
            IDD_MO_FROM_CONFIRMComponent,
            IDD_DOC_SUBCONTRACTING_COMPComponent,
            IDD_MO_LISTComponent,
    ]
})


export class SubcontractingModule { };