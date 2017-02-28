import { ReportingStudioComponent } from './reporting-studio.component';
import { Routes, RouterModule } from '@angular/router';

const RS_ROUTES: Routes = [
    {
        path: 'rs', component: ReportingStudioComponent/*, children: [
            { path: '', component: ReportingStudioHostComponent },
            { path: ':namespace', component: ReportingStudioComponent }
        ]*/
    }
];

export const rsRouting = RouterModule.forChild(RS_ROUTES);