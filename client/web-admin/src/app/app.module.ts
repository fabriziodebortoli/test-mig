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

@NgModule({
  declarations: [
    AppComponent,
    JsonVisualizerPipe,
    SubscriptionHomeComponent,
    AppHomeComponent,
    LoginComponent
  ],
  imports: [
    BrowserModule,
    BrowserAnimationsModule,
    FormsModule,
    RouterModule.forRoot(routes),
    HttpModule,
    MaterialModule.forRoot(),
    FlexLayoutModule,
    MdToolbarModule,
    MdSidenavModule
  ],
  providers: [AccountService, AuthGuardService],
  bootstrap: [AppComponent]
})
export class AppModule { }
