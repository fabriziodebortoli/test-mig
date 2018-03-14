import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { ReportingStudioFactoryComponent } from '@taskbuilder/reporting-studio';

import { environment } from './../environments/environment';

import { CoreGuard, LoginComponent, PageNotFoundComponent } from '@taskbuilder/core';

export const routing: ModuleWithProviders = RouterModule.forRoot([
  { path: '', component: ReportingStudioFactoryComponent, canActivate: [CoreGuard] },
  { path: 'login', component: LoginComponent },
  { path: '**', component: PageNotFoundComponent },
]);

