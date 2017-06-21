import { ModelService } from './services/model.service';
import { AccountComponent } from './components/account/account.component';
import { AppComponent } from './app.component';
import { AppHomeComponent } from './components/app-home/app-home.component';
import { AuthGuardService } from './guards/auth-guard.service';
import { BrowserModule } from '@angular/platform-browser';
import { CompanyAccountComponent } from './components/company-account/company-account.component';
import { CompanyComponent } from './components/company/company.component';
import { EntityListComponent } from './components/entity-list/entity-list.component';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { JsonVisualizerPipe } from './json-visualizer.pipe';
import { LoginComponent } from './components/login/login.component';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SubscriptionHomeComponent } from './components/subscription-home/subscription-home.component';
import { routes } from './app.routes';
import { ConsoleModule } from '@taskbuilder/console';
import { LoginService } from './services/login.service';
import { AccountListComponent } from './components/account/account-list/account-list.component';
import { AccountItemComponent } from './components/account/account-item/account-item.component';
import { DatabaseInfoComponent } from './components/database-info/database-info.component';

@NgModule({
  declarations: [
    AppComponent,
    JsonVisualizerPipe,
    SubscriptionHomeComponent,
    AppHomeComponent,
    LoginComponent,
    EntityListComponent,
    CompanyComponent,
    AccountComponent,
    CompanyAccountComponent,
    AccountListComponent,
    AccountItemComponent,
    DatabaseInfoComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    RouterModule.forRoot(routes),
    HttpModule,
    ConsoleModule
  ],
  providers: [AuthGuardService, LoginService, ModelService],
  bootstrap: [AppComponent]
})
export class AppModule { }