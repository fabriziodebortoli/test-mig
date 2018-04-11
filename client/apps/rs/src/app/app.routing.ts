import { RsHomeComponent } from './rs-home/rs-home.component';
import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { environment } from './../environments/environment';

import { CoreGuard, LoginComponent, PageNotFoundComponent, ProxyRouteComponent, LogoutComponent } from '@taskbuilder/core';
import { StandaloneReportComponent, ReportingStudioComponent } from '@taskbuilder/reporting-studio';

export const routing: ModuleWithProviders = RouterModule.forRoot([
  { path: '', component: RsHomeComponent, canActivate: [CoreGuard] },
  { path: 'login', component: LoginComponent },
  { path: 'rs/:ns', component: ReportingStudioComponent, canActivate: [CoreGuard] },
  { path: 'logout', component: LogoutComponent },
  { path: '**', component: PageNotFoundComponent },
]);

