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
import { DatabaseInfoComponent } from './components/database-info/database-info.component';
import { ModelService } from './services/model.service';
import { LoginService } from './services/login.service';

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
    DatabaseInfoComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    RouterModule.forRoot(routes),
    HttpModule,
    ConsoleModule
  ],
  providers: [ModelService, AuthGuardService, ModelService, LoginService],
  bootstrap: [AppComponent]
})
export class AppModule { }