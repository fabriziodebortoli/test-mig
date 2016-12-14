import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home.component';

const APP_ROUTES: Routes = [
  { path: '', component: HomeComponent },

  { path: 'report', loadChildren: 'app/reporting-studio/reporting-studio.module#ReportingStudioModule' },

  { path: '**', component: HomeComponent }
];

export const routing = RouterModule.forRoot(APP_ROUTES, { useHash: true });
