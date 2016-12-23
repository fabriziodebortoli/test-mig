import { LoginComponent } from './menu/components/login/login.component';
import { PageNotFoundComponent } from 'tb-shared';
import { HomeComponent } from './home.component';
import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

export const appRoutes: Routes = [
    { path: '', component: LoginComponent },
    { path: 'login', component: LoginComponent },
    { path: 'home', component: HomeComponent },
    { path: 'ERP/Languages', loadChildren: 'app/applications/ERP/Languages/languages.module#LanguagesModule', outlet: 'dynamic' },
    { path: 'menu', loadChildren: 'app/menu/menu.module#MenuModule' },
    { path: '**', component: PageNotFoundComponent },

];

export const routing: ModuleWithProviders = RouterModule.forRoot(appRoutes);
