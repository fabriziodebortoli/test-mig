import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

export const appRoutes: Routes = [
    { path: 'ERP/Languages', loadChildren: 'app/applications/ERP/Languages/languages.module#LanguagesModule', outlet: 'dynamic' },
];

export const appRouting: ModuleWithProviders = RouterModule.forRoot(appRoutes);
