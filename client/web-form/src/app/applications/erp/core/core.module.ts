import { IDD_CORE_COMPANYUSER_SETTINGSComponent, IDD_CORE_COMPANYUSER_SETTINGSFactoryComponent } from './coresettings/IDD_CORE_COMPANYUSER_SETTINGS.component';
import { IDD_UPDATE_MANAGERComponent, IDD_UPDATE_MANAGERFactoryComponent } from './uiupdatemanager/IDD_UPDATE_MANAGER.component';
import { IDD_PD_OK_CANCELComponent, IDD_PD_OK_CANCELFactoryComponent } from './uiupdatemanager/IDD_PD_OK_CANCEL.component';
import { IDD_PARAMETERS_PRINTERComponent, IDD_PARAMETERS_PRINTERFactoryComponent } from './uiupdatemanager/IDD_PARAMETERS_PRINTER.component';
import { IDD_PD_PRINTMNG_PRINTINGComponent, IDD_PD_PRINTMNG_PRINTINGFactoryComponent } from './uiprintmng/IDD_PD_PRINTMNG_PRINTING.component';
import { IDD_CROSSREFERENCES_SETTINGSComponent, IDD_CROSSREFERENCES_SETTINGSFactoryComponent } from './uicrossreferencesviewer/IDD_CROSSREFERENCES_SETTINGS.component';
import { IDD_CROSSREFERENCES_MANUALComponent, IDD_CROSSREFERENCES_MANUALFactoryComponent } from './uicrossreferencesviewer/IDD_CROSSREFERENCES_MANUAL.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_CORE_COMPANYUSER_SETTINGS', component: IDD_CORE_COMPANYUSER_SETTINGSFactoryComponent },
            { path: 'IDD_UPDATE_MANAGER', component: IDD_UPDATE_MANAGERFactoryComponent },
            { path: 'IDD_PD_OK_CANCEL', component: IDD_PD_OK_CANCELFactoryComponent },
            { path: 'IDD_PARAMETERS_PRINTER', component: IDD_PARAMETERS_PRINTERFactoryComponent },
            { path: 'IDD_PD_PRINTMNG_PRINTING', component: IDD_PD_PRINTMNG_PRINTINGFactoryComponent },
            { path: 'IDD_CROSSREFERENCES_SETTINGS', component: IDD_CROSSREFERENCES_SETTINGSFactoryComponent },
            { path: 'IDD_CROSSREFERENCES_MANUAL', component: IDD_CROSSREFERENCES_MANUALFactoryComponent },
        ])],
    declarations: [
            IDD_CORE_COMPANYUSER_SETTINGSComponent, IDD_CORE_COMPANYUSER_SETTINGSFactoryComponent,
            IDD_UPDATE_MANAGERComponent, IDD_UPDATE_MANAGERFactoryComponent,
            IDD_PD_OK_CANCELComponent, IDD_PD_OK_CANCELFactoryComponent,
            IDD_PARAMETERS_PRINTERComponent, IDD_PARAMETERS_PRINTERFactoryComponent,
            IDD_PD_PRINTMNG_PRINTINGComponent, IDD_PD_PRINTMNG_PRINTINGFactoryComponent,
            IDD_CROSSREFERENCES_SETTINGSComponent, IDD_CROSSREFERENCES_SETTINGSFactoryComponent,
            IDD_CROSSREFERENCES_MANUALComponent, IDD_CROSSREFERENCES_MANUALFactoryComponent,
    ],
    exports: [
            IDD_CORE_COMPANYUSER_SETTINGSFactoryComponent,
            IDD_UPDATE_MANAGERFactoryComponent,
            IDD_PD_OK_CANCELFactoryComponent,
            IDD_PARAMETERS_PRINTERFactoryComponent,
            IDD_PD_PRINTMNG_PRINTINGFactoryComponent,
            IDD_CROSSREFERENCES_SETTINGSFactoryComponent,
            IDD_CROSSREFERENCES_MANUALFactoryComponent,
    ],
    entryComponents: [
            IDD_CORE_COMPANYUSER_SETTINGSComponent,
            IDD_UPDATE_MANAGERComponent,
            IDD_PD_OK_CANCELComponent,
            IDD_PARAMETERS_PRINTERComponent,
            IDD_PD_PRINTMNG_PRINTINGComponent,
            IDD_CROSSREFERENCES_SETTINGSComponent,
            IDD_CROSSREFERENCES_MANUALComponent,
    ]
})


export class CoreModule { };