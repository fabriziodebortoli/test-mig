import { NgModule, ModuleWithProviders } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule } from '@angular/router';

import { ConnectionInfoComponent } from './connection-info/connection-info.component';
import { ProductInfoComponent } from './product-info/product-info.component';
import { ThemeChangerComponent } from './theme-changer/theme-changer.component';
import { SettingsContainerComponent, SettingsContainerFactoryComponent } from './settings-container/settings-container.component';
export * from './settings-container/settings-container.component';

import { TbSharedModule } from './../shared/shared.module';
import { TbMenuModule } from './../menu/menu.module';
import { TbIconsModule } from '@taskbuilder/icons';

@NgModule({
  imports: [
    CommonModule,
    TbSharedModule,
    TbMenuModule,
    TbIconsModule,
    RouterModule.forChild([
      { path: 'settings', component: SettingsContainerFactoryComponent },
    ]),
  ],
  exports: [
    SettingsContainerComponent,
    SettingsContainerFactoryComponent
  ],
  declarations: [
    SettingsContainerComponent, SettingsContainerFactoryComponent,
    ProductInfoComponent, ThemeChangerComponent, ConnectionInfoComponent
  ],
  entryComponents: [SettingsContainerComponent],
})
export class TbSettingsModule { }
