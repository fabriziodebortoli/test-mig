import { IDD_TECHNICAL_DATA_DEFINITIONComponent, IDD_TECHNICAL_DATA_DEFINITIONFactoryComponent } from './technicaldatadefinition/IDD_TECHNICAL_DATA_DEFINITION.component';
import { IDD_RESULTSComponent, IDD_RESULTSFactoryComponent } from './results/IDD_RESULTS.component';
import { IDD_POSTING_QUALITYINSPECTIONComponent, IDD_POSTING_QUALITYINSPECTIONFactoryComponent } from './qualityinspectionposting/IDD_POSTING_QUALITYINSPECTION.component';
import { IDD_DELETE_QUALITYINSPECTIONComponent, IDD_DELETE_QUALITYINSPECTIONFactoryComponent } from './qualityinspectiondeleting/IDD_DELETE_QUALITYINSPECTION.component';
import { IDD_NONCONFREASONComponent, IDD_NONCONFREASONFactoryComponent } from './nonconformityreason/IDD_NONCONFREASON.component';
import { IDD_LOAD_INSPECTION_ORDERComponent, IDD_LOAD_INSPECTION_ORDERFactoryComponent } from './inspectionorderloading/IDD_LOAD_INSPECTION_ORDER.component';
import { IDD_INSPECTION_ORDERComponent, IDD_INSPECTION_ORDERFactoryComponent } from './inspectionorder/IDD_INSPECTION_ORDER.component';
import { IDD_INSPECTION_NOTESComponent, IDD_INSPECTION_NOTESFactoryComponent } from './inspectionnotes/IDD_INSPECTION_NOTES.component';
import { IDD_ANALYSISPARAMETERSComponent, IDD_ANALYSISPARAMETERSFactoryComponent } from './analysisparameters/IDD_ANALYSISPARAMETERS.component';
import { IDD_ANALYSISMETHODComponent, IDD_ANALYSISMETHODFactoryComponent } from './analysismethod/IDD_ANALYSISMETHOD.component';
import { IDD_ANALYSISAREAComponent, IDD_ANALYSISAREAFactoryComponent } from './analysisarea/IDD_ANALYSISAREA.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TECHNICAL_DATA_DEFINITION', component: IDD_TECHNICAL_DATA_DEFINITIONFactoryComponent },
            { path: 'IDD_RESULTS', component: IDD_RESULTSFactoryComponent },
            { path: 'IDD_POSTING_QUALITYINSPECTION', component: IDD_POSTING_QUALITYINSPECTIONFactoryComponent },
            { path: 'IDD_DELETE_QUALITYINSPECTION', component: IDD_DELETE_QUALITYINSPECTIONFactoryComponent },
            { path: 'IDD_NONCONFREASON', component: IDD_NONCONFREASONFactoryComponent },
            { path: 'IDD_LOAD_INSPECTION_ORDER', component: IDD_LOAD_INSPECTION_ORDERFactoryComponent },
            { path: 'IDD_INSPECTION_ORDER', component: IDD_INSPECTION_ORDERFactoryComponent },
            { path: 'IDD_INSPECTION_NOTES', component: IDD_INSPECTION_NOTESFactoryComponent },
            { path: 'IDD_ANALYSISPARAMETERS', component: IDD_ANALYSISPARAMETERSFactoryComponent },
            { path: 'IDD_ANALYSISMETHOD', component: IDD_ANALYSISMETHODFactoryComponent },
            { path: 'IDD_ANALYSISAREA', component: IDD_ANALYSISAREAFactoryComponent },
        ])],
    declarations: [
            IDD_TECHNICAL_DATA_DEFINITIONComponent, IDD_TECHNICAL_DATA_DEFINITIONFactoryComponent,
            IDD_RESULTSComponent, IDD_RESULTSFactoryComponent,
            IDD_POSTING_QUALITYINSPECTIONComponent, IDD_POSTING_QUALITYINSPECTIONFactoryComponent,
            IDD_DELETE_QUALITYINSPECTIONComponent, IDD_DELETE_QUALITYINSPECTIONFactoryComponent,
            IDD_NONCONFREASONComponent, IDD_NONCONFREASONFactoryComponent,
            IDD_LOAD_INSPECTION_ORDERComponent, IDD_LOAD_INSPECTION_ORDERFactoryComponent,
            IDD_INSPECTION_ORDERComponent, IDD_INSPECTION_ORDERFactoryComponent,
            IDD_INSPECTION_NOTESComponent, IDD_INSPECTION_NOTESFactoryComponent,
            IDD_ANALYSISPARAMETERSComponent, IDD_ANALYSISPARAMETERSFactoryComponent,
            IDD_ANALYSISMETHODComponent, IDD_ANALYSISMETHODFactoryComponent,
            IDD_ANALYSISAREAComponent, IDD_ANALYSISAREAFactoryComponent,
    ],
    exports: [
            IDD_TECHNICAL_DATA_DEFINITIONFactoryComponent,
            IDD_RESULTSFactoryComponent,
            IDD_POSTING_QUALITYINSPECTIONFactoryComponent,
            IDD_DELETE_QUALITYINSPECTIONFactoryComponent,
            IDD_NONCONFREASONFactoryComponent,
            IDD_LOAD_INSPECTION_ORDERFactoryComponent,
            IDD_INSPECTION_ORDERFactoryComponent,
            IDD_INSPECTION_NOTESFactoryComponent,
            IDD_ANALYSISPARAMETERSFactoryComponent,
            IDD_ANALYSISMETHODFactoryComponent,
            IDD_ANALYSISAREAFactoryComponent,
    ],
    entryComponents: [
            IDD_TECHNICAL_DATA_DEFINITIONComponent,
            IDD_RESULTSComponent,
            IDD_POSTING_QUALITYINSPECTIONComponent,
            IDD_DELETE_QUALITYINSPECTIONComponent,
            IDD_NONCONFREASONComponent,
            IDD_LOAD_INSPECTION_ORDERComponent,
            IDD_INSPECTION_ORDERComponent,
            IDD_INSPECTION_NOTESComponent,
            IDD_ANALYSISPARAMETERSComponent,
            IDD_ANALYSISMETHODComponent,
            IDD_ANALYSISAREAComponent,
    ]
})


export class QualityInspectionModule { };