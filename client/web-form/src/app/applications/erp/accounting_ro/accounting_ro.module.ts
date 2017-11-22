import { IDD_TAXDECLARATION_300Component, IDD_TAXDECLARATION_300FactoryComponent } from './taxdeclaration300/IDD_TAXDECLARATION_300.component';
import { IDD_TAXDECLARATIONComponent, IDD_TAXDECLARATIONFactoryComponent } from './taxdeclaration/IDD_TAXDECLARATION.component';
import { IDD_TAXDECLAR394_SLAVEComponent, IDD_TAXDECLAR394_SLAVEFactoryComponent } from './taxdeclar394/IDD_TAXDECLAR394_SLAVE.component';
import { IDD_TAXDECLAR394Component, IDD_TAXDECLAR394FactoryComponent } from './taxdeclar394/IDD_TAXDECLAR394.component';
import { IDD_394PARAMETERSComponent, IDD_394PARAMETERSFactoryComponent } from './d394parameters/IDD_394PARAMETERS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TAXDECLARATION_300', component: IDD_TAXDECLARATION_300FactoryComponent },
            { path: 'IDD_TAXDECLARATION', component: IDD_TAXDECLARATIONFactoryComponent },
            { path: 'IDD_TAXDECLAR394_SLAVE', component: IDD_TAXDECLAR394_SLAVEFactoryComponent },
            { path: 'IDD_TAXDECLAR394', component: IDD_TAXDECLAR394FactoryComponent },
            { path: 'IDD_394PARAMETERS', component: IDD_394PARAMETERSFactoryComponent },
        ])],
    declarations: [
            IDD_TAXDECLARATION_300Component, IDD_TAXDECLARATION_300FactoryComponent,
            IDD_TAXDECLARATIONComponent, IDD_TAXDECLARATIONFactoryComponent,
            IDD_TAXDECLAR394_SLAVEComponent, IDD_TAXDECLAR394_SLAVEFactoryComponent,
            IDD_TAXDECLAR394Component, IDD_TAXDECLAR394FactoryComponent,
            IDD_394PARAMETERSComponent, IDD_394PARAMETERSFactoryComponent,
    ],
    exports: [
            IDD_TAXDECLARATION_300FactoryComponent,
            IDD_TAXDECLARATIONFactoryComponent,
            IDD_TAXDECLAR394_SLAVEFactoryComponent,
            IDD_TAXDECLAR394FactoryComponent,
            IDD_394PARAMETERSFactoryComponent,
    ],
    entryComponents: [
            IDD_TAXDECLARATION_300Component,
            IDD_TAXDECLARATIONComponent,
            IDD_TAXDECLAR394_SLAVEComponent,
            IDD_TAXDECLAR394Component,
            IDD_394PARAMETERSComponent,
    ]
})


export class Accounting_ROModule { };