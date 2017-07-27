import {InstanceComponent} from './components/instance/instance.component';
import { AccountComponent } from "app/components/account/account.component";
import { AppComponent } from 'app/app.component';
import { AppHomeComponent } from 'app/components/app-home/app-home.component';
import { AuthGuardService } from 'app/guards/auth-guard.service';
import { CompanyComponent } from "app/components/company/company.component";
import { LoginComponent } from "app/components/login/login.component";
import { Routes } from '@angular/router';
import { SubscriptionHomeComponent } from 'app/components/subscription/subscription-home.component';
import { AccountsHomeComponent } from "app/components/account/accounts-home.component";
import { DatabasesHomeComponent } from "app/components/databases-home/databases-home.component";

export const routes: Routes = [
  { 
    path: '', 
    component: AppComponent 
  },
  { 
    path: 'loginComponent', 
    component: LoginComponent 
  },
  { 
    path: 'appHome',
    component: AppHomeComponent 
  },
  { 
    path: 'instancesHome',
    component: InstanceComponent,
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService]    
  },
  { 
    path: 'subscriptionsHome', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: SubscriptionHomeComponent
  },
  { 
    path: 'accountsHome', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: AccountsHomeComponent
  },
  { 
    path: 'databasesHome', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: DatabasesHomeComponent,
  },
  { 
    path: 'account', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: AccountComponent,
  },  
  { 
    path: 'logout', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: AppHomeComponent
  }
];