import { IDD_DETYPOLOGY_FULLComponent, IDD_DETYPOLOGY_FULLFactoryComponent } from './packagetypes/IDD_DETYPOLOGY_FULL.component';
import { IDD_BATASSOCIAComponent, IDD_BATASSOCIAFactoryComponent } from './itemsmaterialsassociation/IDD_BATASSOCIA.component';
import { IDD_DEITMCOMP_FULLComponent, IDD_DEITMCOMP_FULLFactoryComponent } from './itemsmaterials/IDD_DEITMCOMP_FULL.component';
import { IDD_DECLICOMP_FULLComponent, IDD_DECLICOMP_FULLFactoryComponent } from './custmaterialsexemption/IDD_DECLICOMP_FULL.component';
import { IDD_CONAI_SETTINGSComponent, IDD_CONAI_SETTINGSFactoryComponent } from './conaisettings/IDD_CONAI_SETTINGS.component';
import { IDD_PARAMETERS_CONAI_FULLComponent, IDD_PARAMETERS_CONAI_FULLFactoryComponent } from './conaiparameters/IDD_PARAMETERS_CONAI_FULL.component';
import { IDD_CONAIENTRY_FULLComponent, IDD_CONAIENTRY_FULLFactoryComponent } from './conaientries/IDD_CONAIENTRY_FULL.component';
import { IDD_CONAI_CALCULATIONComponent, IDD_CONAI_CALCULATIONFactoryComponent } from './conaicalculation/IDD_CONAI_CALCULATION.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_DETYPOLOGY_FULL', component: IDD_DETYPOLOGY_FULLFactoryComponent },
            { path: 'IDD_BATASSOCIA', component: IDD_BATASSOCIAFactoryComponent },
            { path: 'IDD_DEITMCOMP_FULL', component: IDD_DEITMCOMP_FULLFactoryComponent },
            { path: 'IDD_DECLICOMP_FULL', component: IDD_DECLICOMP_FULLFactoryComponent },
            { path: 'IDD_CONAI_SETTINGS', component: IDD_CONAI_SETTINGSFactoryComponent },
            { path: 'IDD_PARAMETERS_CONAI_FULL', component: IDD_PARAMETERS_CONAI_FULLFactoryComponent },
            { path: 'IDD_CONAIENTRY_FULL', component: IDD_CONAIENTRY_FULLFactoryComponent },
            { path: 'IDD_CONAI_CALCULATION', component: IDD_CONAI_CALCULATIONFactoryComponent },
        ])],
    declarations: [
            IDD_DETYPOLOGY_FULLComponent, IDD_DETYPOLOGY_FULLFactoryComponent,
            IDD_BATASSOCIAComponent, IDD_BATASSOCIAFactoryComponent,
            IDD_DEITMCOMP_FULLComponent, IDD_DEITMCOMP_FULLFactoryComponent,
            IDD_DECLICOMP_FULLComponent, IDD_DECLICOMP_FULLFactoryComponent,
            IDD_CONAI_SETTINGSComponent, IDD_CONAI_SETTINGSFactoryComponent,
            IDD_PARAMETERS_CONAI_FULLComponent, IDD_PARAMETERS_CONAI_FULLFactoryComponent,
            IDD_CONAIENTRY_FULLComponent, IDD_CONAIENTRY_FULLFactoryComponent,
            IDD_CONAI_CALCULATIONComponent, IDD_CONAI_CALCULATIONFactoryComponent,
    ],
    exports: [
            IDD_DETYPOLOGY_FULLFactoryComponent,
            IDD_BATASSOCIAFactoryComponent,
            IDD_DEITMCOMP_FULLFactoryComponent,
            IDD_DECLICOMP_FULLFactoryComponent,
            IDD_CONAI_SETTINGSFactoryComponent,
            IDD_PARAMETERS_CONAI_FULLFactoryComponent,
            IDD_CONAIENTRY_FULLFactoryComponent,
            IDD_CONAI_CALCULATIONFactoryComponent,
    ],
    entryComponents: [
            IDD_DETYPOLOGY_FULLComponent,
            IDD_BATASSOCIAComponent,
            IDD_DEITMCOMP_FULLComponent,
            IDD_DECLICOMP_FULLComponent,
            IDD_CONAI_SETTINGSComponent,
            IDD_PARAMETERS_CONAI_FULLComponent,
            IDD_CONAIENTRY_FULLComponent,
            IDD_CONAI_CALCULATIONComponent,
    ]
})


export class ConaiModule { };