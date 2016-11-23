import { PageNotFoundComponent } from 'tb-shared';
import { HomeComponent } from './home.component';
import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

export const appRoutes: Routes = [
    { path: '', component: HomeComponent },
    { path: '**', component: PageNotFoundComponent },
    { path: 'ERP/Languages', loadChildren: 'app/applications/ERP/Languages/languages.module#LanguagesModule', outlet: 'dynamic' }

];

export const routing: ModuleWithProviders = RouterModule.forRoot(appRoutes);
