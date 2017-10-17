import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { environment } from './../environments/environment';

import {
  CoreGuard,
  LoginComponent,
  PageNotFoundComponent,
  UnsupportedFactoryComponent,
  ProxyRouteComponent,
  HomeComponent,
  StandaloneDocumentComponent,
  // StandaloneReportComponent
} from '@taskbuilder/core';
// import { ReportingStudioFactoryComponent } from '@taskbuilder/reporting-studio';

// import { RsTestComponent } from '@taskbuilder/reporting-studio';
import { appRoutes } from './applications/app.routing';

let magoRoutes = [
  // { path: 'rs', loadChildren: '@taskbuilder/reporting-studio#ReportingStudioModule' },
  // { path: 'settings', loadChildren: '@taskbuilder/core#TbSettingsModule' },
  // { path: 'test', loadChildren: '@taskbuilder/core#TbTestModule' },
  { path: 'framework/tbges/IDD_Unsupported', component: UnsupportedFactoryComponent },
  ...appRoutes
];
let childrenRoutes = environment.desktop ? [] : [...magoRoutes];

export const routing: ModuleWithProviders = RouterModule.forRoot([
  { path: '', component: HomeComponent, canActivate: [CoreGuard] },
  { path: 'login', component: LoginComponent },
  { path: 'home', component: HomeComponent, canActivate: [CoreGuard] },
  { path: 'document/:ns', component: StandaloneDocumentComponent, canActivate: [CoreGuard] },
  // { path: 'rs/:ns', component: StandaloneReportComponent },
  {
    path: 'proxy',
    outlet: 'dynamic',
    component: ProxyRouteComponent,
    children: [...childrenRoutes],
  },
  { path: '**', component: PageNotFoundComponent },
]);

