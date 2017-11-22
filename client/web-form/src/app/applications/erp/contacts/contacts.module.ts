import { IDD_PROSPECTIVESUPPDELETINGComponent, IDD_PROSPECTIVESUPPDELETINGFactoryComponent } from './prospectivesuppliersdeleting/IDD_PROSPECTIVESUPPDELETING.component';
import { IDD_PROSPSUPPComponent, IDD_PROSPSUPPFactoryComponent } from './prospectivesuppliers/IDD_PROSPSUPP.component';
import { IDD_PROSPSUPP_BRANCHES_ADD_ON_FLYComponent, IDD_PROSPSUPP_BRANCHES_ADD_ON_FLYFactoryComponent } from './prospectivesupplierbranches/IDD_PROSPSUPP_BRANCHES_ADD_ON_FLY.component';
import { IDD_CONTACTSPECIFICATIONComponent, IDD_CONTACTSPECIFICATIONFactoryComponent } from './contactspecification/IDD_CONTACTSPECIFICATION.component';
import { IDD_DELETE_CONTACTSComponent, IDD_DELETE_CONTACTSFactoryComponent } from './contactsdeleting/IDD_DELETE_CONTACTS.component';
import { IDD_COPYCONTACTComponent, IDD_COPYCONTACTFactoryComponent } from './contactscopy/IDD_COPYCONTACT.component';
import { IDD_CONTACTSComponent, IDD_CONTACTSFactoryComponent } from './contacts/IDD_CONTACTS.component';
import { IDD_CONTACTORIGINComponent, IDD_CONTACTORIGINFactoryComponent } from './contactorigin/IDD_CONTACTORIGIN.component';
import { IDD_CONTACTS_BRANCHES_ADD_ON_FLYComponent, IDD_CONTACTS_BRANCHES_ADD_ON_FLYFactoryComponent } from './contactbranches/IDD_CONTACTS_BRANCHES_ADD_ON_FLY.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_PROSPECTIVESUPPDELETING', component: IDD_PROSPECTIVESUPPDELETINGFactoryComponent },
            { path: 'IDD_PROSPSUPP', component: IDD_PROSPSUPPFactoryComponent },
            { path: 'IDD_PROSPSUPP_BRANCHES_ADD_ON_FLY', component: IDD_PROSPSUPP_BRANCHES_ADD_ON_FLYFactoryComponent },
            { path: 'IDD_CONTACTSPECIFICATION', component: IDD_CONTACTSPECIFICATIONFactoryComponent },
            { path: 'IDD_DELETE_CONTACTS', component: IDD_DELETE_CONTACTSFactoryComponent },
            { path: 'IDD_COPYCONTACT', component: IDD_COPYCONTACTFactoryComponent },
            { path: 'IDD_CONTACTS', component: IDD_CONTACTSFactoryComponent },
            { path: 'IDD_CONTACTORIGIN', component: IDD_CONTACTORIGINFactoryComponent },
            { path: 'IDD_CONTACTS_BRANCHES_ADD_ON_FLY', component: IDD_CONTACTS_BRANCHES_ADD_ON_FLYFactoryComponent },
        ])],
    declarations: [
            IDD_PROSPECTIVESUPPDELETINGComponent, IDD_PROSPECTIVESUPPDELETINGFactoryComponent,
            IDD_PROSPSUPPComponent, IDD_PROSPSUPPFactoryComponent,
            IDD_PROSPSUPP_BRANCHES_ADD_ON_FLYComponent, IDD_PROSPSUPP_BRANCHES_ADD_ON_FLYFactoryComponent,
            IDD_CONTACTSPECIFICATIONComponent, IDD_CONTACTSPECIFICATIONFactoryComponent,
            IDD_DELETE_CONTACTSComponent, IDD_DELETE_CONTACTSFactoryComponent,
            IDD_COPYCONTACTComponent, IDD_COPYCONTACTFactoryComponent,
            IDD_CONTACTSComponent, IDD_CONTACTSFactoryComponent,
            IDD_CONTACTORIGINComponent, IDD_CONTACTORIGINFactoryComponent,
            IDD_CONTACTS_BRANCHES_ADD_ON_FLYComponent, IDD_CONTACTS_BRANCHES_ADD_ON_FLYFactoryComponent,
    ],
    exports: [
            IDD_PROSPECTIVESUPPDELETINGFactoryComponent,
            IDD_PROSPSUPPFactoryComponent,
            IDD_PROSPSUPP_BRANCHES_ADD_ON_FLYFactoryComponent,
            IDD_CONTACTSPECIFICATIONFactoryComponent,
            IDD_DELETE_CONTACTSFactoryComponent,
            IDD_COPYCONTACTFactoryComponent,
            IDD_CONTACTSFactoryComponent,
            IDD_CONTACTORIGINFactoryComponent,
            IDD_CONTACTS_BRANCHES_ADD_ON_FLYFactoryComponent,
    ],
    entryComponents: [
            IDD_PROSPECTIVESUPPDELETINGComponent,
            IDD_PROSPSUPPComponent,
            IDD_PROSPSUPP_BRANCHES_ADD_ON_FLYComponent,
            IDD_CONTACTSPECIFICATIONComponent,
            IDD_DELETE_CONTACTSComponent,
            IDD_COPYCONTACTComponent,
            IDD_CONTACTSComponent,
            IDD_CONTACTORIGINComponent,
            IDD_CONTACTS_BRANCHES_ADD_ON_FLYComponent,
    ]
})


export class ContactsModule { };