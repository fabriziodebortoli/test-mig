import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { RouterModule } from '@angular/router';
import { HttpModule } from '@angular/http';
import { AccountService } from './services/account.service';
import { AppComponent } from './app.component';
import { JsonVisualizerPipe } from './json-visualizer.pipe';
import { BrowserAnimationsModule } from '@angular/platform-browser/animations';
import { MaterialModule, MdSidenavContainer, MdSidenavModule, MdToolbarModule } from '@angular/material';
import { SubscriptionHomeComponent } from './subscription-home/subscription-home.component';
import { AppHomeComponent } from './app-home/app-home.component';
import { routes } from './app.routes';
import { LoginComponent } from './login/login.component';
import { AuthGuardService } from './guards/auth-guard.service';
import { FlexLayoutModule } from "@angular/flex-layout";
import { EntityListComponent } from './entity-list/entity-list.component';
import { CompanyComponent } from './company/company.component';
import { AccountComponent } from './account/account.component';
import { CompanyAccountComponent } from './company-account/company-account.component';
import { DividerComponent } from './divider/divider.component';

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
    DividerComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    RouterModule.forRoot(routes),
    HttpModule,
    MaterialModule,
    FlexLayoutModule,
    MdToolbarModule,
    MdSidenavModule
  ],
  providers: [AccountService, AuthGuardService],
  bootstrap: [AppComponent]
})
export class AppModule { }
