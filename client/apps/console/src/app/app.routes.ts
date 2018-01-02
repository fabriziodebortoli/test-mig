import { DatabaseSummaryComponent } from './components/database/database-summary.component';
import { FileUploadComponent } from './components/file-upload/file-upload.component';
import { DatabaseConfigurationComponent } from './components/database/database-configuration.component';
import { SubscriptionDatabaseComponent } from './components/subscription/subscription-database.component';
import { InstanceHomeComponent } from './components/instance/instance-home.component';
import { InstanceComponent } from './components/instance/instance.component';
import { AccountComponent } from "app/components/account/account.component";
import { AppComponent } from 'app/app.component';
import { AppHomeComponent } from 'app/components/app-home/app-home.component';
import { AuthGuardService } from 'app/guards/auth-guard.service';
import { LoginComponent } from "app/components/login/login.component";
import { Routes } from '@angular/router';
import { SubscriptionHomeComponent } from 'app/components/subscription/subscription-home.component';
import { AccountsHomeComponent } from "app/components/account/accounts-home.component";
import { SubscriptionComponent } from 'app/components/subscription/subscription.component';
import { SubscriptionDbHomeComponent } from 'app/components/subscription/subscription-db-home.component';
import { TestControlsComponent } from 'app/components/test-controls/test-controls.component';
import { InstanceRegistrationComponent } from 'app/components/instance/instance-registration.component';
import { SubscriptionExternalSourceComponent } from 'app/components/external-source/subscription-external-source.component';

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
    path: 'instancesRegistration',
    component: InstanceRegistrationComponent
  },  
  { 
    path: 'instancesHome',
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: InstanceHomeComponent
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
    path: 'database', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: SubscriptionDbHomeComponent
  },
  { 
    path: 'account', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: AccountComponent
  },  
  { 
    path: 'instance', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: InstanceComponent
  },  
  { 
    path: 'subscription', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: SubscriptionComponent
  },  
  { 
    path: 'database/configuration', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: DatabaseConfigurationComponent
  },
  { 
    path: 'database/summary', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: DatabaseSummaryComponent
  },
  { 
    path: 'externalsource', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: SubscriptionExternalSourceComponent
  },  
  { 
    path: 'fileUpload', 
    component: FileUploadComponent
  },
  { 
    path: 'testControls',
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],    
    component: TestControlsComponent
  },  
  { 
    path: 'logout', 
    canActivate: [AuthGuardService],
    canActivateChild: [AuthGuardService],
    component: AppHomeComponent
  }
];