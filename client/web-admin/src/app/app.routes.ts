import { Routes } from '@angular/router';

import { SubscriptionHomeComponent } from 'app/subscription-home/subscription-home.component';
import { AppHomeComponent } from 'app/app-home/app-home.component';
import { AppComponent } from "app/app.component";

export const routes: Routes = [
  { path: '', component: AppComponent },
  { path: 'appHome', component: AppHomeComponent },
  { path: 'subscriptionHome', component: SubscriptionHomeComponent }
];