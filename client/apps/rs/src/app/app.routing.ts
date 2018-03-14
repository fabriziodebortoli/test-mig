import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { ReportingStudioFactoryComponent } from '@taskbuilder/reporting-studio';

import { environment } from './../environments/environment';

import { CoreGuard, LoginComponent, PageNotFoundComponent, HomeComponent, ProxyRouteComponent } from '@taskbuilder/core';

export const routing: ModuleWithProviders = RouterModule.forRoot([
  { path: '', component: HomeComponent, canActivate: [CoreGuard] },
  { path: 'login', component: LoginComponent },
  {
    path: 'proxy',
    outlet: 'dynamic',
    component: ProxyRouteComponent,
    children: [
      { path: 'rs', loadChildren: '@taskbuilder/reporting-studio#ReportingStudioModule' },
    ],
  },
  { path: '**', component: PageNotFoundComponent },
]);

