import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { ESStandaloneComponent } from '@taskbuilder/easystudio';

import { environment } from './../environments/environment';

import { CoreGuard, LoginComponent, PageNotFoundComponent } from '@taskbuilder/core';

export const routing: ModuleWithProviders = RouterModule.forRoot([
  { path: '', component: ESStandaloneComponent, canActivate: [CoreGuard] },
  { path: 'login', component: LoginComponent },
  { path: '**', component: PageNotFoundComponent },
]);

