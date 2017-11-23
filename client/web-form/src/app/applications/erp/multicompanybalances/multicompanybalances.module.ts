import { IDD_TEMPLATEIMPORTComponent, IDD_TEMPLATEIMPORTFactoryComponent } from './templateimport/IDD_TEMPLATEIMPORT.component';
import { IDD_TEMPLATEGENERATIONComponent, IDD_TEMPLATEGENERATIONFactoryComponent } from './templategeneration/IDD_TEMPLATEGENERATION.component';
import { IDD_TEMPLATEEXPORTComponent, IDD_TEMPLATEEXPORTFactoryComponent } from './templateexport/IDD_TEMPLATEEXPORT.component';
import { IDD_OWNERCOMPANIESComponent, IDD_OWNERCOMPANIESFactoryComponent } from './ownercompanies/IDD_OWNERCOMPANIES.component';
import { IDD_OWNEDCOMPANIESComponent, IDD_OWNEDCOMPANIESFactoryComponent } from './ownedcompanies/IDD_OWNEDCOMPANIES.component';
import { IDD_MULTICOMPANYBALANCESComponent, IDD_MULTICOMPANYBALANCESFactoryComponent } from './multicompanybalances/IDD_MULTICOMPANYBALANCES.component';
import { IDD_TEMPLATESCONSOLIDComponent, IDD_TEMPLATESCONSOLIDFactoryComponent } from './consolidationtemplates/IDD_TEMPLATESCONSOLID.component';
import { IDD_PARAM_CONSOLIDComponent, IDD_PARAM_CONSOLIDFactoryComponent } from './consbalsheetparameters/IDD_PARAM_CONSOLID.component';
import { IDD_COMPANYGROUPSComponent, IDD_COMPANYGROUPSFactoryComponent } from './companygroups/IDD_COMPANYGROUPS.component';
import { IDD_BALANCEIMPORTComponent, IDD_BALANCEIMPORTFactoryComponent } from './balanceimport/IDD_BALANCEIMPORT.component';
import { IDD_BALANCEGENERATIONComponent, IDD_BALANCEGENERATIONFactoryComponent } from './balancegeneration/IDD_BALANCEGENERATION.component';
import { IDD_BALANCEEXPORTComponent, IDD_BALANCEEXPORTFactoryComponent } from './balanceexport/IDD_BALANCEEXPORT.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TEMPLATEIMPORT', component: IDD_TEMPLATEIMPORTFactoryComponent },
            { path: 'IDD_TEMPLATEGENERATION', component: IDD_TEMPLATEGENERATIONFactoryComponent },
            { path: 'IDD_TEMPLATEEXPORT', component: IDD_TEMPLATEEXPORTFactoryComponent },
            { path: 'IDD_OWNERCOMPANIES', component: IDD_OWNERCOMPANIESFactoryComponent },
            { path: 'IDD_OWNEDCOMPANIES', component: IDD_OWNEDCOMPANIESFactoryComponent },
            { path: 'IDD_MULTICOMPANYBALANCES', component: IDD_MULTICOMPANYBALANCESFactoryComponent },
            { path: 'IDD_TEMPLATESCONSOLID', component: IDD_TEMPLATESCONSOLIDFactoryComponent },
            { path: 'IDD_PARAM_CONSOLID', component: IDD_PARAM_CONSOLIDFactoryComponent },
            { path: 'IDD_COMPANYGROUPS', component: IDD_COMPANYGROUPSFactoryComponent },
            { path: 'IDD_BALANCEIMPORT', component: IDD_BALANCEIMPORTFactoryComponent },
            { path: 'IDD_BALANCEGENERATION', component: IDD_BALANCEGENERATIONFactoryComponent },
            { path: 'IDD_BALANCEEXPORT', component: IDD_BALANCEEXPORTFactoryComponent },
        ])],
    declarations: [
            IDD_TEMPLATEIMPORTComponent, IDD_TEMPLATEIMPORTFactoryComponent,
            IDD_TEMPLATEGENERATIONComponent, IDD_TEMPLATEGENERATIONFactoryComponent,
            IDD_TEMPLATEEXPORTComponent, IDD_TEMPLATEEXPORTFactoryComponent,
            IDD_OWNERCOMPANIESComponent, IDD_OWNERCOMPANIESFactoryComponent,
            IDD_OWNEDCOMPANIESComponent, IDD_OWNEDCOMPANIESFactoryComponent,
            IDD_MULTICOMPANYBALANCESComponent, IDD_MULTICOMPANYBALANCESFactoryComponent,
            IDD_TEMPLATESCONSOLIDComponent, IDD_TEMPLATESCONSOLIDFactoryComponent,
            IDD_PARAM_CONSOLIDComponent, IDD_PARAM_CONSOLIDFactoryComponent,
            IDD_COMPANYGROUPSComponent, IDD_COMPANYGROUPSFactoryComponent,
            IDD_BALANCEIMPORTComponent, IDD_BALANCEIMPORTFactoryComponent,
            IDD_BALANCEGENERATIONComponent, IDD_BALANCEGENERATIONFactoryComponent,
            IDD_BALANCEEXPORTComponent, IDD_BALANCEEXPORTFactoryComponent,
    ],
    exports: [
            IDD_TEMPLATEIMPORTFactoryComponent,
            IDD_TEMPLATEGENERATIONFactoryComponent,
            IDD_TEMPLATEEXPORTFactoryComponent,
            IDD_OWNERCOMPANIESFactoryComponent,
            IDD_OWNEDCOMPANIESFactoryComponent,
            IDD_MULTICOMPANYBALANCESFactoryComponent,
            IDD_TEMPLATESCONSOLIDFactoryComponent,
            IDD_PARAM_CONSOLIDFactoryComponent,
            IDD_COMPANYGROUPSFactoryComponent,
            IDD_BALANCEIMPORTFactoryComponent,
            IDD_BALANCEGENERATIONFactoryComponent,
            IDD_BALANCEEXPORTFactoryComponent,
    ],
    entryComponents: [
            IDD_TEMPLATEIMPORTComponent,
            IDD_TEMPLATEGENERATIONComponent,
            IDD_TEMPLATEEXPORTComponent,
            IDD_OWNERCOMPANIESComponent,
            IDD_OWNEDCOMPANIESComponent,
            IDD_MULTICOMPANYBALANCESComponent,
            IDD_TEMPLATESCONSOLIDComponent,
            IDD_PARAM_CONSOLIDComponent,
            IDD_COMPANYGROUPSComponent,
            IDD_BALANCEIMPORTComponent,
            IDD_BALANCEGENERATIONComponent,
            IDD_BALANCEEXPORTComponent,
    ]
})


export class MultiCompanyBalancesModule { };