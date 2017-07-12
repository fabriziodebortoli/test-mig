import { ModuleWithProviders } from '@angular/core';
import { RouterModule } from '@angular/router';

import { TestComponent } from './test/test.component';

import { PageNotFoundComponent } from '@taskbuilder/core';
import { HomeComponent } from "@taskbuilder/core";

export const routing: ModuleWithProviders = RouterModule.forRoot([
  { path: '', component: HomeComponent },
  { path: 'home', component: HomeComponent },
  { path: '**', component: PageNotFoundComponent },
]);

