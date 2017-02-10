import { ReportingStudioComponent } from './reporting-studio.component';
import { ReportingStudioHostComponent } from './reporting-studio-host.component';
import { Routes, RouterModule } from '@angular/router';

const RS_ROUTES: Routes = [
    {
        path: '', component: ReportingStudioHostComponent, children: [
            { path: '', component: ReportingStudioHostComponent },
            { path: ':namespace', component: ReportingStudioComponent }
        ]
    }
];

export const rsRouting = RouterModule.forChild(RS_ROUTES);