import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';


import { environment } from './../environments/environment';

import { CoreGuard, LoginComponent, PageNotFoundComponent, ProxyRouteComponent } from '@taskbuilder/core';
import { HomeComponent } from '@taskbuilder/menu';
//import { StandaloneReportComponent } from '@taskbuilder/reporting-studio';





export const routing: ModuleWithProviders = RouterModule.forRoot([
  { path: '', component: HomeComponent, canActivate: [CoreGuard] },
  { path: 'login', component: LoginComponent },
 // { path: 'rs/:ns', component: StandaloneReportComponent },
  { path: '**', component: PageNotFoundComponent },
]);

