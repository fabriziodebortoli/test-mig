import { ModelService } from './services/model.service';
import { BackendService } from './services/backend.service';
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
import { DatabaseInfoComponent } from './components/database/database-info.component';
import { InstanceComponent } from './components/instance/instance.component';
import { BreadcrumbComponent } from './components/shared/breadcrumb.component';
import { EntityHomeComponent } from './components/entity/entity-home.component';
import { EntityDetailComponent } from './components/entity/entity-detail.component';
import { AccountsHomeComponent } from './components/account/accounts-home.component';
import { InstanceHomeComponent } from './components/instance/instance-home.component';
import { SubscriptionComponent } from './components/subscription/subscription.component';
import { AccountSubscriptionsComponent } from './components/account/account-subscriptions.component';
import { SubscriptionDatabaseComponent } from './components/subscription/subscription-database.component';
import { DatabaseConfigurationComponent } from './components/database/database-configuration.component';
import { DatabaseTestconnectionComponent } from './components/database/database-testconnection.component';
import { DatabaseService } from './services/database.service';
import { UploadFileService } from './services/upload-file.service';
import { SubscriptionDbHomeComponent } from './components/subscription/subscription-db-home.component';
import { FileUploadComponent } from './components/file-upload/file-upload.component';
import { TestControlsComponent } from './components/test-controls/test-controls.component';
import { DatabaseOperationsComponent } from './components/database/database-operations.component';
import { InstanceRegistrationComponent } from './components/instance/instance-registration.component';
import { DatabaseUpgradeComponent } from './components/database/database-upgrade.component';
import { DataChannelService } from 'app/services/data-channel.service';
import { ImportDataComponent } from './components/database/import-data.component';
import { DatabaseDeleteComponent } from './components/database/database-delete.component';
import { ActionPanelComponent } from './components/action-panel/action-panel.component';

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
    AccountsHomeComponent,
    InstanceHomeComponent,
    SubscriptionComponent,
    AccountSubscriptionsComponent,
    SubscriptionDatabaseComponent,
    DatabaseConfigurationComponent,
    DatabaseTestconnectionComponent,
    SubscriptionDbHomeComponent,
    FileUploadComponent,
    TestControlsComponent,
    DatabaseOperationsComponent,
    InstanceRegistrationComponent,
    DatabaseUpgradeComponent,
    ImportDataComponent,
    DatabaseDeleteComponent,
    ActionPanelComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    RouterModule.forRoot(routes),
    HttpModule,
    ConsoleModule
    //BrowserAnimationsModule
  ],
  providers: [AuthGuardService, LoginService, ModelService, DatabaseService, UploadFileService, BackendService, DataChannelService],
  bootstrap: [AppComponent]
})
export class AppModule { }