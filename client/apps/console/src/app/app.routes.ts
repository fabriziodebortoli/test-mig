import { AccountComponent } from "app/components/account/account.component";
import { AppComponent } from 'app/app.component';
import { AppHomeComponent } from 'app/components/app-home/app-home.component';
import { AuthGuardService } from 'app/guards/auth-guard.service';
import { CompanyAccountComponent } from "app/components/company-account/company-account.component";
import { CompanyComponent } from "app/components/company/company.component";
import { EntityListComponent } from "app/components/entity-list/entity-list.component";
import { LoginComponent } from "app/components/login/login.component";
import { Routes } from '@angular/router';
import { SubscriptionHomeComponent } from 'app/components/subscription-home/subscription-home.component';
import { AccountListComponent } from './components/account/account-list/account-list.component';

export const routes: Routes = [
  { path: '', component: AppComponent },
  { path: 'loginComponent', component: LoginComponent },
  { 
    path: 'appHome', 
    // canActivate: [AuthGuardService],
    // canActivateChild: [AuthGuardService],    
    component: AppHomeComponent 
  },
  { 
    path: 'subscriptionHome', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: SubscriptionHomeComponent,
  },
  { 
    path: 'entityList', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: EntityListComponent,
  },
  { 
    path: 'company', 
    // canActivate: [AuthGuardService],
    // canActivateChild: [AuthGuardService],
    component: CompanyComponent,
  },
  { 
    path: 'account', 
    // canActivate: [AuthGuardService],
    // canActivateChild: [AuthGuardService],
    component: //AccountListComponent 
    AccountComponent
  },
  { 
    path: 'company-account', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: CompanyAccountComponent,
  }
];