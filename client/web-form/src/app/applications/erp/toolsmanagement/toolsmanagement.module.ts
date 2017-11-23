import { IDD_TOOLS_REGENERATION_SET_DATAComponent, IDD_TOOLS_REGENERATION_SET_DATAFactoryComponent } from './toolsregeneration/IDD_TOOLS_REGENERATION_SET_DATA.component';
import { IDD_TOOLS_REGENERATIONComponent, IDD_TOOLS_REGENERATIONFactoryComponent } from './toolsregeneration/IDD_TOOLS_REGENERATION.component';
import { IDD_TOOLS_MANAGEMENTComponent, IDD_TOOLS_MANAGEMENTFactoryComponent } from './toolsmanagement/IDD_TOOLS_MANAGEMENT.component';
import { IDD_TOOLS_INSPECTION_SET_DATAComponent, IDD_TOOLS_INSPECTION_SET_DATAFactoryComponent } from './toolsinspection/IDD_TOOLS_INSPECTION_SET_DATA.component';
import { IDD_TOOL_INSPECTIONComponent, IDD_TOOL_INSPECTIONFactoryComponent } from './toolsinspection/IDD_TOOL_INSPECTION.component';
import { IDD_TOOLS_FAMILIESComponent, IDD_TOOLS_FAMILIESFactoryComponent } from './toolsfamilies/IDD_TOOLS_FAMILIES.component';
import { IDD_TOOLS_COPYComponent, IDD_TOOLS_COPYFactoryComponent } from './toolscopy/IDD_TOOLS_COPY.component';
import { IDD_TOOLS_ANALISYSComponent, IDD_TOOLS_ANALISYSFactoryComponent } from './toolsanalysis/IDD_TOOLS_ANALISYS.component';
import { IDD_TOOLSComponent, IDD_TOOLSFactoryComponent } from './tools/IDD_TOOLS.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_TOOLS_REGENERATION_SET_DATA', component: IDD_TOOLS_REGENERATION_SET_DATAFactoryComponent },
            { path: 'IDD_TOOLS_REGENERATION', component: IDD_TOOLS_REGENERATIONFactoryComponent },
            { path: 'IDD_TOOLS_MANAGEMENT', component: IDD_TOOLS_MANAGEMENTFactoryComponent },
            { path: 'IDD_TOOLS_INSPECTION_SET_DATA', component: IDD_TOOLS_INSPECTION_SET_DATAFactoryComponent },
            { path: 'IDD_TOOL_INSPECTION', component: IDD_TOOL_INSPECTIONFactoryComponent },
            { path: 'IDD_TOOLS_FAMILIES', component: IDD_TOOLS_FAMILIESFactoryComponent },
            { path: 'IDD_TOOLS_COPY', component: IDD_TOOLS_COPYFactoryComponent },
            { path: 'IDD_TOOLS_ANALISYS', component: IDD_TOOLS_ANALISYSFactoryComponent },
            { path: 'IDD_TOOLS', component: IDD_TOOLSFactoryComponent },
        ])],
    declarations: [
            IDD_TOOLS_REGENERATION_SET_DATAComponent, IDD_TOOLS_REGENERATION_SET_DATAFactoryComponent,
            IDD_TOOLS_REGENERATIONComponent, IDD_TOOLS_REGENERATIONFactoryComponent,
            IDD_TOOLS_MANAGEMENTComponent, IDD_TOOLS_MANAGEMENTFactoryComponent,
            IDD_TOOLS_INSPECTION_SET_DATAComponent, IDD_TOOLS_INSPECTION_SET_DATAFactoryComponent,
            IDD_TOOL_INSPECTIONComponent, IDD_TOOL_INSPECTIONFactoryComponent,
            IDD_TOOLS_FAMILIESComponent, IDD_TOOLS_FAMILIESFactoryComponent,
            IDD_TOOLS_COPYComponent, IDD_TOOLS_COPYFactoryComponent,
            IDD_TOOLS_ANALISYSComponent, IDD_TOOLS_ANALISYSFactoryComponent,
            IDD_TOOLSComponent, IDD_TOOLSFactoryComponent,
    ],
    exports: [
            IDD_TOOLS_REGENERATION_SET_DATAFactoryComponent,
            IDD_TOOLS_REGENERATIONFactoryComponent,
            IDD_TOOLS_MANAGEMENTFactoryComponent,
            IDD_TOOLS_INSPECTION_SET_DATAFactoryComponent,
            IDD_TOOL_INSPECTIONFactoryComponent,
            IDD_TOOLS_FAMILIESFactoryComponent,
            IDD_TOOLS_COPYFactoryComponent,
            IDD_TOOLS_ANALISYSFactoryComponent,
            IDD_TOOLSFactoryComponent,
    ],
    entryComponents: [
            IDD_TOOLS_REGENERATION_SET_DATAComponent,
            IDD_TOOLS_REGENERATIONComponent,
            IDD_TOOLS_MANAGEMENTComponent,
            IDD_TOOLS_INSPECTION_SET_DATAComponent,
            IDD_TOOL_INSPECTIONComponent,
            IDD_TOOLS_FAMILIESComponent,
            IDD_TOOLS_COPYComponent,
            IDD_TOOLS_ANALISYSComponent,
            IDD_TOOLSComponent,
    ]
})


export class ToolsManagementModule { };