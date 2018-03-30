import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { environment } from './../environments/environment';

import {
  CoreGuard,
  LoginComponent,
  PageNotFoundComponent,
  UnsupportedFactoryComponent,
  ProxyRouteComponent
} from '@taskbuilder/core';

import { HomeComponent, StandaloneDocumentComponent, StandaloneReportComponent } from '@taskbuilder/menu';

import { ReportingStudioFactoryComponent } from '@taskbuilder/reporting-studio';

import { appRoutes } from './applications/app.routing';

const webOnlyRoutes = [
  { path: 'rs', loadChildren: '@taskbuilder/reporting-studio#ReportingStudioModule' },
  { path: 'framework/tbges/IDD_Unsupported', component: UnsupportedFactoryComponent },
  { path: 'layout', loadChildren: 'app/layout/layout.module#LayoutModule' },
  ...appRoutes
];
const allEnvRoutes = [];
const childrenRoutes = environment.desktop ? [...allEnvRoutes] : [...allEnvRoutes, ...webOnlyRoutes];

export const routing: ModuleWithProviders = RouterModule.forRoot([
  { path: '', component: HomeComponent, canActivate: [CoreGuard] },
  { path: 'login', component: LoginComponent },
  {
    path: 'proxy',
    outlet: 'dynamic',
    component: ProxyRouteComponent,
    children: [
      ...childrenRoutes
    ],
  },
  { path: '**', component: PageNotFoundComponent }
]);

