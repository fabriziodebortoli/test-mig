import { IDD_JOBSPARAMComponent, IDD_JOBSPARAMFactoryComponent } from './jobsparameters/IDD_JOBSPARAM.component';
import { IDD_JOBSComponent, IDD_JOBSFactoryComponent } from './jobs/IDD_JOBS.component';
import { IDD_JOBSGROUPComponent, IDD_JOBSGROUPFactoryComponent } from './jobgroups/IDD_JOBSGROUP.component';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';
import { NgModule } from '@angular/core';
import { SharedModule } from './../../../shared/shared.module';

@NgModule({
    imports: [
        SharedModule,
        CommonModule,
        RouterModule.forChild([
            { path: 'IDD_JOBSPARAM', component: IDD_JOBSPARAMFactoryComponent },
            { path: 'IDD_JOBS', component: IDD_JOBSFactoryComponent },
            { path: 'IDD_JOBSGROUP', component: IDD_JOBSGROUPFactoryComponent },
        ])],
    declarations: [
            IDD_JOBSPARAMComponent, IDD_JOBSPARAMFactoryComponent,
            IDD_JOBSComponent, IDD_JOBSFactoryComponent,
            IDD_JOBSGROUPComponent, IDD_JOBSGROUPFactoryComponent,
    ],
    exports: [
            IDD_JOBSPARAMFactoryComponent,
            IDD_JOBSFactoryComponent,
            IDD_JOBSGROUPFactoryComponent,
    ],
    entryComponents: [
            IDD_JOBSPARAMComponent,
            IDD_JOBSComponent,
            IDD_JOBSGROUPComponent,
    ]
})


export class JobsModule { };