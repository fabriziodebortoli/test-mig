import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { RsComponent } from '@taskbuilder/bpm';

import { environment } from './../environments/environment';

import { CoreGuard, LoginComponent, PageNotFoundComponent } from '@taskbuilder/core';

export const routing: ModuleWithProviders = RouterModule.forRoot([
  { path: '', component: BPMStandaloneComponent, canActivate: [CoreGuard] },
  { path: 'login', component: LoginComponent },
  { path: '**', component: PageNotFoundComponent },
]);

