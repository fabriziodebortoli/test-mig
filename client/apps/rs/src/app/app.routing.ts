import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { ReportingStudioFactoryComponent } from '@taskbuilder/reporting-studio';

import { environment } from './../environments/environment';

import { CoreGuard, LoginComponent, PageNotFoundComponent, ProxyRouteComponent } from '@taskbuilder/core';
import { HomeComponent } from '@taskbuilder/menu';

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

