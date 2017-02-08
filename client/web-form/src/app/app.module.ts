
import { appRouting } from './applications/app.routing';
import { ConnectionInfoDialogComponent } from './menu/components/menu/connection-info-dialog/connection-info-dialog.component';
import { ProductInfoDialogComponent } from './menu/components/menu/product-info-dialog/product-info-dialog.component';
import { EventManagerService } from './menu/services/event-manager.service';
import { LocalizationService } from './menu/services/localization.service';
import { SettingsService } from './menu/services/settings.service';
import { HttpMenuService } from './menu/services/http-menu.service';
import { ImageService } from './menu/services/image.service';
import { MenuService } from './menu/services/menu.service';
import { NgModule } from '@angular/core';
import { HttpModule } from '@angular/http';
import { FormsModule } from '@angular/forms';
import { BrowserModule } from '@angular/platform-browser';

import { MaterialModule } from '@angular/material';

import { CoreModule } from './core/core.module';
import { SharedModule } from './shared/shared.module';
import { MenuModule } from './menu/menu.module';

import { SidenavService } from './core/sidenav.service';

import { routing } from './app.routing';

import { HomeComponent, HomeSidenavComponent } from './home';
import { UnsupportedFactoryComponent, UnsupportedComponent } from './unsupported.component';
import { AppComponent } from './app.component';
import { DataServiceComponent } from './applications/test/data-service/data-service.component';

@NgModule({
  declarations: [
    AppComponent,
    HomeComponent, HomeSidenavComponent,
    UnsupportedFactoryComponent,
    UnsupportedComponent,
    DataServiceComponent
  ],
  imports: [
    FormsModule,
    BrowserModule,
    HttpModule,
    MaterialModule.forRoot(),
    CoreModule.forRoot(),
    SharedModule,
    MenuModule.forRoot(),
    routing,
    appRouting
  ],
  bootstrap: [AppComponent],
  entryComponents: [
    UnsupportedComponent
  ]
})
export class AppModule { }
