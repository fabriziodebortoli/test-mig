import { Routes, RouterModule } from '@angular/router';
import { HomeComponent } from './home.component';

const APP_ROUTES: Routes = [
  { path: '', component: HomeComponent },

  { path: 'report', loadChildren: 'app/report-studio/report-studio.module#ReportStudioModule' },

  { path: '**', component: HomeComponent }
];

export const routing = RouterModule.forRoot(APP_ROUTES, { useHash: true });
