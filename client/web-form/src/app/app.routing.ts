import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { BPMStandaloneComponent } from '@taskbuilder/bpm';

import { environment } from './../environments/environment';

import {
  CoreGuard,
  LoginComponent,
  PageNotFoundComponent,
  UnsupportedFactoryComponent,
  ProxyRouteComponent,
  HomeComponent,
  StandaloneDocumentComponent,
  StandaloneReportComponent
} from '@taskbuilder/core';
import { ReportingStudioFactoryComponent } from '@taskbuilder/reporting-studio';

import { appRoutes } from './applications/app.routing';

let webOnlyRoutes = [
  { path: 'rs', loadChildren: '@taskbuilder/reporting-studio#ReportingStudioModule' },
  { path: 'test', loadChildren: '@taskbuilder/core#TbTestModule' },
  { path: 'framework/tbges/IDD_Unsupported', component: UnsupportedFactoryComponent },
  ...appRoutes
];
let allEnvRoutes = [
  { path: 'bpm', loadChildren: '@taskbuilder/bpm#BPMModule' }
];
let childrenRoutes = environment.desktop ? [...allEnvRoutes] : [...allEnvRoutes, ...webOnlyRoutes];

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

