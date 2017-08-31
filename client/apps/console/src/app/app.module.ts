import { ModelService } from './services/model.service';
import { AccountComponent } from './components/account/account.component';
import { AppComponent } from './app.component';
import { AppHomeComponent } from './components/app-home/app-home.component';
import { AuthGuardService } from './guards/auth-guard.service';
import { BrowserModule } from '@angular/platform-browser';

import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { JsonVisualizerPipe } from './json-visualizer.pipe';
import { LoginComponent } from './components/login/login.component';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SubscriptionHomeComponent } from './components/subscription/subscription-home.component';
import { routes } from './app.routes';
import { ConsoleModule } from '@taskbuilder/console';
import { LoginService } from './services/login.service';
import { DatabaseInfoComponent } from './components/database-info/database-info.component';
import { InstanceComponent } from './components/instance/instance.component';
import { BreadcrumbComponent } from './components/shared/breadcrumb.component';
import { EntityHomeComponent } from './components/entity/entity-home.component';
import { EntityDetailComponent } from './components/entity/entity-detail.component';
import { SubscriptionSelectionComponent } from './components/subscription/subscription-selection.component';
import { AccountsHomeComponent } from './components/account/accounts-home.component';
import { InstanceHomeComponent } from './components/instance/instance-home.component';
import { SubscriptionComponent } from './components/subscription/subscription.component';
import { AccountSubscriptionsComponent } from './components/account/account-subscriptions.component';
import { SubscriptionDatabasesComponent } from './components/subscription/subscription-databases.component';
//import { BrowserAnimationsModule } from '@angular/platform-browser/animations';

@NgModule({
  declarations: [
    AppComponent,
    JsonVisualizerPipe,
    SubscriptionHomeComponent,
    AppHomeComponent,
    LoginComponent,
    AccountComponent,
    DatabaseInfoComponent,
    InstanceComponent,
    BreadcrumbComponent,
    EntityHomeComponent,
    EntityDetailComponent,
    SubscriptionSelectionComponent,
    AccountsHomeComponent,
    InstanceHomeComponent,
    SubscriptionComponent,
    AccountSubscriptionsComponent,
    SubscriptionDatabasesComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    RouterModule.forRoot(routes),
    HttpModule,
    ConsoleModule
    //BrowserAnimationsModule
  ],
  providers: [AuthGuardService, LoginService, ModelService],
  bootstrap: [AppComponent]
})
export class AppModule { }