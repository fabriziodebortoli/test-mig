import { PageNotFoundComponent } from './core/components/page-not-found.component';
import { HomeComponent } from './home.component';
import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

export const appRoutes: Routes = [
    { path: '', component: HomeComponent },
    { path: '**', component: PageNotFoundComponent },
    { path: 'ERP/Languages', loadChildren: 'app/applications/ERP/Languages/languages.module#LanguagesModule', outlet: 'dynamic' }

];

export const appRoutingProviders: any[] = [];

export const routing: ModuleWithProviders = RouterModule.forRoot(appRoutes);
