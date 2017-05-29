import { Routes } from '@angular/router';

import { SubscriptionHomeComponent } from 'app/subscription-home/subscription-home.component';
import { AppHomeComponent } from 'app/app-home/app-home.component';
import { AppComponent } from 'app/app.component';
import { AuthGuardService } from 'app/guards/auth-guard.service';
import { LoginComponent } from "app/login/login.component";
import { EntityListComponent } from "app/entity-list/entity-list.component";
import { CompanyComponent } from "app/company/company.component";
import { AccountComponent } from "app/account/account.component";
import { CompanyAccountComponent } from "app/company-account/company-account.component";

export const routes: Routes = [
  { path: '', component: AppComponent },
  { path: 'loginComponent', component: LoginComponent },
  { 
    path: 'appHome', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],    
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
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: CompanyComponent,
  },
  { 
    path: 'account', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: AccountComponent,
  },
  { 
    path: 'company-account', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: CompanyAccountComponent,
  }
];