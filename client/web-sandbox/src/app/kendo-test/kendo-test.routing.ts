import { ModuleWithProviders } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { KendoPageComponent } from './kendo-page/kendo-page.component';

const routes: Routes = [
  { path: '', component: KendoPageComponent }
];

export const routing: ModuleWithProviders = RouterModule.forChild(routes);
