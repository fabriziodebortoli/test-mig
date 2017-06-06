import { AccountComponent } from './account/account.component';
import { AccountService } from './services/account.service';
import { AppComponent } from './app.component';
import { AppHomeComponent } from './app-home/app-home.component';
import { AuthGuardService } from './guards/auth-guard.service';
import { BrowserModule } from '@angular/platform-browser';
import { CompanyAccountComponent } from './company-account/company-account.component';
import { CompanyComponent } from './company/company.component';
// import { ConsoleModule } from '@taskbuilder/console';
import { EntityListComponent } from './entity-list/entity-list.component';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { JsonVisualizerPipe } from './json-visualizer.pipe';
import { LoginComponent } from './login/login.component';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SubscriptionHomeComponent } from './subscription-home/subscription-home.component';
import { routes } from './app.routes';

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
    CompanyAccountComponent
  ],
  imports: [
    BrowserModule,
    FormsModule,
    RouterModule.forRoot(routes),
    HttpModule
    // ConsoleModule
  ],
  providers: [AccountService, AuthGuardService],
  bootstrap: [AppComponent]
})
export class AppModule { }
