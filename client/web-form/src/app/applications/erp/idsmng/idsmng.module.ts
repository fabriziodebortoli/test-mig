import { IDD_TAXJOURN_RANGEComponent, IDD_TAXJOURN_RANGEFactoryComponent } from './taxjournalsrange/IDD_TAXJOURN_RANGE.component';
import { IDD_TAXJOURNALComponent, IDD_TAXJOURNALFactoryComponent } from './taxjournals/IDD_TAXJOURNAL.component';
import { IDD_NUMERATOR_TAXJOURNALSComponent, IDD_NUMERATOR_TAXJOURNALSFactoryComponent } from './taxjournalnumbers/IDD_NUMERATOR_TAXJOURNALS.component';
import { IDD_DEBILLSTComponent, IDD_DEBILLSTFactoryComponent } from './stubbooks/IDD_DEBILLST.component';
import { IDD_NUMERATOR_BILLSComponent, IDD_NUMERATOR_BILLSFactoryComponent } from './stubbooknumbers/IDD_NUMERATOR_BILLS.component';
import { IDD_PARAMETERS_IDSMNGComponent, IDD_PARAMETERS_IDSMNGFactoryComponent } from './numberparameters/IDD_PARAMETERS_IDSMNG.component';
import { IDD_NUMERATOR_NOT_FISCALComponent, IDD_NUMERATOR_NOT_FISCALFactoryComponent } from './nonfiscalnumbers/IDD_NUMERATOR_NOT_FISCAL.component';
import { IDD_IDNUMBERSComponent, IDD_IDNUMBERSFactoryComponent } from './idnumbers/IDD_IDNUMBERS.component';
import { IDD_REDUCEDACCTPLComponent, IDD_REDUCEDACCTPLFactoryComponent } from './accountingtemplates/IDD_REDUCEDACCTPL.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TAXJOURN_RANGE', component: IDD_TAXJOURN_RANGEFactoryComponent },
            { path: 'IDD_TAXJOURNAL', component: IDD_TAXJOURNALFactoryComponent },
            { path: 'IDD_NUMERATOR_TAXJOURNALS', component: IDD_NUMERATOR_TAXJOURNALSFactoryComponent },
            { path: 'IDD_DEBILLST', component: IDD_DEBILLSTFactoryComponent },
            { path: 'IDD_NUMERATOR_BILLS', component: IDD_NUMERATOR_BILLSFactoryComponent },
            { path: 'IDD_PARAMETERS_IDSMNG', component: IDD_PARAMETERS_IDSMNGFactoryComponent },
            { path: 'IDD_NUMERATOR_NOT_FISCAL', component: IDD_NUMERATOR_NOT_FISCALFactoryComponent },
            { path: 'IDD_IDNUMBERS', component: IDD_IDNUMBERSFactoryComponent },
            { path: 'IDD_REDUCEDACCTPL', component: IDD_REDUCEDACCTPLFactoryComponent },
        ])],
    declarations: [
            IDD_TAXJOURN_RANGEComponent, IDD_TAXJOURN_RANGEFactoryComponent,
            IDD_TAXJOURNALComponent, IDD_TAXJOURNALFactoryComponent,
            IDD_NUMERATOR_TAXJOURNALSComponent, IDD_NUMERATOR_TAXJOURNALSFactoryComponent,
            IDD_DEBILLSTComponent, IDD_DEBILLSTFactoryComponent,
            IDD_NUMERATOR_BILLSComponent, IDD_NUMERATOR_BILLSFactoryComponent,
            IDD_PARAMETERS_IDSMNGComponent, IDD_PARAMETERS_IDSMNGFactoryComponent,
            IDD_NUMERATOR_NOT_FISCALComponent, IDD_NUMERATOR_NOT_FISCALFactoryComponent,
            IDD_IDNUMBERSComponent, IDD_IDNUMBERSFactoryComponent,
            IDD_REDUCEDACCTPLComponent, IDD_REDUCEDACCTPLFactoryComponent,
    ],
    exports: [
            IDD_TAXJOURN_RANGEFactoryComponent,
            IDD_TAXJOURNALFactoryComponent,
            IDD_NUMERATOR_TAXJOURNALSFactoryComponent,
            IDD_DEBILLSTFactoryComponent,
            IDD_NUMERATOR_BILLSFactoryComponent,
            IDD_PARAMETERS_IDSMNGFactoryComponent,
            IDD_NUMERATOR_NOT_FISCALFactoryComponent,
            IDD_IDNUMBERSFactoryComponent,
            IDD_REDUCEDACCTPLFactoryComponent,
    ],
    entryComponents: [
            IDD_TAXJOURN_RANGEComponent,
            IDD_TAXJOURNALComponent,
            IDD_NUMERATOR_TAXJOURNALSComponent,
            IDD_DEBILLSTComponent,
            IDD_NUMERATOR_BILLSComponent,
            IDD_PARAMETERS_IDSMNGComponent,
            IDD_NUMERATOR_NOT_FISCALComponent,
            IDD_IDNUMBERSComponent,
            IDD_REDUCEDACCTPLComponent,
    ]
})


export class IdsMngModule { };