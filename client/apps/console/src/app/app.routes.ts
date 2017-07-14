import { AccountComponent } from "app/components/account/account.component";
import { AppComponent } from 'app/app.component';
import { AppHomeComponent } from 'app/components/app-home/app-home.component';
import { AuthGuardService } from 'app/guards/auth-guard.service';
import { CompanyComponent } from "app/components/company/company.component";
import { LoginComponent } from "app/components/login/login.component";
import { Routes } from '@angular/router';
import { SubscriptionHomeComponent } from 'app/components/subscription/subscription-home.component';

export const routes: Routes = [
  { path: '', component: AppComponent },
  { path: 'loginComponent', component: LoginComponent },
  { 
    path: 'appHome',
    component: AppHomeComponent 
  },
  { 
    path: 'subscriptionHome', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: SubscriptionHomeComponent,
  },
  { 
    path: 'company', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: CompanyComponent,
  },
  { 
    path: 'account', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: AccountComponent
  }
];