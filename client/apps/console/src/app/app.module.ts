import { AccountComponent } from './account/account.component';
import { AppComponent } from './app.component';
import { AppHomeComponent } from './app-home/app-home.component';
import { AuthGuardService } from './guards/auth-guard.service';
import { BrowserModule } from '@angular/platform-browser';
import { CompanyAccountComponent } from './company-account/company-account.component';
import { CompanyComponent } from './company/company.component';
import { EntityListComponent } from './entity-list/entity-list.component';
import { FormsModule } from '@angular/forms';
import { HttpModule } from '@angular/http';
import { JsonVisualizerPipe } from './json-visualizer.pipe';
import { LoginComponent } from './login/login.component';
import { NgModule } from '@angular/core';
import { RouterModule } from '@angular/router';
import { SubscriptionHomeComponent } from './subscription-home/subscription-home.component';
import { routes } from './app.routes';
import { ConsoleModule } from '@taskbuilder/console';
import { ModelService } from './services/model.service';

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
    HttpModule,
    ConsoleModule
  ],
  providers: [ModelService, AuthGuardService, ModelService],
  bootstrap: [AppComponent]
})
export class AppModule { }
