import { UnsupportedFactoryComponent } from './unsupported.component';
import { LoginComponent } from './menu/components/login/login.component';
import { PageNotFoundComponent } from 'tb-shared';
import { HomeComponent } from './home/home.component';
import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { CoreGuard } from './core/core.guard';


export const appRoutes: Routes = [
    { path: '', component: HomeComponent, canActivate: [CoreGuard] },
    { path: 'login', component: LoginComponent },
    { path: 'home', component: HomeComponent },
    { path: 'Framework/TbGes/Unsupported', component: UnsupportedFactoryComponent, outlet: 'dynamic' },
    { path: 'ERP/Languages', loadChildren: 'app/applications/ERP/Languages/languages.module#LanguagesModule', outlet: 'dynamic' },
    { path: 'menu', loadChildren: 'app/menu/menu.module#MenuModule' },
    { path: '**', component: PageNotFoundComponent },

];

export const routing: ModuleWithProviders = RouterModule.forRoot(appRoutes);
