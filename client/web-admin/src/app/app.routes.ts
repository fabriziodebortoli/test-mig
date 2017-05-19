import { Routes } from '@angular/router';

import { SubscriptionHomeComponent } from 'app/subscription-home/subscription-home.component';
import { AppHomeComponent } from 'app/app-home/app-home.component';
import { AppComponent } from 'app/app.component';
import { AuthGuardService } from 'app/guards/auth-guard.service';
import { LoginComponent } from "app/login/login.component";

export const routes: Routes = [
  { path: '', component: AppComponent },
  { path: 'loginComponent', component: LoginComponent },
  { path: 'appHome', component: AppHomeComponent },
  { 
    path: 'subscriptionHome', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: SubscriptionHomeComponent,
  }
];