import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { environment } from './../environments/environment';

import {
    CoreGuard,
    LoginComponent,
    PageNotFoundComponent,
    UnsupportedFactoryComponent,
    HomeComponent
} from '@taskbuilder/core';

export const routing: ModuleWithProviders = RouterModule.forRoot([
    { path: '', component: HomeComponent, canActivate: [CoreGuard] },
    { path: 'login', component: LoginComponent },
    { path: '**', component: PageNotFoundComponent },
]);

