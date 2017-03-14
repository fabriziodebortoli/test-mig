import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { CoreGuard } from './core/core.guard';

import { HomeComponent } from './home/home.component';
import { PageNotFoundComponent } from './shared/page-not-found/page-not-found.component';
import { LoginComponent } from './shared/login/login.component';
import { LogoutComponent } from './shared/logout/logout.component';

export const routing: ModuleWithProviders = RouterModule.forRoot([
  { path: '', component: HomeComponent, canActivate: [CoreGuard] },
  { path: 'login', component: LoginComponent },
  { path: 'logout', component: LogoutComponent },
  { path: 'kendo', loadChildren: './kendo-test/kendo-test.module#KendoTestModule', canActivate: [CoreGuard] },
  { path: '**', component: PageNotFoundComponent }
]);
