import { ConnectionInfoComponent } from './connection-info/connection-info.component';
import { ProductInfoComponent } from './product-info/product-info.component';
import { ThemeChangerComponent } from './theme-changer/theme-changer.component';
import { RouterModule } from '@angular/router';
import { SettingsContainerComponent, SettingsContainerFactoryComponent } from './settings-container/settings-container.component';
import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { TbSharedModule } from './../shared/shared.module';
import { TbMenuModule } from './../menu/menu.module';
import { ButtonsModule } from '@progress/kendo-angular-buttons';

@NgModule({
  imports: [
    CommonModule,
    TbSharedModule,
    TbMenuModule,
    ButtonsModule,
    RouterModule.forChild([
      { path: 'settings', component: SettingsContainerFactoryComponent },
    ]),
  ],
  exports: [
    SettingsContainerComponent,
    SettingsContainerFactoryComponent
  ],
   declarations: [SettingsContainerComponent,SettingsContainerFactoryComponent,
    ProductInfoComponent,ThemeChangerComponent, ConnectionInfoComponent],
  entryComponents: [SettingsContainerComponent],
})
export class TbSettingsModule {
}
